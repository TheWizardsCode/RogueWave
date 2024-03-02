using NeoFPS;
using NeoFPS.ModularFirearms;
using RogueWave;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave.Editor
{
    public class BasicEnemyShowcase : MonoBehaviour
    {
        [SerializeField, Tooltip("The duration of the animation routine.")]
        float animationDuration = 5f;
        [SerializeField, Tooltip("The speed at which the object should rotate.")]
        float rotationSpeed = 20f;

        [Header("UI")]
        [SerializeField, Tooltip("The text component to display the name of the enemy. If null this will be ignored for this enemy.")]
        TMP_Text nameText;
        [SerializeField, Tooltip("The text component to display the description of the enemy. If null this will be ignored for this enemy.")]
        TMP_Text descriptionText;
        [SerializeField, Tooltip("The text component to display the challenge rating of the enemy. If null this will be ignored for this enemy.")]
        TMP_Text challengeRatingText;

        private EnemyLaser laser;
        private BaseTriggerBehaviour trigger;
        private BasicHealthManager healthManager;
        private BasicEnemyController enemyController;
        private bool isMobile;

        float currentAnimationDuration;

        internal IEnumerator StartShowcase()
        {
            currentAnimationDuration = 5f;

            Setup();

            StartCoroutine(Animate());

            StartCoroutine(Shoot());

            while (enemyController != null)
            {
                yield return null;
            }

            TearDown();
        }

        IEnumerator Animate()
        {
            yield return new WaitForSeconds(Random.Range(0, animationDuration * 0.2f));

            while (currentAnimationDuration > 0)
            {
                currentAnimationDuration -= Time.deltaTime;

                enemyController.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                yield return null;
            }

            if (healthManager != null)
            {
                healthManager.AddDamage(10000);
            }
        }

        IEnumerator Shoot()
        {
            yield return new WaitForSeconds(0.5f);

            if (trigger != null)
            {
                while (currentAnimationDuration > 0)
                {
                    trigger.Press();
                    yield return new WaitForSeconds(0.2f);

                    trigger.Release();
                    yield return new WaitForSeconds(0.8f + Random.Range(-0.1f, 0.1f));
                }
            } else if (laser != null)
            {
                while (currentAnimationDuration > 0)
                {
                    laser.state = EnemyLaser.State.Firing;
                    yield return new WaitForSeconds(0.8f + Random.Range(-0.1f, 0.1f));
                }
            }
        }

        private void TearDown()
        {
            if (enemyController != null)
            {
                enemyController.isMobile = isMobile;
            }
        }

        private void Setup()
        {
            laser = GetComponentInChildren<EnemyLaser>();
            trigger = GetComponentInChildren<BaseTriggerBehaviour>();
            healthManager = GetComponentInChildren<BasicHealthManager>();
            enemyController = GetComponentInChildren<BasicEnemyController>();
            if (enemyController != null)
            {
                isMobile = enemyController.isMobile;
                enemyController.isMobile = false;
            }

            if (nameText != null)
            {
                nameText.text = enemyController.displayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = enemyController.description;
            }

            if (challengeRatingText != null)
            {
                challengeRatingText.text = $"Challenge Rating: {enemyController.challengeRating}";
            }
        }
    }
}