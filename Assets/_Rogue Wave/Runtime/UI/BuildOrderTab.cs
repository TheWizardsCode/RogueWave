using NeoFPS;
using NeoFPS.SinglePlayer;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace RogueWave
{
    [Obsolete("This was used as part of the `RW_PreSpawnPopup` UI in an earlier version. It has been replaced by the `LevelManu` prefab at this point. We can probably get rid of this code now. (StO Aug 11th 2024))")]
    public class BuildOrderTab : InstantSwitchTabBase
    {
        [SerializeField, RequiredObjectProperty, Tooltip("The entire sortable list.")]
        private RectTransform sortableList = null;
        [SerializeField, Tooltip("The UI element to use to represent a weapon in the build order list. This will be cloned for each weapon.")]
        RectTransform buildPrefab;
        [SerializeField, Tooltip("The UI element to use to represent a weapon not in the build order list. This will be cloned for each weapon.")]
        RectTransform doNotBuildPrefab;

        public override string tabName => "Build Order";

        private List<BuildElement> builds = new List<BuildElement>();

        void OnEnable()
        {
            ConfigureUI();
        }

        private void ConfigureUI()
        {
            builds.Clear();
            foreach(RectTransform child in sortableList)
            {
                Destroy(child.gameObject);
            }

            int index = 0;
            foreach (string id in RogueLiteManager.persistentData.WeaponBuildOrder)
            {
                IRecipe recipe;
                if (RecipeManager.TryGetRecipe(id, out recipe))
                {
                    builds.Add(InstantiateBuildElement(recipe, index));
                    index++;
                }
            }

            foreach (string id in RogueLiteManager.persistentData.RecipeIds)
            {
                IRecipe recipe;
                if (RecipeManager.TryGetRecipe(id, out recipe)
                    && recipe is WeaponRecipe
                    && RogueLiteManager.persistentData.WeaponBuildOrder.Contains(id) == false)
                {
                    builds.Add(InstantiateDoNotBuildElement(recipe, index));
                    index++;
                }
            }

            ConfigureMoveButtons();
        }

        BuildElement InstantiateBuildElement(IRecipe recipe, int index)
        {   
            return InstantiateElement(recipe, index, buildPrefab);
        }

        BuildElement InstantiateDoNotBuildElement(IRecipe recipe, int index)
        {
            return InstantiateElement(recipe, index, doNotBuildPrefab);
        }

        BuildElement InstantiateElement(IRecipe recipe, int index, RectTransform prefab)
        {
            BuildElement result = new BuildElement();

            RectTransform recipeUI = Instantiate(prefab, sortableList);
            recipeUI.name = recipe.DisplayName;
            recipeUI.GetComponentInChildren<Text>().text = recipe.DisplayName;
            recipeUI.gameObject.SetActive(true);

            result.rectTransform = recipeUI;
            result.recipe = recipe;

            return result;
        }

        void MoveItem(int index, int direction)
        {
            if (index + direction < 0 || index + direction >= RogueLiteManager.persistentData.WeaponBuildOrder.Count)
                return;

            string id = RogueLiteManager.persistentData.WeaponBuildOrder[index];
            RogueLiteManager.persistentData.WeaponBuildOrder.RemoveAt(index);
            RogueLiteManager.persistentData.WeaponBuildOrder.Insert(index + direction, id);

            BuildElement recipeUI = builds[index];
            builds.RemoveAt(index);
            builds.Insert(index + direction, recipeUI);
            ConfigureUI();

            RogueLiteManager.persistentData.isDirty = true;
        }

        private void ConfigureMoveButtons()
        {
            for (int i = 0; i < builds.Count; i++)
            {
                int index = i; // capture the index in a local variable so it's not changed by the time the button is clicked

                // TODO: it's brittle to search bot the button by name
                Button button = builds[index].rectTransform.Find("Up Button")?.GetComponent<Button>();
                if (button != null)
                {
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
                }

                // TODO: it's brittle to search by name
                button = builds[index].rectTransform.Find("Down Button")?.GetComponent<Button>();
                if (button != null)
                {
                    if (index < RogueLiteManager.persistentData.WeaponBuildOrder.Count - 1)
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

                // TODO: it's brittle to search by name
                button = builds[index].rectTransform.Find("Remove Button")?.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => RemoveItemFromBuild(index));
                }

                // TODO: it's brittle to search by name
                button = builds[index].rectTransform.Find("Add Button")?.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => AddItemToBuild(index));
                }
            }
        }

        private void RemoveItemFromBuild(int index)
        {
            RogueLiteManager.persistentData.WeaponBuildOrder.RemoveAt(index);
            RogueLiteManager.persistentData.isDirty = true;

            ConfigureUI();
        }

        private void AddItemToBuild(int index)
        {
            RogueLiteManager.persistentData.WeaponBuildOrder.Add(builds[index].recipe.UniqueID);
            RogueLiteManager.persistentData.isDirty = true;

            ConfigureUI();
        }
    }

    class BuildElement
    {
        public IRecipe recipe { get; set; }
        public RectTransform rectTransform { get; set; }
    }
}