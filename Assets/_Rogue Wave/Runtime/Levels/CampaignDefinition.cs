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
        // Meta Data
        [SerializeField, Tooltip("The name of the campaign. This is used to identify the campaign in the UI."), BoxGroup("Meta Data")]
        internal string campaignName = "New Campaign";
        [SerializeField, Tooltip("The description of the campaign. This is used to identify the campaign in the UI."), TextArea(2,5), BoxGroup("Meta Data")]
        internal string campaignDescription = "New Campaign Description";
        [SerializeField, Tooltip("If true the campaign will normally only be played once by each player. If false then each player will be able to play the campaign multiple times. Global single shot campaigns are useful for tutorial or story content that should not be repeated."), BoxGroup("Meta Data")]
        internal bool isGlobalSingleShot = false;

        // Ink Story
        [SerializeField, Tooltip("The Ink story file that defines the campaign's story. If null no story will be told during play of this campaign."), BoxGroup("Ink Story")]
        TextAsset m_InkStory = null;

        // Level Management
        [SerializeField, Tooltip("The seed to use for level generation. If set to -1, a random seed will be used."), BoxGroup("Level Management")]
        internal int seed = -1;
        [SerializeField, Tooltip("The level definitions which define the enemies, geometry and more for each level within the campaign."), BoxGroup("Level Management"), Expandable]
        internal WfcDefinition[] levels;
        [SerializeField, Tooltip("A set of achievements that must be attained to complete this campaign. Upon completion the next campaign (below) will be available."), BoxGroup("Level Management")]
        internal Achievement[] requiredAchievementsForCompletion;
        [SerializeField, Tooltip("The campaign that should be played after this one. If null the campaign will end after this one."), BoxGroup("Level Management")]
        internal CampaignDefinition nextCampaign;

        /// <summary>
        /// Get the JSON string for the Ink story file. If there is no associated story then this will be an empty string.
        /// </summary>
        internal TextAsset InkStory
        {
            get
            {
                return m_InkStory;
            }
        }

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
