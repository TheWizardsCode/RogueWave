using NaughtyAttributes;
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

        public void SetLevel(WfcDefinition level, int index = 0)
        {
            levels[index] = level;
        }
    }
}
