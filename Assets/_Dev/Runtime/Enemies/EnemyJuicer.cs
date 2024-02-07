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
    /// 
    /// Place it anywhere on an enemy and ensure that the Juics sections of the config are setup. The juics will be added at the location of this components transform.
    /// </summary>
    public class EnemyJuicer : MonoBehaviour
    {
        BasicEnemyController controller;

        private void Awake()
        {
            controller = GetComponentInParent<BasicEnemyController>();
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
            Vector3 pos = transform.position + controller.config.juiceOffset;
            ParticleSystem deathParticle = Instantiate(controller.config.juicePrefab, pos, Quaternion.identity);
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
                PooledExplosion explosion = deathParticle.GetComponentInChildren<PooledExplosion>();
                explosion.radius = controller.config.deathExplosionRadius;
                explosion.Explode(controller.config.explosionDamageOnDeath, controller.config.explosionForceOnDeath, null);
            }
        }
    }
}