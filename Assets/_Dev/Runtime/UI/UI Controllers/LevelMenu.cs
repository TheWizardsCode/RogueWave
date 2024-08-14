using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using NUnit.Framework;
using RogueWave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizardsCode.RogueWave
{
    public class LevelMenu : PreSpawnPopupBase, IPointerClickHandler
    {
        [Header("Level Map")]
        [SerializeField, Tooltip("The campaign definition to use for the map."), Expandable]
        private CampaignDefinition campaignDefinition;

        [SerializeField, Tooltip("The number of columns in the map.")]
        private int numOfColumns = 8;
        [SerializeField, Tooltip("The number of rows in the map.")]
        private int numOfRows = 4;
        [SerializeField, Tooltip("The parent object to hold the map.")]
        private RectTransform parent;
        [SerializeField, Tooltip("The prefab to use for the level elements in the UI.")]
        private LevelUiController levelElementProtoytpe;
        [SerializeField, Tooltip("The location for details about the player character to be shown.")]
        private RectTransform playerCharacterInfo;

        public override Selectable startingSelection
        {
            get { return null; }
        }

        void Awake()
        {
            GenerateMap();
        }

        private void OnEnable()
        {
            NeoFpsInputManager.captureMouseCursor = false;
        }

        private void OnDisable()
        {
            NeoFpsInputManager.captureMouseCursor = true;
        }

        public void GenerateLevelAndSpawn()
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
