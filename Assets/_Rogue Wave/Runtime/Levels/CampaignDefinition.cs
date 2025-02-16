using NaughtyAttributes;
using RogueWave.GameStats;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// A CampaginDefinition is a collection of levels that are played in sequence.
    /// </summary>
    [CreateAssetMenu(fileName = "New Campaign Definition", menuName = "Rogue Wave/Campaign Definition")]
    public class CampaignDefinition : ScriptableObject
    {
        [Header("Level Management")]
        [SerializeField, Tooltip("The seed to use for level generation. If set to -1, a random seed will be used.")]
        internal int seed = -1;
        [SerializeField, Tooltip("The level definitions which define the enemies, geometry and more for each level within the campaign."), Expandable]
        internal WfcDefinition[] levels;
        [SerializeField, Tooltip("A set of achievements that must be attained to complete this campaign. Upon completion the next campaign (below) will be available.")]
        internal Achievement[] requiredAchievementsForCompletion;
        [SerializeField, Tooltip("The campaign that should be played after this one. If null the campaign will end after this one.")]
        internal CampaignDefinition nextCampaign;

        public bool IsComplete
        {
            get
            {
                if (requiredAchievementsForCompletion.Length > 0)
                {
                    List<Achievement> unlocked = GameStatsManager.Instance.unlockedAchievements;
                    foreach (Achievement achievement in requiredAchievementsForCompletion)
                    {
                        if (!unlocked.Contains(achievement))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public void SetLevel(WfcDefinition level, int index = 0)
        {
            levels[index] = level;
        }

        public WfcDefinition GetLevel(int index = 0)
        {
            return levels[index];
        }
    }
}
