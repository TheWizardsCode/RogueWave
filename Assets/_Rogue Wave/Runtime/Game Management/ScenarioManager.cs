using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using RogueWave;
using UnityEngine;
using WizardsCode.CommandTerminal;

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
        [SerializeField, Tooltip("The scenario to load."), Expandable]
        ScenarioDescriptor m_Scenario;

        bool m_IsInitialized = false;

        private void Awake()
        {
            m_Campaign.levels = new WfcDefinition[1];
            m_Campaign.levels[0] = m_Scenario.LevelDefinition;

            // FIXME: Should not clear the recipes here. We should create a temporary profile that gets deleted at the end of the game.
            RogueLiteManager.persistentData.RecipeIds.Clear();
            GetComponent<RogueWaveGameMode>().StartingRunRecipes = m_Scenario.Recipes;
        }

        private void Start()
        {
            GetComponent<RogueWaveGameMode>().GenerateLevel();
        }

        private void Update()
        {
            if (m_IsInitialized)
            {
                return;
            }

            if (FpsSoloCharacter.localPlayerCharacter == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(m_Scenario.TerminalScript))
            {
                TerminalCommands.RunScript(m_Scenario.TerminalScript);
            }

            Destroy(this, 20);

            m_IsInitialized = true;
        }
    }
}
