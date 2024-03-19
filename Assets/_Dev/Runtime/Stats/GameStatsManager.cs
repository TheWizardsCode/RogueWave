// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.13

#if UNITY_EDITOR
using UnityEditor;
#endif

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
using Steamworks;
#endif

using UnityEngine;
using NaughtyAttributes;
using NeoFPS;
using System;
using RogueWave;
using NeoFPS.SinglePlayer;
using System.Collections.Generic;

namespace WizardsCode.GameStats
{
    //
    // The GameStatsManager is a singleton responsible for managing player Stats and Achievements, as well as
    // game telemetry.
    // 
    // It is designed to work with Steamworks.NET and the Facepunch.Steamworks library for builds that will be distributed on Steam.
    // By default SteamWorks support is disabled. To enable it, define the symbol "STEAMWORKS_ENABLED" in the project settings for the Steam enabled builds, and ensure "STEAMWORKS_DISABLED" is not set (disabled will take precedent if both are set).
    //
    [DisallowMultipleComponent]
    public class GameStatsManager : MonoBehaviour
    {
        private Achievement[] m_Achievements;

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
        [SerializeField, Foldout("Steam"), Tooltip("The Steam App ID for the game.")]
        private uint m_SteamAppId = 0;
        [SerializeField, Foldout("Steam"), Tooltip("The frequency with which stats are stored to Steam.")]
        private float m_FrequencyOfSteamStatStore = 60;

        private float m_TimeToNextSteamStatStore = 0;
#endif

        internal static bool isDirty;

        private List<Spawner> m_Spawners = new List<Spawner>();

        public static GameStatsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

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

        private void Update()
        {
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

            SteamUserStats.AddStat(stat.Key, amount);
            isDirty = true;
#endif
        }

        internal void CheckAchievements(GameStat stat, int value)
        {
            // OPTIMIZATION: This could be optimized by only checking achievements that are related to the stat that has changed. i.e. sort the achievements into a dictionary by stat and only check the relevant ones.
            // OPTIMIZATION: This could be further optimized by only checking achievements that are not yet unlocked, i.e. once an achievement has been unlocked it can be removed from the list of achievements to check.
            foreach (Achievement achievement in m_Achievements)
            {
                if (!achievement.IsUnlocked && achievement.Stat == stat)
                {
                    if (value >= achievement.TargetValue)
                    {
                        achievement.Unlock();
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
                if (!achievement.IsUnlocked && achievement.Stat == stat)
                {
                    if (value >= achievement.TargetValue)
                    {
                        achievement.Unlock();
                    }
                }
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

                Achievement[] achievements = Resources.LoadAll<Achievement>("");
                foreach (Achievement achievement in achievements)
                {
                    achievement.Reset();
                }

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

        [Button("Dump Stats and Achievements to Console")]
        private void DumpStatsAndAchievements()
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
#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
                        DumpSteamStat(stat);
#endif
                    }
                }

                Achievement[] achievements = Resources.LoadAll<Achievement>("");
                foreach (Achievement achievement in m_Achievements)
                {
                    string status = achievement.IsUnlocked ? "Unlocked" : "Locked";
                    Debug.Log($"Scriptable Object: {achievement.name} = {status}");

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
        #endregion

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

        private void DumpSteamAchievement(Achievement achievement)
        {
            foreach (Steamworks.Data.Achievement steamAchievement in SteamUserStats.Achievements)
            {
                if (steamAchievement.Identifier == achievement.Key)
                {
                    string state = steamAchievement.State ? "Unlocked" : "Locked";
                    Debug.Log($"Steam: {achievement.name} = {state}");
                    return;
                }
            }

            Debug.LogError($"Steam: {achievement.name} = Not found");
        }
#else
        [Button("Enable Steamworks")]
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
}
