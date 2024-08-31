using UnityEngine;
using System;
using System.Collections.Generic;

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
    [CreateAssetMenu(fileName = "New GameStat", menuName = "Rogue Wave/Stats/String Game Stat", order = 1)]
    public class StringGameStat : GameStat<string>
    {
        public override int ScoreContribution
        {
            get
            {
                return 0;
            }
        }

        public override string ValueAsString
        {
            get
            {
                return value;
            }
        }

        public override string SetValue(string value)
        {
            if (m_CurrentValue != value)
            {
                m_CurrentValue = value;
                RogueLiteManager.persistentData.isDirty = true;
            }

            return m_CurrentValue;
        }

        /// <summary>
        /// Add a string to the end of the current value.
        /// </summary>
        /// <param name="postfix">the string to add</param>
        /// <returns>The resulting string</returns>
        public override string Add(string postfix)
        {
            if (string.IsNullOrEmpty(postfix))
            {
                return value;
            }

            m_CurrentValue = m_CurrentValue + postfix;
            onChangeEvent?.Raise(postfix);
            RogueLiteManager.persistentData.isDirty = true;

            return value;
        }

        /// <summary>
        /// Remove a substring from the current value.
        /// </summary>
        /// <param name="substring">The string to remove</param>
        /// <returns>The resulting string.</returns>
        public override string Subtract(string substring)
        {
            if (string.IsNullOrEmpty(substring))
            {
                return value;
            }

            int idx = m_CurrentValue.IndexOf(substring);
            if (idx < 0)
            {
                return value;
            }

            m_CurrentValue = value.Remove(idx, substring.Length);
            onChangeEvent?.Raise(substring);
            RogueLiteManager.persistentData.isDirty = true;

            return value;
        }
    }
}
