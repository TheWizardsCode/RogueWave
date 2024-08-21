using NaughtyAttributes;
using UnityEngine;
using System;
using WizardsCode.RogueWave;

namespace RogueWave.GameStats
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

        [Header("Meta Data")]
        [SerializeField, Tooltip("The key to use to store this stat in the GameStatsManager.")]
        string m_Key;
        [SerializeField, Tooltip("The name of the stat as displayed in the UI.")]
        string m_displayName;
        [SerializeField, TextArea, Tooltip("A description of the stat.")]
        string m_description;

        [Header("Value Management")]
        [SerializeField, Tooltip("The type of stat.")]
        StatType m_StatType;
        [SerializeField, Tooltip("The minimum value this stat is allowed to take. If there is an attempt to set it lower it will be clamped to this value.")]
        float m_minValue = 0;
        [SerializeField, OnValueChanged("OnDefaultValueChangedCallback"), Tooltip("The default value of the stat.")]
        float m_DefaultValue = 0;
        [SerializeField, Tooltip("The formatting string to use when displaying a string representation of the stat.")]
        string m_FormatString = "00000";

        [Header("Tracking")]
        [SerializeField, Tooltip("The event to raise when this stat is changed.")]
        ParameterizedGameEvent<float> onChangeEvent;
        [SerializeField, Tooltip("A stat to increase when the event stat is increased. The amount of the increase will be added to this stat. This is useful for tracking things like resources gathered or hit points healed.")]
        internal GameStat increasedAmount;
        [SerializeField, Tooltip("A stat to increase when the event stat is decreased. The amount of the decrease will be added to this stat. This is useful for tracking things like resources spent or hit points lost.")]
        internal GameStat decreasedAmount;

        [Header("Scoring")]
        [SerializeField, Tooltip("If true then this stat will contribute to the players score.")]
        internal bool contributeToScore = false;
        [SerializeField, Tooltip("The amount to multiply this stat by when calculating the score."), ShowIf("contributeToScore")]
        int  m_ScoreMultiplier = 1;

        [ShowNonSerializedField]
        int m_intValue;
        [ShowNonSerializedField]
        float m_floatValue;

        public string key => m_Key;
        public StatType type => m_StatType;

        public int ScoreContribution
        {
            get
            {
                if (!contributeToScore)
                {
                    throw new Exception("Asking for a score contribution from a stat that is not set to contribute to the score.");
                }

                switch (m_StatType)
                {
                    case StatType.Int:
                        return m_intValue * m_ScoreMultiplier;
                    case StatType.Float:
                        return Mathf.RoundToInt(m_floatValue * m_ScoreMultiplier);
                }

                Debug.LogError("Asking for a score for an unknown stat type. Returning 0.");
                return 0;
            }
        }

        public string displayName
        {
            get { 
                if (string.IsNullOrEmpty(m_displayName))
                {
                    return m_Key;
                }
                return m_displayName; 
            }
            private set { m_displayName = value; }
        }

        public string description
        {
            get { return m_description; }
            private set { m_description = value; }
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

            if (value < m_minValue)
            {
                value = Mathf.RoundToInt(m_minValue);
            }

            int change = value - m_intValue;
            m_intValue = value;
            onChangeEvent?.Raise(change);

            if (change > 0)
            {
                if (increasedAmount != null)
                {
                    increasedAmount.Add(change);
                }
            }
            else
            {
                if (decreasedAmount != null)
                {
                    decreasedAmount.Add(-change);
                }
            }

            GameStatsManager.isDirty = true;
            GameStatsManager.Instance.CheckAchievements(this, m_intValue);
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

        public int GetIntRoundedValue()
        {
            if (m_StatType == StatType.Float)
            {
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

            if (value < m_minValue)
            {
                value = m_minValue;
            }

            float change = value - m_floatValue;
            m_floatValue = value;
            onChangeEvent?.Raise(change);

            if (change > 0)
            {
                if (increasedAmount != null)
                {
                    increasedAmount.Add(change);
                }
            }
            else
            {
                if (decreasedAmount != null)
                {
                    decreasedAmount.Add(-change);
                }
            }

            Debug.LogError("Currently not raising events for changes to float values.");

            GameStatsManager.isDirty = true;
            GameStatsManager.Instance.CheckAchievements(this, m_floatValue);
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
        /// Adds an integer value (default 1) to the stat. Note that if amount is negative this will 
        /// decrease the value of the stat.
        /// 
        /// </summary>
        /// <param name="amount">The amount to increment the stat.</param>
        /// <returns>The new value of the stat.</returns>
        internal int Add(int amount = 1)
        {
            if (type == StatType.Float)
            {
                throw new ArgumentException("Asking to increment an int value on a float stat. Use Increment(float amount) instead");
            }
            
            SetValue(m_intValue + amount);

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
        internal float Add(float amount)
        {
            if (type == StatType.Int)
            {
                throw new ArgumentException("Asking to increment a float value on a int stat. Use Increment(int amount) instead");
            }

            SetValue(m_floatValue + amount);

#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
            SteamUserStats.AddStat(key, amount);
#endif

            return m_floatValue;
        }


#if UNITY_EDITOR
        [HorizontalLine(color: EColor.Blue)]
        [SerializeField]
#pragma warning disable CS0414 // used in Button attribute
        bool showDebug = false;
#pragma warning restore CS0414

        [Button, ShowIf("showDebug")]
        private void Add100Resources()
        {
            Add(100);
            Debug.Log($"{displayName} = {GetIntValue()}");
        }

        [Button, ShowIf("showDebug")]
        private void Remove100Resources()
        {
            Add(-100);
            Debug.Log($"{displayName} = {GetIntValue()}");
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
#endif
    }
}
