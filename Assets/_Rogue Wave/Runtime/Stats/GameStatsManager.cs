#if UNITY_EDITOR
using UnityEditor;
#endif

// REFACTOR: Move the steam specific code to SteamworksController
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
using HeathenEngineering.SteamworksIntegration.API;
using StatsClient = HeathenEngineering.SteamworksIntegration.API.StatsAndAchievements.Client;
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
using static RogueWave.GameStats.Achievement;

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
        const string SORRA_THE_WIZARDS_CODE_DEVICE_ID = "4c8db7071bbe6233b16e57e563033f62f451fab1";

        [SerializeField, Tooltip("The scene to load when displaying stats for the player."), Scene]
        private string m_StatsScene = "RogueWave_StatsScene";
#if DISCORD_ENABLED
        [SerializeField, Tooltip("The URL of the webhook to send Sorra's data log to.")]
        WebhookData exceptionWebhook;
        [SerializeField, Tooltip("The URL of the webhook to send real player data log to.")]
        [FormerlySerializedAs("webhookData")]
        WebhookData playerDataWebhook;
        [SerializeField, Tooltip("The URL of the webhook to send developer data log to.")]
        WebhookData developerDataWebhook;
        [SerializeField, Tooltip("The URL of the webhook to send Sorra's data log to.")]
        WebhookData sorraDataWebhook;
#endif

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        [SerializeField, BoxGroup("Steam"), Tooltip("The frequency with which stats are stored to Steam.")]
        private float m_FrequencyOfSteamStatStore = 60;

        private float m_TimeToNextSteamStatStore = 0;
#endif
        [InfoBox("The following fields are populated automatically by the GameStatsManager. Provided in Read Only for debug purposes.")]
        [SerializeField, ReadOnly] private Achievement[] m_Achievements = new Achievement[0];
        [SerializeField, ReadOnly] private IntGameStat[] m_IntGameStats = default;
        [SerializeField, ReadOnly] private StringGameStat[] m_StringGameStats = default;

        internal static bool isDirty;

        private List<Spawner> m_Spawners = new List<Spawner>();
        private static float endTime;

        private static GameStatsManager m_Instance;
        public static GameStatsManager Instance {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindFirstObjectByType<GameStatsManager>();
                    if (m_Instance == null)
                    {
                        m_Instance = new GameObject("Game Stats Manager (Dynamically Created)").AddComponent<GameStatsManager>();
                    }

                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(m_Instance.gameObject);
                    }
                }

                return m_Instance;
            }
        }

        public static string statsScene
        {
            get
            {
                return Instance.m_StatsScene;
            }
        }

        public Achievement[] Achievements => m_Achievements;

        Dictionary<Category, List<Achievement>> m_CategorizedAchievements = new Dictionary<Category, List<Achievement>>();

#if DISCORD_ENABLED
        WebhookData activeWebhook
        {
            get
            {
                if (SystemInfo.deviceUniqueIdentifier == SORRA_THE_WIZARDS_CODE_DEVICE_ID)
                {
                    return sorraDataWebhook;
                }
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
        }

        private void OnEnable()
        {
            m_Instance = this;
            m_IntGameStats = Resources.LoadAll<IntGameStat>("");
            m_StringGameStats = Resources.LoadAll<StringGameStat>("");
            BuildAchievementCache();

#if DEVELOPMENT_BUILD
            Application.logMessageReceived += HandleLog;
#endif
        }

        private void BuildAchievementCache()
        {
            m_Achievements = Resources.LoadAll<Achievement>("");
            Array.Sort(m_Achievements, (x, y) =>
            {
#if DEMO
                if (x.isDemoLocked && !y.isDemoLocked)
                {
                    return 1;
                }
                if (!x.isDemoLocked && y.isDemoLocked)
                {
                    return -1;
                }
#endif

                int categoryComparison = x.category.CompareTo(y.category);
                if (categoryComparison != 0)
                {
                    return categoryComparison;
                }

                int statComparison = 0;
                if (x.stat != null && y.stat != null)
                {
                    statComparison = x.stat.CompareTo(y.stat);
                }
                else if (x.stat == null && y.stat != null)
                {
                    statComparison = -1;
                }
                else if (x.stat != null && y.stat == null)
                {
                    statComparison = 1;
                }

                if (statComparison != 0)
                {
                    return statComparison;
                }

                int targetValueComparison = x.targetValue.CompareTo(y.targetValue);
                if (targetValueComparison != 0)
                {
                    return targetValueComparison;
                }

                return x.displayName.CompareTo(y.displayName);
            });

            m_NotDemoLocked = Achievements.Where(a => !a.isDemoLocked && a.Validate(out string _)).ToArray();
            m_DemoLocked = Achievements.Where(a => a.isDemoLocked && a.Validate(out string _)).ToArray();

            foreach (Category category in Enum.GetValues(typeof(Category)))
            {
                List<Achievement> achievements = new List<Achievement>();
                foreach (Achievement achievement in Achievements)
                {
                    if (achievement.category == category)
                    {
                        achievements.Add(achievement);
                    }
                }
                m_CategorizedAchievements.Add(category, achievements);
            }
        }

        private void OnDisable()
        {
#if DEVELOPMENT_BUILD
            Application.logMessageReceived -= HandleLog;
#endif
        }

        internal void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                SendExceptionToWebhook(logString, stackTrace);
            }
        }

        public void SendExceptionToWebhook(Exception exception)
        {
            StartCoroutine(SendExceptionToWebhookCoroutine(exception.Message, exception.StackTrace));
        }

        public void SendExceptionToWebhook(string logString, string stackTrace)
        {
            StartCoroutine(SendExceptionToWebhookCoroutine(logString, stackTrace));
        }

