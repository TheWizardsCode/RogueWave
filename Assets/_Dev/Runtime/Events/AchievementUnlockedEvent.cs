using NaughtyAttributes;
using RogueWave.GameStats;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// An AchievementUnlockedEvent is an event that is raised whenever a player unlocks and achievement.
    /// </summary>
    [CreateAssetMenu(fileName = "New Achievement Unlocked Event", menuName = "Rogue Wave/Events/Achievement Unlocked Event")]
    public class AchievementUnlockedEvent : ScriptableObject
    {
        internal Achievement achievement;

        private List<IAchievementEventListener> listeners = new List<IAchievementEventListener>();

        public virtual void Raise(Achievement achievement)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(achievement);
            }
        }

        public void AddListener(IAchievementEventListener listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(IAchievementEventListener listener)
        {
            listeners.Remove(listener);
        }
    }

}
