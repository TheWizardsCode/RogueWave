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

        private Achievement[] m_Achievements = new Achievement[0];

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
                        m_Instance = new GameObject("Game Stat Manager").AddComponent<GameStatsManager>();
                    }

                    DontDestroyOnLoad(m_Instance.gameObject);
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

            m_Achievements = Resources.LoadAll<Achievement>("");

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

            webhook.Send($"\n\n\n\nGame Stats Data for {SystemInfo.deviceUniqueIdentifier.GetHashCode()}\n\n\n\n");

            foreach (string chunk in chunks)
            {
                if (chunk.Length > 2000)
                {
                    Debug.LogWarning("Data chunk too large to send to webhook. Data will be split across messages. Need a proper way to get the logs.");
                    string[] lines = chunk.Split('\n');
                    StringBuilder sb = new StringBuilder();
                    foreach (string line in lines)
                    {
                        if (sb.Length > 1800 && line.Trim().StartsWith('-'))
                        {
                            StartCoroutine(webhook.Send($"```yaml\n# Partial\n{sb}```"));
                            yield return new WaitForSeconds(0.5f);
                            sb.Clear();
                        }
                        sb.Append(line);
                    }

                    StartCoroutine(webhook.Send($"```yaml\n# Partial (Last)\n{sb}```"));
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    StartCoroutine(webhook.Send($"```yaml\n{chunk}```"));
                    yield return new WaitForSeconds(0.5f);
                }
            }

            webhook.Send($"\n\n\n\nEnd of data for {SystemInfo.deviceUniqueIdentifier.GetHashCode()}\n\n\n\n");
        }
#endif

        private string[] GetDataAsYAML()
        {// send a summary of the stats and achevements to a webhook
         // OPTIMIZATION: This could be optimized by only sending the stats and achievements that have changed since the last time this was called.
         // OPTIMIZATION: This could be further optimized by only sending the stats and achievements that are not yet unlocked, i.e. once an achievement has been unlocked it can be removed from the list of achievements to send.

            List<string> chunks = new List<string>();

            StringBuilder sb = new StringBuilder();
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
            sb.AppendLine("Gameplay Stats:");
            sb.AppendLine($"  - START_TIME: {startTime}");
            sb.AppendLine($"  - END_TIME: {endTime}");
            sb.AppendLine($"  - PLAY_TIME: {endTime - startTime}");
            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Player Stats:");
            foreach (GameStat stat in Resources.LoadAll<GameStat>(""))
            {
                if (stat.key != null)
                {
                    switch (stat.type)
                    {
                        case GameStat.StatType.Int:
                            sb.Append($"  - {stat.key}: {stat.GetIntValue()}\n");
                            break;
                        case GameStat.StatType.Float:
                            sb.Append($"  - {stat.key}: {stat.GetFloatValue()}\n");
                            break;
                    }
                }
            }

            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Score:");
            int totalScore = 0;
            GameStat[] stats = Resources.LoadAll<GameStat>("");
            foreach (GameStat stat in stats)
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
            sb.AppendLine("Achievements:");
            foreach (Achievement achievement in m_Achievements)
            {
                string status = achievement.isUnlocked ? "Unlocked" : "Locked";
                sb.AppendLine($"  - {achievement.key}: {status}");
            }

            chunks.Add(sb.ToString());

            chunks.Add(GameLog.ToYAML());

            return chunks.ToArray();
        }

        internal GameStat GetStat(string key)
        {
            GameStat[] stats = Resources.LoadAll<GameStat>("");
            foreach (GameStat stat in stats)
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

        /// <summary>
        /// Increments an integer counter by 1.
        /// </summary>
        /// <param name="stat"></param>
        internal void IncrementCounter(GameStat stat)
        {
            IncrementCounter(stat, 1);
        }

        /// <summary>
        /// Increments an integer counter by a set amount.
        /// </summary>
        /// <param name="stat"></param>
        internal void IncrementCounter(GameStat stat, int amount)
        {
            int value = stat.Increment(amount);
            CheckAchievements(stat, value);

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            if (!SteamClient.IsValid)
                return;

            SteamUserStats.AddStat(stat.key, amount);
            isDirty = true;
#endif
        }

        internal void CheckAchievements(GameStat stat, int value)
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

        internal void CheckAchievements(GameStat stat, float value)
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Button("Reset Stats and Achievements (Play mode only)"), ShowIf("showDebug")]
        internal static void ResetStats()
        {
            if (Application.isPlaying)
            {
                ResetLocalStatsAndAchievements();
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
                ResetSteamStats();
#endif
                Debug.Log("Stats and achievements reset.");
            }
            else
            {
                Debug.LogError("You can only reset Steam stats and achievements in play mode.");
            }
        }

        [MenuItem("Tools/Rogue Wave/Data/Destructive/Reset Stats and Achievements")]
#endif
        public static void ResetLocalStatsAndAchievements()
        {
            GameStat[] gameStats = Resources.LoadAll<GameStat>("");
            foreach (GameStat stat in gameStats)
            {
                stat.Reset();
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
            if (Application.isPlaying)
            {
                GameStat[] gameStats = Resources.LoadAll<GameStat>("");

                foreach (GameStat stat in gameStats)
                {
                    if (stat.key != null)
                    {
                        switch (stat.type)
                        {
                            case GameStat.StatType.Int:
                                Debug.Log($"Scriptable Object: {stat.key} = {stat.GetIntValue()}");
                                break;
                            case GameStat.StatType.Float:
                                Debug.Log($"Scriptable Object: {stat.key} = {stat.GetFloatValue()}");
                                break;
                        }
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
                        DumpSteamStat(stat);
#endif
                    }
                }

                Achievement[] achievements = Resources.LoadAll<Achievement>("");
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
            else
            {
                Debug.LogError("You can only dump stats and achievements in play mode.");
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
#endif
#endregion
    }

    [Serializable]
    internal struct ScoreCallculation
    {
        [SerializeField, Tooltip("The stat this scord caclulation is based on.")]
        internal GameStat stat;
        [SerializeField, Tooltip("The number of points per unit of the stat.")]
        internal int pointsPerUnit;
    }
}
