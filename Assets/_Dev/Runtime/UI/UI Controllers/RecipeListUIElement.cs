using RogueWave;
using TMPro;
using UnityEngine;

namespace WizardsCode.RogueWave.UI
{
    public class RecipeListUIElement : MonoBehaviour
    {
        [SerializeField, Tooltip("The name of the recipe.")]
        private TMP_Text nameText = null;

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

        private void ConfigureUI()
        {
            if (recipe == null)
            {
                gameObject.SetActive(false);
            }

            gameObject.name = m_recipe.DisplayName;
            if (string.IsNullOrEmpty(m_recipe.TechnicalSummary))
            {
                nameText.text = recipe.DisplayName;
            }
            else
            {
                nameText.text = $"{recipe.DisplayName} ({recipe.TechnicalSummary})";
            }

            gameObject.SetActive(true);
        }
    }
}
