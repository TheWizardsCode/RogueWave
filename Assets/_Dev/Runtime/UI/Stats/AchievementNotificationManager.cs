using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.GameStats
{
    public class AchievementNotificationManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The protoype to use for notifications that are created.")]
        private AchievementNotification achievementNotificationPrototype;

        float fadeInDuration = 0.5f;
        float fadeOutDuration = 0.5f;
        float displayDuration = 5f;
        Queue<AchievementNotification> notificationsQueue = new Queue<AchievementNotification>();
        AchievementNotification currentNotification;

        private void Start()
        {
            GameStatsManager.OnAchievementUnlocked += OnAchievementUnlocked;
        }

        private void OnAchievementUnlocked(Achievement achievement)
        {
            AchievementNotification notification = Instantiate(achievementNotificationPrototype, transform);
            notificationsQueue.Enqueue(notification);
            notification.SetAchievement(achievement);
        }

        private void Update()
        {
            if (currentNotification == null && notificationsQueue.Count > 0)
            {
                currentNotification = notificationsQueue.Peek();
            }

            if (currentNotification != null)
            {
                float time = Time.time - currentNotification.creationTime;

                if (time < fadeInDuration)
                {
                    currentNotification.SetAlpha(time / fadeInDuration);
                }
                else if (time < fadeInDuration + displayDuration)
                {
                    currentNotification.SetAlpha(1);
                }
                else if (time < fadeInDuration + displayDuration + fadeOutDuration)
                {
                    currentNotification.SetAlpha(1 - (time - fadeInDuration - displayDuration) / fadeOutDuration);
                }
                else
                {
                    notificationsQueue.Dequeue();
                    Destroy(currentNotification.gameObject);
                    currentNotification = null;
                }
            }
        }
    }
}