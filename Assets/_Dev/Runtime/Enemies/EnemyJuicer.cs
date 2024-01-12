using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Playground
{
    /// <summary>
    /// EnemyJuicer is responsible for adding juice to the game when an enemy is spawned, injured, killed or other events.
    /// </summary>
    public class EnemyJuicer : MonoBehaviour
    {
        BasicEnemyController controller;

        private void Awake()
        {
            controller = GetComponent<BasicEnemyController>();
        }

        private void OnEnable()
        {
            controller.onDeath.AddListener(OnDeath);
        }

        void OnDeath(BasicEnemyController controller)
        {
            if (controller.config.juicePrefab == null)
            {
                return;
            }

            // TODO use pool for juice Object
            ParticleSystem deathParticle = Instantiate(controller.config.juicePrefab, transform.position, Quaternion.identity);
            if (controller.parentRenderer != null)
            {
                var particleSystemRenderer = deathParticle.GetComponent<ParticleSystemRenderer>();
                if (particleSystemRenderer != null)
                {
                    particleSystemRenderer.material = controller.parentRenderer.material;
                }
            }
            deathParticle.Play();

            if (controller.config.shouldExplodeOnDeath)
            {
                PooledExplosion explosion = deathParticle.GetComponent<PooledExplosion>();
                explosion.radius = controller.config.deathExplosionRadius;
                explosion.Explode(controller.config.explosionDamageOnDeath, controller.config.explosionForceOnDeath, null);
            }
        }
    }
}