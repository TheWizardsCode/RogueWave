using NaughtyAttributes;
using RogueWave;
using UnityEngine;
using WizardsCode.StoryTeller;

namespace WizardsCode.RogueWave
{
    public class CampaignManager : StoryManager
    {
        [SerializeField, Tooltip("The campaign definitions which defines the levels to play in order, which in turn defines the enemies, geometry and more for each level."), Expandable, BoxGroup("Ink Configuration"), Required]
        CampaignDefinition m_Campaign;

        public CampaignDefinition CurrentCampaign 
        { 
            get { return m_Campaign; } 
            internal set { m_Campaign = value; }
        }
    }
}
