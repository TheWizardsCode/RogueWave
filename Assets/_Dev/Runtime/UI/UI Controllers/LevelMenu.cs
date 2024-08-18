using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using RogueWave;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizardsCode.RogueWave
{
    public class LevelMenu : PreSpawnPopupBase, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField, Tooltip("The Interface Animation Manager that will make the UI appear and dissapear.")]
        private InterfaceAnimManager interfaceAnimationManager;
        [SerializeField, Tooltip("The prefab to use for the level elements in the UI.")]
        private LevelUiController levelElementProtoytpe;
        [SerializeField, Tooltip("The stand by message for when the UI is appearing and disappearing.")]
        private TextMeshProUGUI standbyMessage;

        [Header("Level Map")]
        [SerializeField, Tooltip("The campaign definition to use for the map."), Expandable]
        private CampaignDefinition campaignDefinition;
        [SerializeField, Tooltip("The parent object to hold the map.")]
        private RectTransform parent;
        
        public override Selectable startingSelection
        {
            get { return null; }
        }

        private void Start()
        {
            GenerateMap();
        }

        private void OnEnable()
        {
            NeoFpsInputManager.captureMouseCursor = false;
            standbyMessage.gameObject.SetActive(true);
            if (interfaceAnimationManager != null)
            {
                interfaceAnimationManager.OnEndAppear += OnAppear;
                interfaceAnimationManager.startAppear();
            }
        }

        private void OnAppear(InterfaceAnimManager _IAM)
        {
            standbyMessage.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            NeoFpsInputManager.captureMouseCursor = true;
            if (interfaceAnimationManager != null)
            {
                interfaceAnimationManager.OnEndAppear -= OnAppear;
                interfaceAnimationManager.OnEndDisappear -= _GenerateLevelAndSpawn;
            }
        }

        public void GenerateLevelAndSpawn()
        {
            standbyMessage.gameObject.SetActive(true);
            if (interfaceAnimationManager != null)
            {
                interfaceAnimationManager.OnEndDisappear += _GenerateLevelAndSpawn;
                interfaceAnimationManager.startDisappear();
            }
        }

        private void _GenerateLevelAndSpawn(InterfaceAnimManager _IAM)
        {
            ((RogueWaveGameMode)gameMode).GenerateLevel();
            Spawn();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Empty to prevent clicks falling through and cancelling the popup
        }

        [Button]
        private void GenerateMap()
        {
            // remove all children of the parent object
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            int columnIndex = 0;
            foreach (WfcDefinition levelDefinition in campaignDefinition.levels)
            {
                columnIndex++;
                LevelUiController levelElement = Instantiate(levelElementProtoytpe, parent);
                levelElement.Init(levelDefinition);
                levelElement.name = levelDefinition.DisplayName;
                levelElement.OnLevelClicked += OnLevelClicked;
            }
        }

        private void OnLevelClicked(LevelUiController controller)
        {
            RogueLiteManager.persistentData.currentGameLevel = Array.IndexOf(campaignDefinition.levels, controller.levelDefinition);
        }
    }
}
