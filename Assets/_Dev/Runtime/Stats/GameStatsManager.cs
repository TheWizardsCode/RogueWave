// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.13

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;using NaughtyAttributes;
#if !DISABLESTEAMWORKS
using Steamworks;
using NeoFPS;
using System;
using UnityEditor.PackageManager;
using RogueWave;
using NeoFPS.SinglePlayer;
#endif

namespace WizardsCode.GameStats
{
	    //
    // The StatsManager is responsible for managing the Stats and Achievements. Both locally and on Steam.
    //
    [DisallowMultipleComponent]
    [RequireComponent(typeof(IPlayerCharacterWatcher))]
    public class GameStatsManager : MonoBehaviour, IPlayerCharacterSubscriber
    {
        [Header("Player Performance")]
        [SerializeField, Tooltip("The count of succesful runs in the game.")]
        private GameStat m_VictoryCount;
        [SerializeField, Tooltip("The count of deaths in the game.")]
        private GameStat m_DeathCount;

        [Header("Nanobot Stats")]
        [SerializeField, Tooltip("The count of resources collected in the game.")]
        private GameStat m_ResourcesCollected;
        [SerializeField, Tooltip("The count of resources spent in the game.")]
        private GameStat m_ResourcesSpent;

        [Header("Stat Database Settings")]
        [SerializeField, Tooltip("The Steam App ID for the game.")]
        private uint m_SteamAppId = 0;
        [SerializeField, Tooltip("The frequency with which stats are stored to Steam.")]
        private float m_FrequencyOfStatStore = 60;

        private RogueWaveGameMode m_GameMode;

        private static bool isDirty;
        private float m_TimeToNextStatStore = 0;
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
            if (isDirty)
            {
                SteamUserStats.StoreStats();
            }
            SteamClient.Shutdown();

            RogueWaveGameMode.onVictory -= OnVictory;
            if (FpsSoloCharacter.localPlayerCharacter != null)
                FpsSoloCharacter.localPlayerCharacter.onIsAliveChanged -= OnIsAliveChanged;
        }

        private void Update()
        {
            m_TimeToNextStatStore -= Time.deltaTime;
            if (isDirty && m_TimeToNextStatStore > 0)
            {
                if (SteamUserStats.StoreStats())
                {
                    isDirty = false;
                    m_TimeToNextStatStore = m_FrequencyOfStatStore;
                }
            }

            SteamClient.RunCallbacks();
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
            if (!SteamClient.IsValid)
                return;

            SteamUserStats.AddStat(stat.Key, amount);
            isDirty = true;
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
            if (!SteamClient.IsValid)
                return;

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
        [Button("Reset Stats and Achievements")]
        private void ResetStats()
        {
            if (Application.isPlaying)
            {
                SteamUserStats.ResetAll(true); // true = wipe achivements too
                SteamUserStats.StoreStats();
                SteamUserStats.RequestCurrentStats();

                Debug.Log("Stats and achievements reset.");
            }
            else
            {
                Debug.LogError("You can only reset stats and achievements in play mode.");
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
                                Debug.Log($"{stat.Key} = {SteamUserStats.GetStatInt(stat.Key)}");
                                break;
                            case GameStat.StatType.Float:
                                Debug.Log($"{stat.Key} = {SteamUserStats.GetStatFloat(stat.Key)}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("You can only dump stats and achievements in play mode.");
            }
        }
#endif
        #endregion
    }
}
