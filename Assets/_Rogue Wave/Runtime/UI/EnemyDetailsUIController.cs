using NeoFPS;
using NeoFPS.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave
{
    public class EnemyDetailsUIController : MonoBehaviour
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
        [SerializeField, Tooltip("The sprite to display the enemy icon.")]
        private Image imageIcon = null;
        [SerializeField, Tooltip("The UI element to display the enemy's detailed description.")]
        private TextMeshProUGUI textDetails = null;
        [SerializeField, Tooltip("The UI element to display the enemy's challenge rating.")]
        private TextMeshProUGUI textCR = null;
        [SerializeField, Tooltip("The UI element to display the enemy's health.")]
        private TextMeshProUGUI textHealth = null;

        [Header("Defaults")]
        [SerializeField, Tooltip("The default icon to use if the enemy does not have one.")]
        private Sprite defaultIcon = null;

        ScrollRect parentScrollRect;

        int iconIndex = 0;
        float timeOfIconChange = 0f;
        float iconChangeInterval = 0.4f;

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

                    if (m_Enemy.icon.Length != 0)
                    {
                        imageIcon.sprite = m_Enemy.icon[iconIndex];
                        timeOfIconChange = Time.time + iconChangeInterval;
                    } else
                    {
                        imageIcon.sprite = defaultIcon;
                    }

                    textDetails.text = m_Enemy.description;
                    textCR.text = m_Enemy.challengeRating.ToString("000");
                    textHealth.text = m_Enemy.GetComponent<BasicHealthManager>().healthMax.ToString("0000");
                }
            }
        }

        private void OnGUI()
        {
            if (timeOfIconChange <= Time.realtimeSinceStartup)
            {
                iconIndex = (iconIndex + 1) % m_Enemy.icon.Length;
                imageIcon.sprite = m_Enemy.icon[iconIndex];
                timeOfIconChange = Time.realtimeSinceStartup + iconChangeInterval;
            }
        }

        private void OnEnable()
        {
            parentScrollRect = GetComponentInParent<ScrollRect>();
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

            if (parentScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                parentScrollRect.content.anchoredPosition = (Vector2)parentScrollRect.transform.InverseTransformPoint(parentScrollRect.content.position) - (Vector2)parentScrollRect.transform.InverseTransformPoint(rt.position);
            }
        }
    }
}
