using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WizardsCode.RogueWave;

namespace RogueWave.GameStats
{
    [CreateAssetMenu(fileName = "New Achievement", menuName = "Rogue Wave/Stats/Achievement")]
    public class Achievement : ScriptableObject, IParameterizedGameEventListener<int>
    {
        [SerializeField, Tooltip("The key to use to store this achievement in the GameStatsManager.")]
        string m_Key;
        [SerializeField, Tooltip("The name of the achievement as used in the User Interface."), FormerlySerializedAs("m_DispayName")]
        string m_DisplayName;
        [SerializeField, Tooltip("The description of the achievement as used in the User Interface.")]
        string m_Description;
        [SerializeField, Tooltip("The hero image for the achievement.")]
        Sprite m_HeroImage;
        [SerializeField, Tooltip("The icon to use for the achievement.")]
        Sprite m_Icon;

        [Header("Tracking")]
        [SerializeField, Tooltip("The stat that this achievement is tracking.")]
        IntGameStat m_StatToTrack;
        [SerializeField, Tooltip("The value that the stat must reach for the achievement to be unlocked.")]
        float m_TargetValue;

        [Header("Events")]
        [SerializeField, Tooltip("The event to raise when this achievement is unlocked.")]
        internal AchievementUnlockedEvent onUnlockEvent = default;

        [SerializeField, Tooltip("Is this achievement unlocked (as in has the player completed the achievement."), ReadOnly]
        bool m_IsUnlocked = false;
        [SerializeField, Tooltip("The UTC time the achievement was unlocked (if it is unlocked)."), ReadOnly, ShowIf("m_IsUnlocked")]
        string m_TimeOfUnlock;

        public string key => m_Key;
        public string displayName => m_DisplayName;
        public string description => m_Description;
        public Sprite icon => m_Icon;
        public IntGameStat stat => m_StatToTrack;
        public float targetValue => m_TargetValue;
        public bool isUnlocked => m_IsUnlocked;
        public string timeOfUnlock => m_TimeOfUnlock;
        
        private void OnEnable()
        {
            stat.onChangeEvent?.AddListener(this);
        }

        private void OnDisable()
        {
            stat.onChangeEvent?.RemoveListener(this);
        }

        internal void Reset()
        {
            m_IsUnlocked = false;
        }

        internal void Unlock() 
        {
            if (isUnlocked) return;
            
            m_IsUnlocked = true;
            m_TimeOfUnlock = DateTime.UtcNow.ToString();
            onUnlockEvent?.Raise(this);
            GameLog.Info($"Achievement {displayName} unlocked!");
        }

        public void OnEventRaised(IParameterizedGameEvent<int> e, int change)
        {
            if (e is IntStatEvent intEvent && intEvent.stat == m_StatToTrack)
            {
                if (intEvent.stat.value >= m_TargetValue)
                {
                    Unlock();
                }
            }
        }

#if UNITY_EDITOR
        [Button]
        void TestUnlock()
        {
            Unlock();
        }

        [Button]
        void TestReset()
        {
            Reset();
        }
#endif
    }
}