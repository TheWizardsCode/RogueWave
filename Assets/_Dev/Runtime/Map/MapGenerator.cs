using UnityEngine;
using NaughtyAttributes;
using Unity.VisualScripting;
using System.Collections.Generic;
using TMPro;
using RogueWave;
using UnityEngine.UI;

namespace RogueWave
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField, Tooltip("The campaign definition to use for the map."), Expandable]
        private CampaignDefinition campaignDefinition;

        [SerializeField, Tooltip("The number of columns in the map.")]
        private int numOfColumns = 8;
        [SerializeField, Tooltip("The number of rows in the map.")]
        private int numOfRows = 4;
        [SerializeField, Tooltip("The parent object to hold the map.")]
        private RectTransform parent;

        [Header("UI Elements")]
        [SerializeField, Tooltip("The prefab to use for the level elements in the UI.")]
        private LevelUiController levelElementProtoytpe;
        [SerializeField, Tooltip("The UI element to display the selected level details.")]
        TMP_Text descriptionText;

        void Start()
        {
            GenerateMap();
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
                levelElement.Init(levelDefinition, descriptionText);
                levelElement.name = levelDefinition.DisplayName;
            }
        }
    }
}