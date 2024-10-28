using NeoFPS.Samples;
using RogueWave.GameStats;
using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// Add this to a UI Element to enable or disable it based on a value in a GameStat.
    /// </summary>
    public class ElementEnablementController : MonoBehaviour
    {
        public enum ComparisonType
        {
            Equal,
            NotEqual,
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual
        }

        [SerializeField, Tooltip("The GameStat to check to determine if the element should be enabled or disabled.")]
        private GameStat<int> m_GameStatInt = null;
        [SerializeField, Tooltip("The value of the GameStat should be compared to.")]
        private int m_CompareValue = 0;
        [SerializeField, Tooltip("Should the element be enabled if the value is equal to, greater than, less than (etc.) the compare value?")]
        private ComparisonType m_ComparisonType = ComparisonType.GreaterOrEqual;
        
        private Selectable element;
        private MultiInputWidget multiInput = null;
        private bool previousInteractableState = true;

        private void Start()
        {
            element = GetComponent<MultiInputButton>();
            multiInput = element as MultiInputWidget;
            previousInteractableState = element.interactable;
        }

        private void Update()
        {
            if (m_GameStatInt == null || element == null) return;

            switch (m_ComparisonType)
            {
                case ComparisonType.Equal:
                    element.interactable = m_GameStatInt.value == m_CompareValue;
                    break;
                case ComparisonType.NotEqual:
                    element.interactable = m_GameStatInt.value != m_CompareValue;
                    break;
                case ComparisonType.Greater:
                    element.interactable = m_GameStatInt.value > m_CompareValue;
                    break;
                case ComparisonType.GreaterOrEqual:
                    element.interactable = m_GameStatInt.value >= m_CompareValue;
                    break;
                case ComparisonType.Less:
                    element.interactable = m_GameStatInt.value < m_CompareValue;
                    break;
                case ComparisonType.LessOrEqual:
                    element.interactable = m_GameStatInt.value <= m_CompareValue;
                    break;
            }

            if (multiInput && previousInteractableState != element.interactable)
            {
                multiInput.RefreshInteractable();
                previousInteractableState = element.interactable;
            }

        }
    }
}
