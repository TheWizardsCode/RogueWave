using RogueWave;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.RogueWave.UI
{
    [RequireComponent(typeof(RecipeTooltipTrigger))]
    public class RecipeListUIElement : MonoBehaviour
    {
        [SerializeField, Tooltip("The icon element for the recipe.")]
        protected Image icon = null;
        [SerializeField, Tooltip("The name of the recipe this item represents.")]
        protected TMP_Text nameText = null;

        RecipeTooltipTrigger m_TooltipTrigger;

        IRecipe m_recipe;
        /// <summary>
        /// The recipe to be displayed in this UI element.
        /// </summary>
        public IRecipe recipe
        {
            get { return m_recipe; }
            set
            {
                m_recipe = value;
                ConfigureUI();
                m_TooltipTrigger.Initialize(m_recipe, false);
            }
        }

        int m_instanceCount = 1;
        /// <summary>
        /// The number of this recipe the player has.
        /// </summary>
        public int count
        {
            get
            {
                return m_instanceCount;
            }
            set
            {
                if (m_instanceCount != value)
                {
                    m_instanceCount = value;
                    ConfigureUI();
                    m_TooltipTrigger.Initialize(m_recipe, false);
                }
            }
        }

        private void Awake()
        {
            m_TooltipTrigger = GetComponent<RecipeTooltipTrigger>();
        }

        protected virtual void ConfigureUI()
        {
            if (recipe == null)
            {
                gameObject.SetActive(false);
            }

            if (count > 1)
            {
                nameText.text = $"{count} x ";
            } else
            {
                nameText.text = string.Empty;
            }

            gameObject.name = recipe.DisplayName;
            if (string.IsNullOrEmpty(recipe.TechnicalSummary))
            {
                nameText.text += recipe.DisplayName;
            }
            else
            {
                nameText.text += $"{recipe.DisplayName} ({recipe.TechnicalSummary})";
            }

            icon.sprite = recipe.Icon;

            gameObject.SetActive(true);
        }
    }
}
