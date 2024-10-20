using NaughtyAttributes;
using RogueWave;
using UnityEditor;
using UnityEngine;
using NeoFPS.SinglePlayer;
using NeoFPS.CharacterMotion;
using System;
using System.Threading.Tasks;
using NeoFPS;
using WizardsCode.CommandTerminal;
using WizardsCode.RogueWave;

namespace WizardsCode.Marketing
{
    [CreateAssetMenu(fileName = "New Gameplay Descriptor", menuName = "Wizards Code/Marketing/Gameplay Descriptor")]
    public class GameplayDescriptor : ScreenshotOnEventAssetDescriptor
    {
        [HorizontalLine(color: EColor.Gray)]

        //[Header("Level Settings")]
        [SerializeField, Tooltip("The level definition to use when generating this level."), Expandable, BoxGroup("Level Settings")]
        WfcDefinition m_LevelDefinition;
        [SerializeField, Tooltip("Terminal commands to execute once the player has spawned in."), TextArea(5, 10), BoxGroup("Level Settings")]
        string m_TerminalCommands;

        [HorizontalLine(color: EColor.Gray)]

        //[Header("Player Settings")]
        [SerializeField, Tooltip("The position of the player at the start of the gameplay."), BoxGroup("Player Settings")]
        Vector3 m_PlayerStartPosition;
        [SerializeField, Tooltip("The rotation of the player at the start of the gameplay."), BoxGroup("Player Settings")]
        Quaternion m_PlayerStartRotation;
        [SerializeField, Tooltip("The recipes the player should have on startup."), BoxGroup("Player Settings")]
        AbstractRecipe[] m_Recipes;

        [HorizontalLine(color: EColor.Gray)]

        //[Header("Audio Settings")]
        [SerializeField, Tooltip("Should the music be muted for this gameplay setting."), BoxGroup("Audio Settings")]
        bool m_MuteMusic = true;

        [HorizontalLine(color: EColor.Gray)]

        //[Header("Input Settings")]
        [SerializeField, Tooltip("Should mouse smoothing be enabled for this gameplay setting."), BoxGroup("Input Settings")]
        bool enableMouseSmoothing = false;
        [SerializeField, Tooltip("The amount of mouse smoothing to apply."), Range(0.01f, 1), BoxGroup("Input Settings"), ShowIf("enableMouseSmoothing")]
        float mouseSmoothingAmount = 0.5f;

        private RogueWaveGameMode gameMode;

        public override void LoadSceneSetup()
        {
            base.LoadSceneSetup();
            
            gameMode = FindObjectOfType<RogueWaveGameMode>();

            // Configure the level
            CampaignDefinition campaign = gameMode.Campaign;
            if (!campaign.name.StartsWith("Test"))
            {
                Debug.LogError("You must set the campaign to a test campaign before starting a gameplay session. This process overwrites settings and is destructive.");
                return;
            }
            campaign.SetLevel(m_LevelDefinition);

            // Configure the player
            RogueLiteManager.persistentData.RecipeIds.Clear();
            RogueLiteManager.runData.Recipes.Clear();
            gameMode.StartingRunRecipes = m_Recipes;
            EditorUtility.SetDirty(gameMode);

            _ = StartGamePlay();
        }

        public override void SaveSceneSetup()
        {
            base.SaveSceneSetup();

            SetPlayerStart();

            enableMouseSmoothing = FpsSettings.input.enableMouseSmoothing;
            mouseSmoothingAmount = FpsSettings.input.mouseSmoothing;
            
            gameMode = FindObjectOfType<RogueWaveGameMode>();

            CampaignDefinition campaign = gameMode.Campaign;
            if (campaign.name.StartsWith("Test"))
            {
                m_LevelDefinition = campaign.GetLevel();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "You must set the campaign to a test campaign before saving a gameplay session. This process overwrites settings and is destructive.", "OK");
            }

            m_Recipes = gameMode.StartingRunRecipes;
        }

        [Button]
        void SetPlayerStart()
        {
            if (EditorUtility.DisplayDialog("Confirm Action", "Are you sure you want to set the player start position?", "Yes", "No"))
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                m_PlayerStartPosition = sceneView.camera.transform.position;
                // the y value will be checked on teleport to ensure it is valid, set high now to ensure we are above anything generated in this space
                m_PlayerStartPosition.y = 1000;
                m_PlayerStartRotation = Quaternion.Euler(0, sceneView.camera.transform.rotation.eulerAngles.y, 0);
            }
        }

        public async Task StartGamePlay()
        {
            if (!Application.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }

            int waitIterations = 10000;
            while (waitIterations > 0 && FpsSoloCharacter.localPlayerCharacter == null)
            {
                waitIterations--;
                await Task.Delay(100);
            }

            // Execute terminal commands
            if (!string.IsNullOrEmpty(m_TerminalCommands))
            {
                TerminalCommands.RunScript(m_TerminalCommands);
            }

            // Ensure the start position is on the ground
            RaycastHit hit;
            if (Physics.Raycast(m_PlayerStartPosition, Vector3.down, out hit))
            {
                FpsSoloCharacter.localPlayerCharacter.GetComponent<MotionController>().characterController.Teleport(hit.point, m_PlayerStartRotation);
            }

            if (m_MuteMusic)
            {
                AudioManager.Instance.MuteMusicForSession();
            }

            FpsSettings.input.enableMouseSmoothing = enableMouseSmoothing;
            FpsSettings.input.mouseSmoothing = mouseSmoothingAmount;
        }
    }
}