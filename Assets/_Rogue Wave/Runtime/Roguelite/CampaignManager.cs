using NaughtyAttributes;
using RogueWave;
using UnityEngine;
using WizardsCode.StoryTeller;

namespace WizardsCode.RogueWave
{
    public class CampaignManager : StoryManager
    {
        [SerializeField, Tooltip("The campaign definitions which defines the levels to play in order, which in turn defines the enemies, geometry and more for each level."), Expandable, BoxGroup("Campaigns")]
        CampaignDefinition[] m_Campaign;

        [SerializeField, HideInInspector]
        int m_CurrentCampaignIndex = 0;
        public CampaignDefinition CurrentCampaign 
        { 
            get { return m_Campaign[m_CurrentCampaignIndex]; } 
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            foreach (CampaignDefinition campaign in m_Campaign)
            {
                if (campaign.isGlobalSingleShot && campaign.IsComplete)
                {
                    m_CurrentCampaignIndex++;
                    continue;
                }

                if (!campaign.IsComplete)
                {
                    break;
                }
            }

            if (CurrentCampaign.InkStory != null)
            {
                InitializeStory(CurrentCampaign.InkStory);
            }
        }

        /// <summary>
        /// Checks to see if the conditions for completion of the current campaign have been met.
        /// If they have then the next campaign will be unlocked.
        /// </summary>
        internal void EnableNextCampaignIfReady()
        {
            if (CurrentCampaign.nextCampaign != null && CurrentCampaign.IsComplete)
            {
                m_CurrentCampaignIndex++;
                RogueLiteManager.PersistentData.currentGameLevel = 0;

                if (CurrentCampaign.InkStory != null)
                {
                    InitializeStory(CurrentCampaign.InkStory);
                }
            }
        }

        public override void Load()
        {
            if (RogueLiteManager.CurrentProfile == null)
            {
                return;
            } 

            base.Load();
        }

        public override void Save(string knotName)
        {

            if (RogueLiteManager.CurrentProfile == null)
            {
                return;
            }
            
            base.Save(knotName);
        }
    }
}
