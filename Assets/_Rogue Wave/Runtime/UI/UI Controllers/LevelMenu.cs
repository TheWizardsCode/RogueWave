using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using RogueWave;
using System;
using System.Collections;
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
        [SerializeField, Tooltip("The stand-by panel for when the UI is appearing and disappearing.")]
        private RectTransform standbyPanel;

        [Header("Level Map")]
        [SerializeField, Tooltip("The parent object to hold the map.")]
        private RectTransform parent;

        private CampaignDefinition campaignDefinition;

        public override Selectable startingSelection
        {
            get { return null; }
        }

        private void Start()
        {
            campaignDefinition = FindObjectOfType<RogueWaveGameMode>().Campaign;
            GenerateMap();
        }

        private void OnEnable()
        {
            NeoFpsInputManager.captureMouseCursor = false;
            StartCoroutine(FadeStandyMessageIn());
            if (interfaceAnimationManager != null)
            {
                interfaceAnimationManager.OnEndAppear += OnAppear;
            }
        }

        private void OnAppear(InterfaceAnimManager _IAM)
        {
            HudHider.HideHUD();

            StartCoroutine(FadeStandyMessageOut());

            // Select the current level
            for (int i = 0; i < parent.childCount; i++)
            {
                if (i == RogueLiteManager.persistentData.currentGameLevel)
                {
                    Button button = parent.GetChild(i).GetComponent<Button>();
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                    button.onClick.Invoke();
                }
            }
        }

        private void OnDisable()
        {
            NeoFpsInputManager.captureMouseCursor = true;
            if (interfaceAnimationManager != null)
            {
                interfaceAnimationManager.OnEndAppear -= OnAppear;
                interfaceAnimationManager.OnEndDisappear -= _GenerateLevelAndSpawn;
            }

            HudHider.ShowHUD();
        }

        private IEnumerator FadeStandyMessageIn()
        {
            standbyPanel.gameObject.SetActive(true);
            CanvasGroup canvasGroup = standbyPanel.GetComponent<CanvasGroup>();
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FadeStandyMessageOut()
        {
            CanvasGroup canvasGroup = standbyPanel.GetComponent<CanvasGroup>();
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime;
                yield return null;
            }
            standbyPanel.gameObject.SetActive(false);
        }

        public void GenerateLevelAndSpawn()
        {

            StartCoroutine(FadeStandyMessageIn());
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

            // Add the level UI elements for each level in this campaign
            int columnIndex = 0;
            foreach (WfcDefinition levelDefinition in campaignDefinition.levels)
            {
                LevelUiController levelElement = Instantiate(levelElementProtoytpe, parent);
                levelElement.Init(levelDefinition);
                levelElement.name = levelDefinition.DisplayName;
                levelElement.OnLevelClicked += OnLevelClicked;
                
                columnIndex++;
            }
        }

        private void OnLevelClicked(LevelUiController controller)
        {
            RogueLiteManager.persistentData.currentGameLevel = Array.IndexOf(campaignDefinition.levels, controller.levelDefinition);
        }
    }
}
