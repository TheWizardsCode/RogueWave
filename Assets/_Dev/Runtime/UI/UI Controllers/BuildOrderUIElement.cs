using RogueWave;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.RogueWave.UI
{
    public class BuildOrderUIElement : RecipeListUIElement
    {
        [SerializeField, Tooltip("The button to move the item up in the list.")]
        internal Button moveUpButton = null;
        [SerializeField, Tooltip("The button to move the item down in the list.")]
        internal Button moveDownButton = null;
        [SerializeField, Tooltip("The button to take the item from the build order.")]
        internal Button removeButton = null;
        [SerializeField, Tooltip("The button to add the item to the build order.")]
        internal Button addButton = null;

        protected override void ConfigureUI()
        {
            base.ConfigureUI();
        }
    }
}
