using NaughtyAttributes;
using UnityEngine;

namespace WizardsCode.GameStats
{
    public class AchievementNotification : MonoBehaviour
    {
        [SerializeField, Tooltip("Set to true if this notification is a compact notification, showing the minimum of information.")]
        private bool isCompact = false;
        [SerializeField, Tooltip("The text component to use for the title of the achievement."), Required] 
        private TMPro.TextMeshProUGUI titleText;
        [HideIf("isCompact")]
        [SerializeField, Tooltip("The text component to use for the description of the achievement.")] 
        private TMPro.TextMeshProUGUI descriptionText;
        [SerializeField, Tooltip("The image component to use for the icon of the achievement."), Required] 
        private UnityEngine.UI.Image iconImage;

        internal float creationTime;
        CanvasGroup canvasGroup;

        private void Awake()
        {
            creationTime = Time.time;
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }

        public void SetAchievement(Achievement achievement)
        {
            titleText.text = achievement.displayName;
            if (!isCompact)
            {
                descriptionText.text = achievement.description;
            }
            iconImage.sprite = achievement.icon;

            canvasGroup.alpha = 0;
        }

        public void SetAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;
        }
    }
}
