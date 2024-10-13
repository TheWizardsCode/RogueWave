using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave
{
    public class SortableRecipeList : MonoBehaviour
    {
        [SerializeField, Tooltip("The UI element to use to represent a weapon in the list. This will be cloned for each weapon.")]
        RectTransform recipePrefab;

        private List<RectTransform> labels = new List<RectTransform>();

        void OnEnable()
        {
            int index = 0;
            foreach (string id in RogueLiteManager.persistentData.WeaponBuildOrder)
            {
                IRecipe recipe;
                if (RecipeManager.TryGetRecipe(id, out recipe))
                {
                    labels.Add(InstantiateRecipeElement(recipe.DisplayName, index));
                    index++;
                }
            }
        }

        void OnDisable()
        {
            labels.Clear();
        }

        RectTransform InstantiateRecipeElement(string name, int index)
        {
            RectTransform recipeUI = Instantiate(recipePrefab, transform);
            recipeUI.name = name;
            recipeUI.GetComponentInChildren<Text>().text = name;
            recipeUI.gameObject.SetActive(true);

            // TODO: Move this to a controller on the prefab itself, it's brittle to search by name
            Button button = recipeUI.transform.Find("Up Button").GetComponent<Button>();
            button.onClick.AddListener(() => MoveItem(index, -1));

            // TODO: Move this to a controller on the prefab itself, it's brittle to search by name
            button = recipeUI.transform.Find("Down Button").GetComponent<Button>();
            button.onClick.AddListener(() => MoveItem(index, 1));

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
            recipeUI.transform.SetSiblingIndex(index + direction);

            int newIndex = index + direction;
            // TODO: Move this to a controller on the prefab itself, it's brittle to search by name
            Button button = recipeUI.transform.Find("Up Button").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => MoveItem(newIndex, -1));

            // TODO: Move this to a controller on the prefab itself, it's brittle to search by name
            button = recipeUI.transform.Find("Down Button").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => MoveItem(newIndex, 1));

            RogueLiteManager.persistentData.isDirty = true;
        }
    }
}