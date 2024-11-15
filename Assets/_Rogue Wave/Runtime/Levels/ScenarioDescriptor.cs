using NaughtyAttributes;
using NeoFPS.CharacterMotion.States;
using NeoFPS.SinglePlayer;
using RogueWave;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// A ScenarioDescriptor is a scriptable object that describes a scenario, which is a specific configuration of the player
    /// and a level. Scenarios can be loaded independently of campaigns and represent single challenges that a player may
    /// undertake. They are also used extensively in testing and debugging where we setup encironments to recreate reported
    /// bugs.
    /// </summary>
    [CreateAssetMenu(fileName = "New Scenario Descriptor", menuName = "Rogue Wave/Scenario Descriptor")]
    public class ScenarioDescriptor : ScriptableObject
    {

        // Metadata
        [SerializeField, Tooltip("The name of the scenario."), BoxGroup("Metadata")]
        string m_DisplayName;
        [SerializeField, Tooltip("A description of the scenario."), TextArea(2, 5), BoxGroup("Metadata")]
        string m_Description;

        //[Header("Player Settings")]
        [SerializeField, Tooltip("The recipes the player should have on startup."), BoxGroup("Player Settings")]
        AbstractRecipe[] m_Recipes;

        //[Header("Level Settings")]
        [SerializeField, Tooltip("Terminal commands to execute once the player has spawned in."), TextArea(5, 10), BoxGroup("Level Settings")]
        string m_TerminalCommands;
        [SerializeField, Tooltip("The level definition to use when generating this level."), Expandable, BoxGroup("Level Settings")]
        WfcDefinition m_LevelDefinition;

        public string DisplayName { get { return m_DisplayName; } }
        public string Description { get { return m_Description; } }
        public WfcDefinition LevelDefinition { get { return m_LevelDefinition; } }
        public string TerminalScript { get { return m_TerminalCommands; } }
        public AbstractRecipe[] Recipes { get { return m_Recipes; } }

        public override string ToString()
        {
            return $"{DisplayName}: {Description}";
        }

        bool IsValid()
        {
            return IsValid(out string _);
        }

        private bool IsValid(out string message)
        {
            message = string.Empty;

            IsLevelDefinitionValid(out message);

            if (string.IsNullOrEmpty(message))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsLevelDefinitionValid()
        {
            return IsLevelDefinitionValid(out string _);
        }

        private bool IsLevelDefinitionValid(out string message)
        {
            message = string.Empty;

            if (m_LevelDefinition == null)
            {
                message = "You must set a level definition for the scenario.";
            }

            return string.IsNullOrEmpty(message);
        }

#if UNITY_EDITOR
        [Button("Create Level Definition"), DisableIf("IsLevelDefinitionValid")]
        public void CreateLevelDefinition()
        {
            // create a new level definition ScriptableObject and store it as a child of this asset
            WfcDefinition levelDefinition = CreateInstance<WfcDefinition>();
            levelDefinition.name = "_LevelDefinition";

            AssetDatabase.AddObjectToAsset(levelDefinition, this);
            m_LevelDefinition = levelDefinition;

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

//        [Button("Start Scenario", enabledMode: EButtonEnableMode.Editor), EnableIf("IsValid")]
//        public async void StartScenario()
//        {
//            string sceneName = "Assets/_Rogue Wave/Scenes/RogueWave_ScenarioPlayer.unity";

//#if UNITY_EDITOR
//            if (Application.isPlaying)
//            {
//                UnityEditor.SceneManagement.EditorSceneManager.LoadScene(sceneName);
//            }
//            else
//            {
//                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneName);
//                EditorApplication.isPlaying = true;
//            }
//#else
//            SceneManager.LoadScene(sceneName);
//#endif

//            CampaignDefinition campaign = RogueWaveGameMode.Instance.Campaign;
//            if (!campaign.name.StartsWith("Scenario"))
//            {
//                Debug.LogError("You must set the campaign in the game mode to a scenario campaign before starting a scenario session. This process overwrites settings and is destructive.");
//                return;
//            }
//            campaign.SetLevel(LevelDefinition);

//            // Configure the player
//            RogueLiteManager.persistentData.RecipeIds.Clear();
//            RogueLiteManager.runData.Clear();
//            RogueWaveGameMode.Instance.StartingRunRecipes = Recipes;

//            RogueWaveGameMode.Instance.gameObject.SetActive(true);

//#if UNITY_EDITOR
//            if (!Application.isPlaying)
//            {
//                UnityEditor.EditorApplication.isPlaying = true;
//            }
//#endif

//            if (!string.IsNullOrEmpty(TerminalScript))
//            {
//                TerminalCommands.RunScript(TerminalScript);
//            }
//        }
    }
}
