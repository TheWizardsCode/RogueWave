using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace RogueWave.Marketing
{
    internal class FireProjectiles : MonoBehaviour
    {
        [SerializeField]
        Transform spawnPosition;
        [SerializeField]
        float spawnOffset = 0.3f;
        [SerializeField]
        GameObject projectilePrototype = default;
        [SerializeField]
        float speed = 1000;
        [SerializeField, Tooltip("The target to aim the projectile at.")]
        Transform target = default;

        [Header("Auto Fire")]
        [SerializeField, Tooltip("Should the projectile fire automatically?")]
        bool autoFire = true;
        [SerializeField, ShowIf("autoFire")]
        float coolDown = 1.5f;

        [Header("Animation")]
        [SerializeField, Tooltip("The animation controller trigger to start the cast animations.")]
        string animationTrigger = "RH+SwingCast_Fast";

        Animator animator;

        IEnumerator Start()
        {
            animator = GetComponent<Animator>();

            while (autoFire)
            {
                Fire();
                yield return new WaitForSeconds(coolDown);
            }
        }

        /// <summary>
        /// This is the Animation Event Trigger used by KryptoFX.
        /// </summary>
        public void ActivateEffect()
        {
            FireProjectile();
        }

        public void Fire()
        {
            animator.SetTrigger(animationTrigger);
        }

        void FireProjectile()
        {
            Vector3 spawnPositionWithOffset = spawnPosition.position + spawnPosition.forward * spawnOffset;
            GameObject projectile = Instantiate(projectilePrototype, spawnPositionWithOffset, Quaternion.identity);
            Vector3 directionToTarget = (target.position - spawnPositionWithOffset).normalized;
            projectile.GetComponent<Rigidbody>().AddForce(directionToTarget * speed);
        }
    }
}
