using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WizardsCode.GameStats;

namespace WizardsCode.GameStats
{
    [CreateAssetMenu(fileName = "New Achievement", menuName = "Rogue Wave/Stats/Achievement")]
    public class Achievement : ScriptableObject
    {
        [SerializeField, Tooltip("The key to use to store this achievement in the GameStatsManager.")]
        string m_Key;
        [SerializeField, Tooltip("The name of the achievement as used in the User Interface."), FormerlySerializedAs("m_DispayName")]
        string m_DisplayName;
        [SerializeField, Tooltip("The description of the achievement as used in the User Interface.")]
        string m_Description;
        [SerializeField, Tooltip("The icon to use for the achievement.")]
        Sprite m_Icon;

        [Header("Tracking")]
        [SerializeField, Tooltip("The stat that this achievement is tracking.")]
        GameStat m_StatToTrack;
        [SerializeField, Tooltip("The value that the stat must reach for the achievement to be unlocked.")]
        float m_TargetValue;

        bool m_IsUnlocked = false;

        public string Key => m_Key;
        public string displayName => m_DisplayName;
        public string description => m_Description;
        public Sprite icon => m_Icon;
        public GameStat stat => m_StatToTrack;
        public float targetValue => m_TargetValue;
        public bool isUnlocked => m_IsUnlocked;

        internal void Reset()
        {
            m_IsUnlocked = false;
        }

        internal void Unlock() {
            m_IsUnlocked = true;
            Debug.Log($"Achievement {displayName} unlocked!");
        }
    }
}