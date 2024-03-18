using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.GameStats;

namespace WizardsCode.GameStats
{
    [CreateAssetMenu(fileName = "New Achievement", menuName = "Rogue Wave/Stats/Achievement")]
    public class Achievement : ScriptableObject
    {
        [SerializeField, Tooltip("The key to use to store this achievement in the GameStatsManager.")]
        string m_Key;
        [SerializeField, Tooltip("The name of the achievement as used in the User Interface.")]
        string m_DispayName;
        [SerializeField, Tooltip("The description of the achievement as used in the User Interface.")]
        string m_Description;

        [Header("Tracking")]
        [SerializeField, Tooltip("The stat that this achievement is tracking.")]
        GameStat m_StatToTrack;
        [SerializeField, Tooltip("The value that the stat must reach for the achievement to be unlocked.")]
        float m_TargetValue;

        bool isUnlocked = false;

        public string Key => m_Key;
        public GameStat Stat => m_StatToTrack;
        public float TargetValue => m_TargetValue;
        public bool IsUnlocked => isUnlocked;

        internal void Reset()
        {
            isUnlocked = false;
        }

        internal void Unlock() {
            isUnlocked = true;
            Debug.Log($"Achievement {m_DispayName} unlocked!");
        }
    }
}