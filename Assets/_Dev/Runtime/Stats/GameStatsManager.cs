#if UNITY_EDITOR
using UnityEditor;
#endif

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
using Steamworks;
#endif

using UnityEngine;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Text;
#if DISCORD_ENABLED
using Lumpn.Discord;
#endif
using System.Collections;
using UnityEngine.Serialization;
using System.Linq;
using Lumpn.Discord.Utils;

namespace RogueWave.GameStats
{
    //
    // The GameStatusManager is a singleton responsible for managing player Stats and Achievements, as well as
    // game telemetry.
    // 
    // It is designed to work with Steamworks.NET and the Facepunch.Steamworks library for builds that will be distributed on Steam.
    // By default SteamWorks support is disabled. To enable it, define the symbol "STEAMWORKS_ENABLED" in the project settings for the Steam enabled builds, and ensure "STEAMWORKS_DISABLED" is not set (disabled will take precedent if both are set).
    //
    [DisallowMultipleComponent]
    public class GameStatsManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The scene to load when displaying stats for the player."), Scene]
        private string m_StatsScene = "RogueWave_StatsScene";
#if DISCORD_ENABLED
        [SerializeField, Tooltip("The URL of the webhook to send player stats and achievements to.")]
        [FormerlySerializedAs("webhookData")]
        WebhookData playerDataWebhook;
        [SerializeField, Tooltip("The URL of the webhook to send developer stats and achievements to.")]
        WebhookData developerDataWebhook;
#endif

        [SerializeField] private Achievement[] m_Achievements = new Achievement[0];
        [SerializeField] private IntGameStat[] m_GameStats = default;

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        [SerializeField, Foldout("Steam"), Tooltip("The Steam App ID for the game.")]
        private uint m_SteamAppId = 0;
        [SerializeField, Foldout("Steam"), Tooltip("The frequency with which stats are stored to Steam.")]
        private float m_FrequencyOfSteamStatStore = 60;

        private float m_TimeToNextSteamStatStore = 0;
#endif

        internal static bool isDirty;

        private List<Spawner> m_Spawners = new List<Spawner>();
        private float startTime;
        private float endTime;

        private static GameStatsManager m_Instance;
        public static GameStatsManager Instance {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindFirstObjectByType<GameStatsManager>();
                    if (m_Instance == null)
                    {
                        Debug.LogError("There is no GameStatsManager in the scene. Please add and configure one.");
                    }

                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(m_Instance.gameObject);
                    }
                }

                return m_Instance;
            }
        }

        public static Action<Achievement> OnAchievementUnlocked { get; internal set; }

        public static string statsScene
        {
            get
            {
                return Instance.m_StatsScene;
            }
        }


#if DISCORD_ENABLED
        WebhookData activeWebhook
        {
            get
            {
#if UNITY_EDITOR
                return developerDataWebhook;
#else
                return playerDataWebhook;  
#endif
            }
        }
