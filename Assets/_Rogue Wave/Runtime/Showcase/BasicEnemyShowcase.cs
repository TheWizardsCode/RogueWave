using NeoFPS;
using NeoFPS.ModularFirearms;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using NaughtyAttributes;

namespace RogueWave.Editor
{
    [DefaultExecutionOrder(1000)]
    public class BasicEnemyShowcase : MonoBehaviour
    {
        [Header("Showcase Settings")]
        [SerializeField, Tooltip("Should this showcase be started automatically? If set to false then an external manager needs to start the showcase using the StartShowcase coroutine.")]
        bool startAutomatically = false;
        [SerializeField, Tooltip("The prefab of the enemy to showcase.")]
        BasicEnemyController enemy;
        [SerializeField, Tooltip("The speed at which the object should rotate.")]
        float rotationSpeed = 20f;
        [SerializeField, Tooltip("The duration of the showcase. Before this time the enemies health will be reduced to zero to simulate death. The time allocated for the death is set below in Death Duration.")]
        float showcaseDuration = 6;
        [SerializeField, Tooltip("The duration of the death animation.")]
        float deathDuration = 1;
        
        [Header("UI")]
        [SerializeField, Tooltip("Should the UI be displayed for this enemy?")]
        bool showUI = true;
        [SerializeField, Tooltip("The text component to display the name of the enemy. If null this will be ignored for this enemy."), ShowIf("showUI")]
        TMP_Text nameText;
        [SerializeField, Tooltip("The text component to display the description of the enemy. If null this will be ignored for this enemy."), ShowIf("showUI")]
        TMP_Text descriptionText;
        [SerializeField, Tooltip("The text component to display the challenge rating of the enemy. If null this will be ignored for this enemy."), ShowIf("showUI")]
        TMP_Text challengeRatingText;

        private EnemyLaser laser;
        private BaseTriggerBehaviour trigger;
        private BasicHealthManager healthManager;
        private BasicEnemyController enemyController;

        float currentAnimationDuration;
        private BasicMovementController movementController;

        private void Start()
        {
            if (startAutomatically)
            {
                StartCoroutine(StartShowcase());
            }
        }

        public IEnumerator StartShowcase()
        {
            currentAnimationDuration = showcaseDuration - deathDuration;

            Setup();

            StartCoroutine(Animate());

            StartCoroutine(Shoot());

            while (enemyController != null)
            {
                yield return null;
            }

            TearDown();
        }

        public void StopShowcase()
        {
            if (enemyController != null)
            {
                DestroyImmediate(enemyController.gameObject);
            }
        }

        IEnumerator Animate()
        {
            while (currentAnimationDuration > 0)
            {
                currentAnimationDuration -= Time.deltaTime;

                enemyController.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                yield return null;
            }

            if (healthManager != null)
            {
                healthManager.AddDamage(10000);
                yield return new WaitForSeconds(deathDuration);
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
            BasicMovementController movementController = GetComponentInChildren<BasicMovementController>();
            if (movementController != null)
            {
                movementController.enabled = true;
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
                movementController = GetComponentInChildren<BasicMovementController>();
                if (movementController != null)
                {
                    movementController.enabled = false;
                }
            } else
            {
                Debug.LogError($"No enemy controller for {this} to showcase.");
            }

            SetupUI();
        }

        private void SetupUI()
        {
            if (!showUI)
            {
                return;
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