#if DISCORD_ENABLED
        /// <summary>
        /// Send the stats to the discord server. The `eventName` is the name of the event that triggered the sending of the stats and will be included in the message.
        /// </summary>
        /// <param name="eventName"></param>
        internal void SendDataToWebhook(string eventName) 
        {
            if (activeWebhook == null)
            {
                return;
            }

            string[] chunks = GetDataAsYAML();
            StartCoroutine(SendDataToWebhookCoroutine(eventName, chunks));
        }

        IEnumerator SendExceptionToWebhookCoroutine(string logString, string stackTrace)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(logString + "\n\n" + stackTrace);
#endif

            string playerID = SystemInfo.deviceUniqueIdentifier.GetHashCode().ToString();
            if (SystemInfo.deviceUniqueIdentifier == SORRA_THE_WIZARDS_CODE_DEVICE_ID)
            {
                playerID = "Sorra The Orc";
            }

            Message message = new Message();
            message.username = "Rogue Wave";

            Author author = new Author();
            author.name = $"Rogue Wave (Exception Report, Player ID {playerID}) v{Application.version}";

            //List<Embed> embeds = new List<Embed>();
            //Embed embed = new Embed();
            //embed.author = author;
            //embed.title = logString;
            //embed.description = stackTrace;
            //embed.color = ColorUtils.ToColorCode(Color.red);
            //embeds.Add(embed);
            //message.embeds = embeds.ToArray();

            message.content = $"Exception Report\nPlayer ID {playerID}\nv{Application.version}\n\n{logString}\n\n";
            if (stackTrace.Length + message.content.Length + 6 > 2000)
            {
                message.content += $"```{stackTrace.Substring(0, 2000 - message.content.Length - 6)}```";
            }
            else
            {
                message.content += $"```{stackTrace}```";
            }

            Webhook webhook = exceptionWebhook.CreateWebhook();
            yield return webhook.Send(message);
        }

        IEnumerator SendDataToWebhookCoroutine(string eventName, string[] chunks)
        {
            Webhook webhook = activeWebhook.CreateWebhook();

            Message message = new Message();
            message.username = "Rogue Wave";

            Author author = new Author();
            author.name = $"Rogue Wave ({eventName}, Player ID {SystemInfo.deviceUniqueIdentifier.GetHashCode()}) v{Application.version}";
            
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
            int totalSeconds = Mathf.RoundToInt(endTime);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            sb.AppendLine($"  - Session time: {hours}:{minutes}:{seconds}");
            sb.AppendLine($"  - TOTAL_TIME_IN_RUNS {GameStatsManager.Instance.GetIntStat("TOTAL_TIME_IN_RUNS".ToString())}");
            sb.AppendLine($"  - RUNS_STARTED: {GameStatsManager.Instance.GetIntStat("RUNS_STARTED").ToString()}");
            sb.AppendLine($"  - RUNS_COMPLETED: {GameStatsManager.Instance.GetIntStat("RUNS_COMPLETED").ToString()}");
            sb.AppendLine($"  - DEATH_COUNT: {GameStatsManager.Instance.GetIntStat("DEATH_COUNT").ToString()}");
            sb.AppendLine($"  - MAX_NANOBOT_LEVEL: {GameStatsManager.Instance.GetIntStat("MAX_NANOBOT_LEVEL").ToString()}");
            sb.AppendLine($"  - RESOURCES_SPENT_IN_RUNS: {GameStatsManager.Instance.GetIntStat("RESOURCES_SPENT_IN_RUNS").ToString()}");
            sb.AppendLine($"  - RUN_LOG: {GameStatsManager.Instance.GetStringStat("RUN_LOG").ToString()}");
            
            FPSCounter fps = FindObjectOfType<FPSCounter>();
            if (fps != null)
            {
                sb.AppendLine(fps.ToYAML());
            }
            
            sb.AppendLine($"  - CPU: {SystemInfo.processorType}");
            sb.AppendLine($"  - GPU: {SystemInfo.graphicsDeviceName}");

            chunks.Add(sb.ToString());

            sb.Clear();
            sb.AppendLine("Player Stats:");
            foreach (IntGameStat stat in m_IntGameStats)
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
            foreach (IntGameStat stat in m_IntGameStats)
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
            if (fps != null)
            {
                sb.AppendLine(fps.ToYAML()); 
                sb.AppendLine($"  - SCREEN_RESOLUTION: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
                sb.AppendLine($"  - CPU: {SystemInfo.processorType}");
                sb.AppendLine($"  - GPU: {SystemInfo.graphicsDeviceName}");

                chunks.Add(sb.ToString());
            }

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

        public IntGameStat GetIntStat(string key)
        {
            // OPTIMIZATION: This would be faster if it were a HashSet
            foreach (IntGameStat stat in m_IntGameStats)
            {
                if (stat.key == key)
                {
                    return stat;
                }
            }

            return null;
        }

        public StringGameStat GetStringStat(string key)
        {
            // OPTIMIZATION: This would be faster if it were a HashSet
            foreach (StringGameStat stat in m_StringGameStats)
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
            endTime = Time.realtimeSinceStartup;

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            m_TimeToNextSteamStatStore -= Time.deltaTime;
            if (isDirty && m_TimeToNextSteamStatStore > 0)
            {
                if (StatsAndAchievements.Client.StoreStats())
                {
                    isDirty = false;
                    m_TimeToNextSteamStatStore = m_FrequencyOfSteamStatStore;
                }
            }
#endif
        }

        /// <summary>
        /// Resets all stats to their default values. Locks all achievements.
        /// </summary>
        [Button("Reset Stats and Achievements"), ShowIf("showDebug")]
        internal void ResetStats()
        {
            ResetLocalStatsAndAchievements();
            Debug.Log("Stats and achievements reset.");
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Rogue Wave/Data/Destructive/Reset Stats and Achievements")]
#endif
        public static void ResetLocalStatsAndAchievements()
        {
            IntGameStat[] intGameStats = Resources.LoadAll<IntGameStat>("");
            foreach (IntGameStat stat in intGameStats)
            {
                stat.SetValue(stat.defaultValue);
            }

            StringGameStat[] strGameStats = Resources.LoadAll<StringGameStat>("");
            foreach (StringGameStat stat in strGameStats)
            {
                stat.SetValue(stat.defaultValue);
            }

            Achievement[] achievements = Resources.LoadAll<Achievement>("");
            foreach (Achievement achievement in achievements)
            {
                achievement.Reset();
            }
        }

        /// <summary>
        /// Get all the achievements in a given category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal List<Achievement> AllAchievementsInCategory(Category category)
        {
            return m_CategorizedAchievements.GetValueOrDefault(category);
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
            IntGameStat[] gameStats = m_IntGameStats;

            foreach (IntGameStat stat in gameStats)
            {
                if (stat.key != null)
                {
                    Debug.Log($"Scriptable Object: {stat.key} = {stat.value}");
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
                    DumpSteamStatInt(stat);
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
                DumpSteamAchievement(achievement.key);
#endif
            }
        }

        private Achievement[] m_NotDemoLocked;
        /// <summary>
        /// Get a sorted list of all the achievements that are not demo locked
        /// </summary>
        /// <seealso cref="Achievements"/>
        /// <seealso cref="DemoLockedAchievements"/>
        public Achievement[] NotDemoLockedAchievements => m_NotDemoLocked;

        private Achievement[] m_DemoLocked;
        /// <summary>
        /// Get a sorted list of all the achievements that are demo locked
        /// </summary>
        /// <seealso cref="Achievements"/>
        /// <seealso cref="NotDemoLockedAchievements"/>
        public Achievement[] DemoLockedAchievements => m_DemoLocked;

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        private void ResetSteamStats()
        {
            StatsClient.ResetAllStats(true); // true = wipe achivements too

            Debug.Log("Steam Stats and achievements reset.");
        }

        private void DumpSteamStatInt(GameStat<int> stat)
        {
            if (StatsClient.GetStat(stat.key, out int value))
            {
                Debug.Log($"Steam: {stat.key} = {value}");
            } 
            else
            {
                Debug.LogError($"Steam: {stat.key} = Not found");
            }
        }

        private void DumpSteamAchievement(string achievementName)
        {
            bool achieved;
            DateTime unlockTime;
            if (StatsClient.GetAchievement(achievementName, out achieved, out unlockTime))
            {
                string state = achieved ? "Unlocked" : "Locked";
                Debug.Log($"Steam: {achievementName} = {state}");
            }
            else
            {
                Debug.LogError($"Steam: {achievementName} = Not found");
            }
        }
#endif // steamworks
#endif // editor
        #endregion
    }

    [Serializable]
    internal struct ScoreCallculation
    {
        [SerializeField, Tooltip("The stat this score caclulation is based on.")]
        internal IntGameStat stat;
        [SerializeField, Tooltip("The number of points per unit of the stat.")]
        internal int pointsPerUnit;
    }
}
