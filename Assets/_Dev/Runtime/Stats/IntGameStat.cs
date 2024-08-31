using UnityEngine;
using System;
using NaughtyAttributes;
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
    [CreateAssetMenu(fileName = "New GameStat", menuName = "Rogue Wave/Stats/Integer Game Stat", order = 1)]
    public class IntGameStat : GameStat<int>
    {

        [Header("Value Management")]
        [SerializeField, Tooltip("Is this a time in seconds?")]
        internal bool isTime = false;
        [SerializeField, HideIf("isTime"), Tooltip("The formatting string to use when displaying a string representation of the stat.")]
        internal string m_FormatString = "00000";

        [Header("Scoring")]
        [SerializeField, Tooltip("If true then this stat will contribute to the players score.")]
        internal bool contributeToScore = false;
        [SerializeField, Tooltip("The amount to multiply this stat by when calculating the score."), ShowIf("contributeToScore")]
        internal int m_ScoreMultiplier = 1;

        [Header("Tracking")]
        [SerializeField, Tooltip("A stat to increase when the event stat is increased. The amount of the increase will be added to this stat. This is useful for tracking things like resources gathered or hit points healed.")]
        internal GameStat<int> increasedAmount;
        [SerializeField, Tooltip("A stat to increase when the event stat is decreased. The amount of the decrease will be added to this stat. This is useful for tracking things like resources spent or hit points lost.")]
        internal GameStat<int> decreasedAmount;

        public override int ScoreContribution
        {
            get
            {
                if (!contributeToScore)
                {
                    throw new Exception("Asking for a score contribution from a stat that is not set to contribute to the score.");
                }

                return value * m_ScoreMultiplier;
            }
        }

        public override string ValueAsString
        {
            get
            {
                if (isTime)
                {
                    return TimeSpan.FromSeconds(value).ToString(@"hh\:mm\:ss");
                }
                else
                {
                    return value.ToString(m_FormatString);
                }
            }
        }

        public override int SetValue(int value)
        {
            if (m_CurrentValue != value)
            {
                m_CurrentValue = value;
                RogueLiteManager.persistentData.isDirty = true;
            }

            return m_CurrentValue;
        }

        public override int Add(int change)
        {
            if (change == 0)
            {
                return value;
            }

            m_CurrentValue = m_CurrentValue + change;
            onChangeEvent?.Raise(change);
            RogueLiteManager.persistentData.isDirty = true;

            if (change > 0 && increasedAmount != null)
            {
                increasedAmount.Add(change);
            }
            else if (change < 0 && decreasedAmount != null)
            {
                decreasedAmount.Add(change * -1);
            }
            return value;
        }

        public override int Subtract(int change)
        {
            if (change == 0)
            {
                return value;
            }

            m_CurrentValue = (dynamic)m_CurrentValue - change;
            onChangeEvent?.Raise(change);
            RogueLiteManager.persistentData.isDirty = true;

            if (change > 0 && decreasedAmount != null)
            {
                decreasedAmount.Add(change);
            }
            else if (change < 0 && increasedAmount != null)
            {
                increasedAmount.Add(change * -1);
            }
            return value;
        }
    }
}
