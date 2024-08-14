using RogueWave;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.RogueWave.UI
{
    public class RecipeListUIElement : MonoBehaviour
    {
        [SerializeField, Tooltip("The icon element for the recipe.")]
        protected Image icon = null;
        [SerializeField, Tooltip("The name of the recipe this item represents.")]
        protected TMP_Text nameText = null;

        IRecipe m_recipe;
        public IRecipe recipe
        {
            get { return m_recipe; }
            set
            {
                m_recipe = value;
                ConfigureUI();
            }
        }

        int m_instances = 1;
        public int instances
        {
            get
            {
                return m_instances;
            }
            set
            {
                if (m_instances != value)
                {
                    m_instances = value;
                    ConfigureUI();
                }
            }
        }

        protected virtual void ConfigureUI()
        {
            if (recipe == null)
            {
                gameObject.SetActive(false);
            }

            if (instances > 1)
            {
                nameText.text = $"{instances} x ";
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
