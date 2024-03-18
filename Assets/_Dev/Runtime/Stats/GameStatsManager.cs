// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.13

using UnityEngine;using NaughtyAttributes;
#if ENABLE_STEAMWORKS
using Steamworks;
#endif
using NeoFPS;
using System;
using RogueWave;
using NeoFPS.SinglePlayer;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WizardsCode.GameStats
{
	//
    // The GameStatsManager is responsible for managing player Stats and Achievements, as well as
    // game telemetry.
    // 
    // It is designed to work with Steamworks.NET and the Facepunch.Steamworks library for builds that will be distributed on Steam.
    // By default SteamWorks support is disabled. To enable it, define the symbol "ENABLE_STEAMWORKS" in the project settings for the Steam enabled builds.
    //
    [DisallowMultipleComponent]
    [RequireComponent(typeof(IPlayerCharacterWatcher))]
    public class GameStatsManager : MonoBehaviour, IPlayerCharacterSubscriber
    {
        [Header("Player Performance")]
        [SerializeField, Expandable, Tooltip("The count of succesful runs in the game.")]
        private GameStat m_VictoryCount;
        [SerializeField, Expandable, Tooltip("The count of deaths in the game.")]
        private GameStat m_DeathCount;

        [Header("Nanobot Stats")]
        [SerializeField, Expandable, Tooltip("The count of resources collected in the game.")]
        private GameStat m_ResourcesCollected;
        [SerializeField, Expandable, Tooltip("The count of resources spent in the game.")]
        private GameStat m_ResourcesSpent;

#if ENABLE_STEAMWORKS
        [Header("Steam Settings")]
        [SerializeField, Tooltip("The Steam App ID for the game.")]
        private uint m_SteamAppId = 0;
        [SerializeField, Tooltip("The frequency with which stats are stored to Steam.")]
        private float m_FrequencyOfSteamStatStore = 60;

        private float m_TimeToNextSteamStatStore = 0;
#endif

        private RogueWaveGameMode m_GameMode;

        private static bool isDirty;
        private IPlayerCharacterWatcher m_Watcher;

        public static GameStatsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            m_Watcher = GetComponentInParent<IPlayerCharacterWatcher>();
            if (m_Watcher == null)
            {
                Debug.LogError("GameStatsManager require a component that implements IPlayerCharacterWatcher", gameObject);
                return;
            }
            m_Watcher.AttachSubscriber(this);

#if ENABLE_STEAMWORKS
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
            RogueWaveGameMode.onVictory += OnVictory;
        }

        private void OnIsAliveChanged(ICharacter character, bool alive)
        {
            if (!alive)
            {
                IncrementCounter(m_DeathCount);
            }
        }

        private void OnVictory()
        {
            IncrementCounter(m_VictoryCount);
        }

        private void OnDisable()
        {
#if ENABLE_STEAMWORKS
            if (isDirty)
            {
                SteamUserStats.StoreStats();
            }
            SteamClient.Shutdown();
#endif

            RogueWaveGameMode.onVictory -= OnVictory;
            if (FpsSoloCharacter.localPlayerCharacter != null)
                FpsSoloCharacter.localPlayerCharacter.onIsAliveChanged -= OnIsAliveChanged;
        }

        private void Update()
        {
#if ENABLE_STEAMWORKS
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
        internal static void IncrementCounter(GameStat stat)
        {
            IncrementCounter(stat, 1);
        }

        /// <summary>
        /// Increments an integer counter by a set amount.
        /// </summary>
        /// <param name="stat"></param>
        internal static void IncrementCounter(GameStat stat, int amount)
        {
            stat.Increment(amount);

#if ENABLE_STEAMWORKS
            if (!SteamClient.IsValid)
                return;

            SteamUserStats.AddStat(stat.Key, amount);
            isDirty = true;
#endif
        }

        private ICharacter m_Character;
        private NanobotManager m_NanobotManager;
        public void OnPlayerCharacterChanged(ICharacter character)
        {
            if (character == null || m_Character == character)
                return;

            if (m_Character != null)
            {
                m_Character.onIsAliveChanged -= OnIsAliveChanged;
                m_NanobotManager.onResourcesChanged -= OnResourcesChanged;
            }

            m_Character = character;
            m_Character.onIsAliveChanged += OnIsAliveChanged;

            m_NanobotManager = character.GetComponent<NanobotManager>();
            m_NanobotManager.onResourcesChanged += OnResourcesChanged;
        }

        private void OnResourcesChanged(float from, float to, float resourcesUntilNextLevel)
        {
            if (from < to)
            {
                IncrementCounter(m_ResourcesCollected, Mathf.RoundToInt(to - from));
            }
            else if (from > to)
            {
                IncrementCounter(m_ResourcesSpent, Mathf.RoundToInt(from - to));
            }
        }

        #region EDITOR_ONLY
#if UNITY_EDITOR
        #region ScriptableObjects
        [Button("Reset Stats and Achievements")]
        private void ResetStats()
        {
            if (Application.isPlaying)
            {

                GameStat[] gameStats = Resources.LoadAll<GameStat>("");
                foreach (GameStat stat in gameStats)
                {
                    stat.Reset();
                }

#if ENABLE_STEAMWORKS
                ResetSteamStats();
#endif
                    Debug.Log("Stats and achievements reset.");
            }
            else
            {
                Debug.LogError("You can only reset Steam stats and achievements in play mode.");
            }
        }

        [Button("Dump Stats and Achievements to Console")]
        private void DumpStats()
        {
            if (Application.isPlaying)
            {
                GameStat[] gameStats = Resources.LoadAll<GameStat>("");

                foreach (GameStat stat in gameStats)
                {
                    if (stat.Key != null)
                    {
                        switch (stat.Type)
                        {
                            case GameStat.StatType.Int:
                                Debug.Log($"Scriptable Object: {stat.Key} = {stat.GetIntValue()}");
                                break;
                            case GameStat.StatType.Float:
                                Debug.Log($"Scriptable Object: {stat.Key} = {stat.GetFloatValue()}");
                                break;
                        }
#if ENABLE_STEAMWORKS
                        DumpSteamStat(stat);
#endif
                    }
                }
            }
            else
            {
                Debug.LogError("You can only dump stats and achievements in play mode.");
            }
        }
        #endregion

#if ENABLE_STEAMWORKS
        #region Steamworks
        [Button("Disable Steamworks")]
        private void DisableSteamworks()
        {
            // Remove the ENABLE_STEAMWORKS symbol to the project settings
            PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] defines);
            defines = defines.Length > 0 ? Array.FindAll(defines, s => s != "ENABLE_STEAMWORKS") : new string[] { };
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
            switch (stat.Type)
            {
                case GameStat.StatType.Int:
                    Debug.Log($"Steam: {stat.Key} = {SteamUserStats.GetStatInt(stat.Key)}");
                    break;
                case GameStat.StatType.Float:
                    Debug.Log($"Steam: {stat.Key} = {SteamUserStats.GetStatFloat(stat.Key)}");
                    break;
            }
        }
        #endregion
#else
        #region NoSteamworks
        [Button("Enable Steamworks")]
        private void EnableSteamworks()
        {
            // Add the ENABLE_STEAMWORKS symbol to the project settings
            PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] defines);
            Array.Resize(ref defines, defines.Length + 1);
            defines[defines.Length - 1] = "ENABLE_STEAMWORKS";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
            AssetDatabase.Refresh();
        }
        #endregion
#endif
#endif
            #endregion
        }
}
