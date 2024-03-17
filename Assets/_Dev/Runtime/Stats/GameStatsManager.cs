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
#endif

namespace WizardsCode.GameStats
{
	    //
    // The StatsManager is responsible for managing the Stats and Achievements. Both locally and on Steam.
    //
    [DisallowMultipleComponent]
    public class GameStatsManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The game stats that will be collected.")]
        private GameStat[] m_Stats;

        [Header("Stat Database Settings")]
        [SerializeField, Tooltip("The Steam App ID for the game.")]
        private uint m_SteamAppId = 0;
        [SerializeField, Tooltip("The frequency with which stats are stored to Steam.")]
        private float m_FrequencyOfStatStore = 60;

        private static bool isDirty;
        private float m_TimeToNextStatStore = 0;

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

            try
            {
                SteamClient.Init(m_SteamAppId, true);
                Debug.Log("Steam ID: " + SteamClient.SteamId);
            }
            catch (Exception e)
            {
                Debug.LogError("Steamworks failed to initialize: " + e.Message);
            }
        }

        private void OnDisable()
        {
            if (isDirty)
            {
                SteamUserStats.StoreStats();
            }
            SteamClient.Shutdown();
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
            SteamUserStats.AddStat(stat.Key, 1);
            isDirty = true;
        }

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
                foreach (GameStat stat in m_Stats)
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
    }
}