#endif

        public List<Achievement> unlockedAchievements
        {
            get
            {
                List<Achievement> unlocked = new List<Achievement>();
                foreach (Achievement achievement in m_Achievements)
                {
                    if (achievement.isUnlocked)
                    {
                        unlocked.Add(achievement);
                    }
                }
                return unlocked;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            try
            {
                SteamClient.Init(m_SteamAppId, false); // false = manual callbacks (recommended in manual in the case of Unity)
                Debug.Log($"Steam ID: {SteamClient.SteamId} ({SteamClient.Name})");

                Debug.Log("Friends:");
                foreach (var player in SteamFriends.GetFriends())
                {
                    Debug.Log($"Steam ID: {player.Id} ({player.Name}) {player.Relationship}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Steamworks failed to initialize: " + e.Message);
            }
#endif
        }

        private void OnEnable()
        {
            m_Instance = this;
            m_GameStats = Resources.LoadAll<IntGameStat>("");
            m_Achievements = Resources.LoadAll<Achievement>("");
        }

        private void OnDisable()
        {
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            if (isDirty)
            {
                SteamUserStats.StoreStats();
            }
            SteamClient.Shutdown();
#endif
        }

#if DISCORD_ENABLED
        internal void SendDataToWebhook() 
        {
            if (activeWebhook == null)
            {
                return;
            }

            string[] chunks = GetDataAsYAML();
            StartCoroutine(SendDataToWebhookCoroutine(chunks));
        }

        IEnumerator SendDataToWebhookCoroutine(string[] chunks)
        {
            Webhook webhook = activeWebhook.CreateWebhook();

            Message message = new Message();
            message.username = "Rogue Wave";

            Author author = new Author();
            author.name = $"Rogue Wave v{Application.version} (Player ID {SystemInfo.deviceUniqueIdentifier.GetHashCode()})";
            
            List<Embed> embeds = new List<Embed>();
            bool isFirstEmbed = true;
            foreach (string chunk in chunks)
            {
                string[] lines = chunk.Split(new[] { '\n' }, StringSplitOptions.None);

                Embed embed = new Embed();
                embed.author = author;
                embed.title = lines[0];
                embed.description = string.Join("\n", lines.Skip(1));
                
                if (isFirstEmbed)
                {
                    isFirstEmbed = false;
                    embed.color = ColorUtils.ToColorCode(Color.green);
                }
                
                embeds.Add(embed);
            }

            //Field field = new Field();
            //field.name = "Field Name";
            //field.value = "Field Value";
            //embed.fields = new Field[] { field };

            message.embeds = embeds.ToArray();

            yield return webhook.Send(message);
        }
#endif

        private string[] GetDataAsYAML()
        {
            // OPTIMIZATION: This could be optimized by only sending the stats and achievements that have changed since the last time this was called.
            // OPTIMIZATION: This could be further optimized by only sending the stats and achievements that are not yet unlocked, i.e. once an achievement has been unlocked it can be removed from the list of achievements to send.

            List<string> chunks = new List<string>();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Summary Stats:");
            //sb.AppendLine($"  - START_TIME: {startTime}");
            //sb.AppendLine($"  - END_TIME: {endTime}");
            int totalSeconds = Mathf.RoundToInt(endTime - startTime); 
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            sb.AppendLine($"  - PLAY_TIME: {hours}:{minutes}:{seconds}");

            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Player Stats:");
            foreach (IntGameStat stat in m_GameStats)
            {
                if (stat.key != null)
                {
                    sb.Append($"  - {stat.key}: {stat.value}\n");
                }
            }

            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Achievements:");
            foreach (Achievement achievement in m_Achievements)
            {
                string status = achievement.isUnlocked ? "Unlocked" : "Locked";
                sb.AppendLine($"  - {achievement.key}: {status}");
            }
            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Score:");
            int totalScore = 0;
            foreach (IntGameStat stat in m_GameStats)
            {
                if (stat.contributeToScore)
                {
                    int score = stat.ScoreContribution;
                    totalScore += score;
                    sb.AppendLine($"  - {stat.key}: {score}");
                }
            }
            sb.AppendLine($"  - Total Score: {totalScore}");
            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Performance Stats:");
            FPSCounter fps = FindObjectOfType<FPSCounter>();
            if (fps != null)
            {
                sb.AppendLine($"  - AVERAGE_FPS: {fps.averageFPS}");
                sb.AppendLine($"  - MIN_FPS: {fps.minFPS}");
                sb.AppendLine($"  - MAX_FPS: {fps.maxFPS}");
            }
            else
            {
                sb.AppendLine("No FPS Counter found.");
            }
            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Machine Stats:");
            sb.AppendLine($"  - UNIQUE_DEVICE_ID_HASH: {SystemInfo.deviceUniqueIdentifier.GetHashCode()}"); // Use a hash of the device ID to avoid sending the actual device ID which could be used to track a user.
            sb.AppendLine($"  - OS: {SystemInfo.operatingSystem}");
            sb.AppendLine($"  - CPU: {SystemInfo.processorType}");
            sb.AppendLine($"  - GPU: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"  - RAM: {SystemInfo.systemMemorySize} MB");
            sb.AppendLine($"  - DEVICE_MODEL: {SystemInfo.deviceModel}");
            sb.AppendLine($"  - DEVICE_TYPE: {SystemInfo.deviceType}");
            sb.AppendLine($"  - GRAPHICS_API: {SystemInfo.graphicsDeviceType}");
            sb.AppendLine($"  - SCREEN_RESOLUTION: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
            sb.AppendLine($"  - SCREEN_DPI: {Screen.dpi}");
            sb.AppendLine($"  - FULL_SCREEN: {Screen.fullScreen}");
            sb.AppendLine($"  - VSYNC: {QualitySettings.vSyncCount}");
            sb.AppendLine($"  - QUALITY_LEVEL: {QualitySettings.GetQualityLevel()}");
            sb.AppendLine($"  - TARGET_FRAME_RATE: {Application.targetFrameRate}");
            sb.AppendLine($"  - PLATFORM: {Application.platform}");
            sb.AppendLine($"  - LANGUAGE: {Application.systemLanguage}");
            sb.AppendLine($"  - LOCAL_TIME: {DateTime.Now}");
            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Build:");
            sb.AppendLine($"  - NAME: {Application.productName}");
            sb.AppendLine($"  - VERSION: {Application.version}");
            sb.AppendLine($"  - BUILD_GUID: {Application.buildGUID}");
            sb.AppendLine($"  - GENUINE_CHECK_AVAILABLE: {Application.genuineCheckAvailable}");
            sb.AppendLine($"  - GENUINE: {Application.genuine}");
            chunks.Add(sb.ToString());

            // chunks.Add(GameLog.ToYAML());

            return chunks.ToArray();
        }

        public IntGameStat GetStat(string key)
        {
            // OPTIMIZATION: This would be faster if it were a HashSet
            foreach (IntGameStat stat in m_GameStats)
            {
                if (stat.key == key)
                {
                    return stat;
                }
            }

            return null;
        }

        private void Update()
        {
            if (startTime == 0)
            {
                startTime = Time.time;
            } else
            {
                endTime = Time.time;
            }

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            m_TimeToNextSteamStatStore -= Time.deltaTime;
            if (isDirty && m_TimeToNextSteamStatStore > 0)
            {
                if (SteamUserStats.StoreStats())
                {
                    isDirty = false;
                    m_TimeToNextSteamStatStore = m_FrequencyOfSteamStatStore;
                }
            }

            SteamClient.RunCallbacks();
#endif
        }

        internal void CheckAchievements(IntGameStat stat, int value)
        {
            // OPTIMIZATION: This could be optimized by only checking achievements that are related to the stat that has changed. i.e. sort the achievements into a dictionary by stat and only check the relevant ones.
            // OPTIMIZATION: This could be further optimized by only checking achievements that are not yet unlocked, i.e. once an achievement has been unlocked it can be removed from the list of achievements to check.
            foreach (Achievement achievement in m_Achievements)
            {
                if (!achievement.isUnlocked && achievement.stat == stat)
                {
                    if (value >= achievement.targetValue)
                    {
                        UnlockAchievement(achievement);
                    }
                }
            }
        }

        internal void CheckAchievements(IntGameStat stat, float value)
        {
            // OPTIMIZATION: This could be optimized by only checking achievements that are related to the stat that has changed. i.e. sort the achievements into a dictionary by stat and only check the relevant ones.
            // OPTIMIZATION: This could be further optimized by only checking achievements that are not yet unlocked, i.e. once an achievement has been unlocked it can be removed from the list of achievements to check.
            foreach (Achievement achievement in m_Achievements)
            {
                if (!achievement.isUnlocked && achievement.stat == stat)
                {
                    if (value >= achievement.targetValue)
                    {
                        UnlockAchievement(achievement);
                    }
                }
            }
        }

        private static void UnlockAchievement(Achievement achievement)
        {
            achievement.Unlock();
            OnAchievementUnlocked?.Invoke(achievement);
        }

        [Button("Reset Stats and Achievements (Play mode only)"), ShowIf("showDebug")]
        internal static void ResetStats()
        {
            ResetLocalStatsAndAchievements();
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            ResetSteamStats();
#endif
            Debug.Log("Stats and achievements reset.");
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Rogue Wave/Data/Destructive/Reset Stats and Achievements")]
#endif
        public static void ResetLocalStatsAndAchievements()
        {
            IntGameStat[] gameStats = Resources.LoadAll<IntGameStat>("");
            foreach (IntGameStat stat in gameStats)
            {
                stat.SetValue(stat.defaultValue);
            }

            Achievement[] achievements = Resources.LoadAll<Achievement>("");
            foreach (Achievement achievement in achievements)
            {
                achievement.Reset();
            }
        }

        #region EDITOR_ONLY
#if UNITY_EDITOR
        [HorizontalLine(color: EColor.Blue)]
        [SerializeField]
        #pragma warning disable CS0414 // used in Button attribute
        bool showDebug = false;
#pragma warning restore CS0414

        [Button("Dump Stats and Achievements to Console"), ShowIf("showDebug")]
        private void DumpStatsAndAchievements()
        {
            IntGameStat[] gameStats = m_GameStats;

            foreach (IntGameStat stat in gameStats)
            {
                if (stat.key != null)
                {
                    Debug.Log($"Scriptable Object: {stat.key} = {stat.value}");
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
                    DumpSteamStat(stat);
#endif
                }
            }

            foreach (Achievement achievement in m_Achievements)
            {
                if (achievement.isUnlocked)
                {
                    Debug.Log($"Scriptable Object: {achievement.key} = unlocked at {achievement.timeOfUnlock}");
                }
                else
                {
                    Debug.Log($"Scriptable Object: {achievement.key} = locked");
                }
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
                DumpSteamAchievement(achievement);
#endif
            }
        }

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        [Button("Disable Steamworks")]
        private void DisableSteamworks()
        {
            // Remove the STEAMWORKS_ENABLED symbol to the project settings
            PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] defines);

            defines = defines.Length > 0 ? Array.FindAll(defines, s => s != "STEAMWORKS_ENABLED") : new string[] { };
            
            Array.Resize(ref defines, defines.Length + 1);
            defines[defines.Length - 1] = "STEAMWORKS_DISABLED";
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
            AssetDatabase.Refresh();
        }

        private void ResetSteamStats()
        {
            SteamUserStats.ResetAll(true); // true = wipe achivements too
            SteamUserStats.StoreStats();
            SteamUserStats.RequestCurrentStats();

            Debug.Log("Steam Stats and achievements reset.");
        }

        private void DumpSteamStat(GameStat stat)
        {
            switch (stat.type)
            {
                case GameStat.StatType.Int:
                    Debug.Log($"Steam: {stat.key} = {SteamUserStats.GetStatInt(stat.key)}");
                    break;
                case GameStat.StatType.Float:
                    Debug.Log($"Steam: {stat.key} = {SteamUserStats.GetStatFloat(stat.key)}");
                    break;
            }
        }

        private void DumpSteamAchievement(Achievement achievement)
        {
            foreach (Steamworks.Data.Achievement steamAchievement in SteamUserStats.Achievements)
            {
                if (steamAchievement.Identifier == achievement.key)
                {
                    string state = steamAchievement.State ? "Unlocked" : "Locked";
                    Debug.Log($"Steam: {achievement.key} = {state}");
                    return;
                }
            }

            Debug.LogError($"Steam: {achievement.name} = Not found");
        }
#else
        [Button("Enable Steamworks"), ShowIf("showDebug")]
        private void EnableSteamworks()
        {
            PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] defines);
            
            defines = defines.Length > 0 ? Array.FindAll(defines, s => s != "STEAMWORKS_DISABLED") : new string[] { };

            Array.Resize(ref defines, defines.Length + 1);
            defines[defines.Length - 1] = "STEAMWORKS_ENABLED";

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
            AssetDatabase.Refresh();
        }
#endif

        [Button]
        private void SendTestMessage()
        {
            SendDataToWebhook();    
        }
#endif
#endregion
    }

    [Serializable]
    internal struct ScoreCallculation
    {
        [SerializeField, Tooltip("The stat this scord caclulation is based on.")]
        internal IntGameStat stat;
        [SerializeField, Tooltip("The number of points per unit of the stat.")]
        internal int pointsPerUnit;
    }
}
