using NaughtyAttributes;
using UnityEngine;
using System;

namespace WizardsCode.GameStats
{
    /// <summary>
    /// A Game Stat is a single stat that can be tracked by the GameStatsManager.
    /// 
    /// Create as many instances of this class as you need to track the stats you want to track.
    /// Calle the SetValue, Increment, or other helper methods to change the value of the stat.
    /// 
    /// The GameStatsManager will automatically save and load the stats to and from PlayerPrefs and, if enabled, SteamWorks.
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "New GameStat", menuName = "Rogue Wave/Stats/Game Stat", order = 1)]
    public class GameStat : ScriptableObject
    {
        public enum StatType
        {
            Int,
            Float
        }

        [SerializeField, Tooltip("The key to use to store this stat in the GameStatsManager.")]
        string m_Key;
        [SerializeField, Tooltip("The name of the stat as displayed in the UI.")]
        string m_displayName;
        [SerializeField, Tooltip("The type of stat.")]
        StatType m_StatType;
        [SerializeField, OnValueChanged("OnDefaultValueChangedCallback"), Tooltip("The default value of the stat.")]
        float m_DefaultValue = 0;
        [SerializeField, Tooltip("The formatting string to use when displaying a string representation of the stat.")]
        string m_FormatString = "00000";
        

        [ShowNonSerializedField]
        int m_intValue;
        [ShowNonSerializedField]
        float m_floatValue;

        public string key => m_Key;
        public StatType type => m_StatType;

        public string displayName
        {
            get { 
                if (string.IsNullOrEmpty(m_displayName))
                {
                    return m_Key;
                }
                return m_displayName; 
            }
            set { m_displayName = value; }
        }

        internal void Reset()
        {
            m_intValue = 0;
            m_floatValue = 0;
        }

        public void SetValue(int value)
        {
            if (m_StatType == StatType.Float)
            {
                Debug.LogWarning("Asking to set an int value on a float stat. This will be cast to a float.");
                SetValue((float)value);
            }

            m_intValue = value;
        }

        public int GetIntValue()
        {
            if (m_StatType == StatType.Float)
            {
                Debug.LogWarning("Asking for an int value from a float stat. Float has been rounded to an int.");
                return Mathf.RoundToInt(m_floatValue);
            }
            return m_intValue;
        }

        public void SetValue(float value)
        {
            if (m_StatType == StatType.Int)
            {
                Debug.LogWarning("Asking to set a float value on an int stat. This will be rounded to an int.");
                value = Mathf.RoundToInt(value);
            }
            m_floatValue = value;
        }

        public float GetFloatValue()
        {
            if (m_StatType == StatType.Int)
            {
                Debug.LogWarning("Asking for a float value from an int stat. Int has been cast to a float.");
                return m_intValue;
            }
            return m_floatValue;
        }

        public string GetValueAsString()
        {
            if (m_StatType == StatType.Int)
            {
                return m_intValue.ToString(m_FormatString);
            } else
            {
                return m_floatValue.ToString(m_FormatString);
            }
        }

        /// <summary>
        /// Increments an integer value by a set amount.
        /// </summary>
        /// <param name="amount">The amoun to increment the stat.</param>
        /// <returns>The new value of the stat.</returns>
        internal int Increment(int amount = 1)
        {
            if (type == StatType.Float)
            {
                throw new ArgumentException("Asking to increment an int value on a float stat. Use Increment(float amount) instead");
            }
            
            m_intValue += amount;
            GameStatsManager.isDirty = true;

            GameStatsManager.Instance.CheckAchievements(this, m_intValue);

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            SteamUserStats.AddStat(key, amount);
#endif

            return m_intValue;
        }

        /// <summary>
        /// Increments an float value by a set amount.
        /// </summary>
        /// <param name="amount">The amoun to increment the stat.</param>
        /// <returns>The new value of the stat.</returns>
        internal float Increment(float amount)
        {
            if (type == StatType.Int)
            {
                throw new ArgumentException("Asking to increment a float value on a int stat. Use Increment(int amount) instead");
            }

            m_floatValue += amount;
            GameStatsManager.isDirty = true;

            GameStatsManager.Instance.CheckAchievements(this, m_floatValue);

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            SteamUserStats.AddStat(key, amount);
#endif

            return m_floatValue;
        }

        private void OnDefaultValueChangedCallback()
        {
           switch (m_StatType)
            {
                case StatType.Int:
                    m_DefaultValue = Mathf.RoundToInt(m_DefaultValue);
                    break;
                case StatType.Float:
                    break;
            }
        }
    }
}
