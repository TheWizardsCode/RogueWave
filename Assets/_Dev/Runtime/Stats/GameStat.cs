using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
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
    public abstract class GameStat<T> : ScriptableObject, IGameStat<T>
    {
        [Header("Meta Data")]
        [SerializeField, Tooltip("The key to use to store this stat in the GameStatsManager.")]
        string m_Key;
        [SerializeField, Tooltip("The name of the stat as displayed in the UI.")]
        string m_displayName;
        [SerializeField, TextArea, Tooltip("A description of the stat.")]
        string m_description;

        [Header("Value Management")]
        [SerializeField, Tooltip("The default value for this stat.")]
        T m_DefaultValue = default;
        [SerializeField, Tooltip("Is this a time in seconds?")]
        internal bool isTime = false;
        [SerializeField, HideIf("isTime"), Tooltip("The formatting string to use when displaying a string representation of the stat.")]
        internal string m_FormatString = "00000";

        [Header("Tracking")]
        [SerializeField, Tooltip("The event to raise when this stat is changed.")]
        internal ParameterizedGameEvent<T> onChangeEvent;
        [SerializeField, Tooltip("A stat to increase when the event stat is increased. The amount of the increase will be added to this stat. This is useful for tracking things like resources gathered or hit points healed.")]
        internal GameStat<T> increasedAmount;
        [SerializeField, Tooltip("A stat to increase when the event stat is decreased. The amount of the decrease will be added to this stat. This is useful for tracking things like resources spent or hit points lost.")]
        internal GameStat<T> decreasedAmount;

        [Header("Scoring")]
        [SerializeField, Tooltip("If true then this stat will contribute to the players score.")]
        internal bool contributeToScore = false;
        [SerializeField, Tooltip("The amount to multiply this stat by when calculating the score."), ShowIf("contributeToScore")]
        internal int m_ScoreMultiplier = 1;

        public T m_CurrentValue = default;
        public T value { 
            get { return m_CurrentValue; } 
        }

        public virtual T SetValue(T value)
        {
            // OPTIMIZATION: This method should be overridden in subclasses in order to remove the need for this kind of validation.
#if UNITY_EDITOR
            Debug.LogWarning("An implementation of GameStat has not overrideen SetValue which has performance implications.");
#endif

            if (!EqualityComparer<T>.Default.Equals(m_CurrentValue, value))
            {
                m_CurrentValue = value;
                RogueLiteManager.persistentData.isDirty = true;
            }
            return m_CurrentValue;
        }

        public virtual T Add(T change)
        {
            // OPTIMIZATION: This method should be overridden in subclasses in order to remove the need for `dynamic`, which has performance implications.
#if UNITY_EDITOR
            Debug.LogWarning("An implementation of GameStat has not overrideen Add(change) which has performance implications.");
#endif

            if (Comparer<T>.Default.Compare(change, default(T)) == 0)
            {
                return value;
            }

            m_CurrentValue = (dynamic)m_CurrentValue + change;
            onChangeEvent.Raise(m_CurrentValue);
            RogueLiteManager.persistentData.isDirty = true;

            if (Comparer<T>.Default.Compare(change, default(T)) > 0 && increasedAmount != null)
            {
                increasedAmount.Add(change);
            }
            else if (Comparer<T>.Default.Compare(change, default(T)) < 0 && decreasedAmount != null)
            {
                decreasedAmount.Add((dynamic)change * -1);
            }
            return value;
        }

        public virtual T Subtract(T change)
        {
            // OPTIMIZATION: This method should be overridden in subclasses in order to remove the need for `dynamic`, which has performance implications.
#if UNITY_EDITOR
            Debug.LogWarning("An implementation of GameStat has not overrideen Subtract(change) which has performance implications.");
#endif

            if (Comparer<T>.Default.Compare(change, default(T)) == 0)
            {
                return value;
            }
            
            m_CurrentValue = (dynamic)m_CurrentValue - change;
            onChangeEvent.Raise(m_CurrentValue);
            RogueLiteManager.persistentData.isDirty = true;

            if (Comparer<T>.Default.Compare(change, default(T)) > 0 && decreasedAmount != null)
            {
                decreasedAmount.Add(change);
            }
            else if (Comparer<T>.Default.Compare(change, default(T)) < 0 && increasedAmount != null)
            {
                increasedAmount.Add((dynamic)change * -1);
            }
            return value;
        }

        public T defaultValue => m_DefaultValue;

        public string key => m_Key;

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

        public abstract string ValueAsString { get; }

        public abstract int ScoreContribution { get; }

    }
}
