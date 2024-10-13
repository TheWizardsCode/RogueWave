using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.RogueWave.CommandTerminal.InGame
{
    /// <summary>
    /// Provides a clickable tab that can be used to switch between different views. Attach this script
    /// to a UI element that provides OnClick and related events.
    /// </summary>
    public class TabController : MonoBehaviour
    {
        [SerializeField, Tooltip("Other tabs that should be deselected when this tab is selected.")]
        TabController[] otherTabs;
        [SerializeField, Tooltip("The UI elements that should be displayed when this tab is selected.")]
        RectTransform[] displayItems;
        [SerializeField, Tooltip("Should this tab be selected by default?")]
        bool isSelected = false;

        CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            Button button = GetComponent<Button>();
            button.onClick.AddListener(OnTabClicked);
        }

        private void Start()
        {
            SetSelected(isSelected, false);
        }

        private void OnTabClicked()
        {
            if (isSelected)
            {
                return;
            }

            SetSelected(true, true);
        }


        /// <summary>
        /// Set whether this tab is slected or note. If toggleOther is true then all other tabs will be deselected.
        /// </summary>
        /// <param name="value">If true this tab will be selected.</param>
        /// <param name="toggleOther">If true all `otherTabs` will be set to the opposite of this one.</param>
        internal void SetSelected(bool value, bool toggleOther)
        {
            if (isSelected == value)
            {
                return;
            }

            isSelected = value;

            foreach (TabController tab in otherTabs)
            {
                if (tab != this)
                {
                    tab.SetSelected(!isSelected, false);
                }
            }

            canvasGroup.alpha = isSelected ? 1 : 0.5f;

            foreach (RectTransform rect in displayItems)
            {
                rect.gameObject.SetActive(isSelected);
            }
        }
    }
}
