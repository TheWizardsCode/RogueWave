using NaughtyAttributes;
using RogueWave.GameStats;
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
using HeathenEngineering.SteamworksIntegration.API;
using HeathenEngineering.SteamworksIntegration;
using Screenshots = HeathenEngineering.SteamworksIntegration.API.Screenshots.Client;
#endif
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class SteamworksController : MonoBehaviour
    {
#if STEAMWORKS_DISABLED && !STEAMWORKS_ENABLED
        [InfoBox("Steamworks is enabled. You can enable it by clicking disable button in the management section below.")]
#else
        [InfoBox("Steamworks is enabled. You can disable it by clicking the disable button in the management section below.")]
#endif

        [SerializeField, Tooltip("Show the management buttons for enabling and disabling Steamworks."), BoxGroup("Management")]
        private bool showManagement = false;
#if BUILD_DEMO && STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        [InfoBox("The build system is currently setup to build the demo version of the game. If you want to build the full version of the game click the Build Full Game button in the management section below.")]
        [SerializeField, Tooltip("The Steam Demo App Settings for Steamworks integration."), Expandable, Required, BoxGroup("Steam")]
        private SteamSettings m_SteamDemoAppSettings;
#endif

#if !BUILD_DEMO && STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        [InfoBox("The build system is currently setup to build the Full version version of the game. If you want to build the full version of the game click the Build Demo button in the management section below.")]
        [SerializeField, Tooltip("The Steam Main App settings for Steamworks integration."), Expandable, Required, BoxGroup("Steam")]
        private SteamSettings m_SteamMainAppSettings;
#endif

#if UNITY_EDITOR && STEAMWORKS_DISABLED && !STEAMWORKS_ENABLED
        [Button("Enable Steamworks", EButtonEnableMode.Editor), ShowIf("showManagement")]
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

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        [SerializeField, Tooltip("Shuld Steam automatically take screenshots when a screenshot event is received."), BoxGroup("Screenshots")]
        bool m_AutomaticScreenshots = false;
        [SerializeField, Tooltip("The GameEvent to use to trigger a screenshot."), BoxGroup("Screenshots"), ShowIf("m_AutomaticScreenshots")]
        private GameEvent m_ScreenshotEvent = null;
        [SerializeField, Tooltip("The minimum number of screenshot events needed before a screenshot is taken. That is, if this is set to 5 then a screenshot will only be taken if 5 screenshot events have been fired in the time window (defined below)."), BoxGroup("Screenshots"), ShowIf("m_AutomaticScreenshots")]
        private int m_MinScreenshotEventsNeeded = 5;
        [SerializeField, Tooltip("The time window in which the minimum number of screenshot events must occur before a screenshot is taken. Once this time has passed, without a new event, the count will be reset to zero."), BoxGroup("Screenshots"), ShowIf("m_AutomaticScreenshots")]
        private float m_ScreenshotEventTimeWindow = 1.0f;
        [SerializeField, Tooltip("The number of screenshots to take when a screenshot is triggered."), BoxGroup("Screenshots"), ShowIf("m_AutomaticScreenshots")]
        int m_NumberOfSscreenhots = 1;
        [SerializeField, Tooltip("The delay between taking screenshots."), BoxGroup("Screenshots"), ShowIf("m_AutomaticScreenshots")]
        float delayBetweenShots = 0.25f;
        [SerializeField, Tooltip("The cooldown period after taking a screenshot before another can be taken."), BoxGroup("Screenshots"), ShowIf("m_AutomaticScreenshots")]
        float cooldown = 1.0f;

        private static SteamworksController m_Instance;
        private SteamSettings m_SteamSettings;

        private int screenshotEventCount = 0;
        private bool canTakeScreenshot = true;
        private float lastScreenshotEventTime = 0f;

        public static SteamworksController Instance
        {
            get => m_Instance;
            set => m_Instance = value;
        }
        public string PlayerName {
            get
            {
                if (PlayerSteamId == null)
                {
                    return "Unknown";
                }
                return PlayerSteamId.Name;
            }
        }
        public UserData PlayerSteamId { get => User.Client.Id; }
        public bool ConnectedToSteam { get; private set; }

        public void Awake()
        {
#endif

#if BUILD_DEMO && STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            m_SteamSettings = m_SteamDemoAppSettings;
#endif

#if !BUILD_DEMO && STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            m_SteamSettings = m_SteamMainAppSettings;
#endif

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
                try
                {
                    m_SteamSettings.Initialize();
                    ConnectedToSteam = true;
                }
                catch (Exception e)
                {
                    ConnectedToSteam = false;
                    GameStatsManager.Instance.SendExceptionToWebhook("Steamworks failed to initialize: " + e.Message, e.StackTrace);
                }
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            if (m_AutomaticScreenshots && m_ScreenshotEvent != null)
            {
                m_ScreenshotEvent.RegisterListener(OnScreenshotEventReceived);
            }
        }

        private void OnDisable()
        {
            m_ScreenshotEvent?.UnregisterListener(OnScreenshotEventReceived);

            if (App.Initialized)
            {
                StatsAndAchievements.Client.StoreStats();
            }
        }

        /// <summary>
        /// Takes screenshot(s) if it is permitted to do so.
        /// </summary>
        /// <returns>True if a screenshot was taken, otherwise false.</returns>
        public bool TakeScreenshot()
        {
            if (canTakeScreenshot)
            {
                StartCoroutine(TakeScreenshotsCoroutine(m_NumberOfSscreenhots, delayBetweenShots, cooldown));
                
                return true;
            } 
            else
            {
                return false;
            }
        }

        public void OnScreenshotEventReceived()
        {
            float currentTime = Time.time;

            if (currentTime - lastScreenshotEventTime > m_ScreenshotEventTimeWindow)
            {
                screenshotEventCount = 0;
            }

            screenshotEventCount++;
            lastScreenshotEventTime = currentTime;

            if (screenshotEventCount >= m_MinScreenshotEventsNeeded)
            {
                if (TakeScreenshot())
                {
                    screenshotEventCount = 0;
                }
            }
        }


        private IEnumerator TakeScreenshotsCoroutine(int numberOfShots, float delayBetweenShots, float cooldown)
        {
            Debug.Log("Taking screenshot");

            canTakeScreenshot = false;

            for (int i = 0; i < numberOfShots; i++)
            {
                try
                {
                    Screenshots.TriggerScreenshot();
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to take screenshot: " + e.Message);
                }
                yield return new WaitForSeconds(delayBetweenShots);
            }

            yield return new WaitForSeconds(cooldown);
            canTakeScreenshot = true;
        }

#endif

#if UNITY_EDITOR && BUILD_DEMO
        [Button("Set to Build Full Game", EButtonEnableMode.Editor), ShowIf("showManagement")]
        private void SetToBuildFullGame() {
            // Remove the BUILD_DEMO symbol to the project settings
            PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] defines);

            defines = defines.Length > 0 ? Array.FindAll(defines, s => s != "BUILD_DEMO") : new string[] { };

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
            AssetDatabase.Refresh();
        }

#endif

#if UNITY_EDITOR && !BUILD_DEMO
        [Button("Set to Build Demo", EButtonEnableMode.Editor), ShowIf("showManagement")]
        private void SetToBuildDemo()
        {
            // Add the BUILD_DEMO symbol to the project settings
            PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] defines);

            Array.Resize(ref defines, defines.Length + 1);
            defines[defines.Length - 1] = "BUILD_DEMO";

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
            AssetDatabase.Refresh();
        }
#endif

#if UNITY_EDITOR && STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        [Button("Disable Steamworks", EButtonEnableMode.Editor), ShowIf("showManagement")]
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

        [Button("Reset Steam Stats", EButtonEnableMode.Playmode), ShowIf("showManagement")]
        private void ResetSteamStats()
        {
            StatsAndAchievements.Client.ResetAllStats(true); // true = wipe achivements too

            Debug.Log("Steam Stats and achievements reset.");
        }
#endif
    }
}
