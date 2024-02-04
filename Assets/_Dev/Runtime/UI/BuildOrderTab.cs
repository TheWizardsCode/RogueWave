using NeoFPS;
using NeoFPS.SinglePlayer;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Playground
{
    public class BuildOrderTab : InstantSwitchTabBase
    {
        [SerializeField, RequiredObjectProperty, Tooltip("The entire sortable list.")]
        private RectTransform sortableList = null;
        [SerializeField, Tooltip("The UI element to use to represent a weapon in the list. This will be cloned for each weapon.")]
        RectTransform recipePrefab;

        public override string tabName => "Build Order";

        private List<RectTransform> labels = new List<RectTransform>();

        void OnEnable()
        {
            int index = 0;
            foreach (string id in RogueLiteManager.persistentData.WeaponBuildOrder)
            {
                IRecipe recipe;
                if (RecipeManager.TryGetRecipeFor(id, out recipe))
                {
                    labels.Add(InstantiateRecipeElement(recipe.DisplayName, index));
                    index++;
                }
            }

            ConfigureMoveButtons();
        }

        RectTransform InstantiateRecipeElement(string name, int index)
        {
            RectTransform recipeUI = Instantiate(recipePrefab, sortableList);
            recipeUI.name = name;
            recipeUI.GetComponentInChildren<Text>().text = name;
            recipeUI.gameObject.SetActive(true);

            return recipeUI;
        }

        void MoveItem(int index, int direction)
        {
            if (index + direction < 0 || index + direction >= RogueLiteManager.persistentData.WeaponBuildOrder.Count)
                return;

            string id = RogueLiteManager.persistentData.WeaponBuildOrder[index];
            RogueLiteManager.persistentData.WeaponBuildOrder.RemoveAt(index);
            RogueLiteManager.persistentData.WeaponBuildOrder.Insert(index + direction, id);

            RectTransform recipeUI = labels[index];
            labels.RemoveAt(index);
            labels.Insert(index + direction, recipeUI);
            recipeUI.transform.SetSiblingIndex(index + direction + 1);
            ConfigureMoveButtons();

            RogueLiteManager.persistentData.isDirty = true;
        }

        private void ConfigureMoveButtons()
        {
            for (int i = 0; i < labels.Count; i++)
            {
                int index = i; // capture the index in a local variable so it's not changed by the time the button is clicked

                // TODO: it's brittle to search by name
                Button button = labels[index].transform.Find("Up Button").GetComponent<Button>();
                if (index > 0)
                {
                    button.gameObject.SetActive(true);
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => MoveItem(index, -1));
                }
                else
                {
                    button.gameObject.SetActive(false);
                }

                // TODO: it's brittle to search by name
                button = labels[index].transform.Find("Down Button").GetComponent<Button>();
                if (index < labels.Count - 1)
                {
                    button.gameObject.SetActive(true);
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => MoveItem(index, 1));
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }
}