using NaughtyAttributes;
using NeoSaveGames;
using RogueWave;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// Scenario Manager is responsible for loading a scenaio into the Game Mode and then starting the game.
    /// It essentially replaces the player selecting the level to play.
    /// </summary>
    [RequireComponent(typeof(RogueWaveGameMode))]
    public class ScenarioManager : MonoBehaviour
    {
        public const string SCENARIO_PLAYER_SCENE = "Assets/_Rogue Wave/Scenes/RogueWave_ScenarioPlayer.unity";

        [SerializeField, Tooltip("The dummy campaign to use when loading a scenario into the game mode."), ReadOnly]
        CampaignDefinition m_Campaign;
        [SerializeField, Tooltip("The scenario to load.")]
        ScenarioDescriptor m_Scenario;

        float m_CountdownDelay = 5;
        bool m_Started = false;

        private void Awake()
        {
            m_Campaign.levels = new WfcDefinition[1];
            m_Campaign.levels[0] = m_Scenario.LevelDefinition;

            // FIXME: Should not clear the recipes here. We should create a temporary profile that gets deleted at the end of the game.
            RogueLiteManager.persistentData.RecipeIds.Clear();
            GetComponent<RogueWaveGameMode>().StartingRunRecipes = m_Scenario.Recipes;
        }

        private void Update()
        {
            if (m_Started) return;

            while (m_CountdownDelay > 0)
            {
                m_CountdownDelay -= Time.deltaTime;
                // log countdown every second
                if (m_CountdownDelay > 0 && m_CountdownDelay % 1 < Time.deltaTime)
                {
                    Debug.Log("Starting in " + m_CountdownDelay + " seconds");
                }
            }

            m_Started = true;
            GetComponent<RogueWaveGameMode>().GenerateLevel();

            Destroy(this, 10);
        }
    }
}
