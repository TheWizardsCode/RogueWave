using NeoFPS;
using NeoFPS.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave
{
    public class EnemyDetails : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField, Tooltip("The height of this element when collapsed.")]
        private float collapsedHeight = 60f;
        [SerializeField, Tooltip("The height of this element when expanded.")]
        private float expandedHeight = 500f;
        [SerializeField, Tooltip("The UI element to display details about the enemy. This is a container that will be enabled/disabled when the title is clicked on.")]
        private RectTransform detailsContainer = null;

        [Header("Content")]
        [SerializeField, Tooltip("The UI element to display the enemy's name")]
        private TextMeshProUGUI textName = null;
        [SerializeField, Tooltip("The UI element to display the enemy's detailed description.")]
        private TextMeshProUGUI textDetails = null;
        [SerializeField, Tooltip("The UI element to display the enemy's challenge rating.")]
        private TextMeshProUGUI textCR = null;
        [SerializeField, Tooltip("The UI element to display the enemy's health.")]
        private TextMeshProUGUI textHealth = null;

        BasicEnemyController m_Enemy;
        internal BasicEnemyController enemy
        {
            get { return m_Enemy; }
            set
            {
                if (m_Enemy == value)
                {
                    return;
                }

                m_Enemy = value;
                if (m_Enemy != null)
                {
                    textName.text = m_Enemy.displayName;
                    textDetails.text = m_Enemy.description;
                    textCR.text = m_Enemy.challengeRating.ToString("000");
                    textHealth.text = m_Enemy.GetComponent<BasicHealthManager>().healthMax.ToString("0000");
                }
            }
        }

        private void OnEnable()
        {
            GetComponent<UiStyledButton>().onClick.AddListener(ToggleDetails);

            RectTransform rt = transform as RectTransform;
            Collapse(rt);
        }

        private void OnDisable()
        {
            GetComponent<UiStyledButton>().onClick.RemoveListener(ToggleDetails);
        }

        public void ToggleDetails()
        {
            RectTransform rt = transform as RectTransform;
            if (detailsContainer.gameObject.activeSelf)
            {
                Collapse(rt);
            }
            else
            {
                Expand(rt);
            }
        }

        private void Collapse(RectTransform rt)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, collapsedHeight);
            detailsContainer.gameObject.SetActive(false);
        }

        private void Expand(RectTransform rt)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, expandedHeight);
            detailsContainer.gameObject.SetActive(true);
        }
    }
}
