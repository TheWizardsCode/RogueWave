using NeoFPS;
using RogueWave;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RogueWave.Spawner;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// Handles damage to a shield. Almost all the damage is absorbed by the shield, but
    /// a little will be passed to the shield generators. Effects on the shield are generated.
    /// </summary>
    [DefaultExecutionOrder(900)]
    public class ShieldDamageHandler : BasicDamageHandler
    {
        [SerializeField, Tooltip("The percentage of damage that is passed through to the shield generators.")]
        float passThroughPercentage = 0.1f;
        [SerializeField, Tooltip("The particle system to play on the shield when it is hit. These will be placed at the point of impact when possible.")]
        ParticleSystem hitEffect;
        [SerializeField, Tooltip("The generator feedback particle to play when the shield is hit. This will be placed on the generator itself.")]
        PooledObject generatorFeedbackPrototype;

        Spawner spawner;
        List<BasicDamageHandler> generatorDamageHandlers = new List<BasicDamageHandler>();
        int layerMask;

        protected void Start()
        {
            spawner = GetComponentInParent<Spawner>();
            for (int i = 0; i < spawner.shieldGenerators.Count; i++)
            {
                generatorDamageHandlers.Add(spawner.shieldGenerators[i].gameObject.GetComponentInChildren<BasicDamageHandler>());
            }
        }

        public override DamageResult AddDamage(float damage)
        {
            Debug.LogWarning("ShieldDamageHandler.AddDamage(float damage) called but we are not playing hit FX from this method as we don't have a hit point.");
            PassThroughDamage(damage);
            GeneratorFeedbackFx();
            return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage, RaycastHit hit)
        {
            PassThroughDamage(damage);
            GeneratorFeedbackFx();
            HitFX(hit.point);
            return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage, IDamageSource source)
        {
            Debug.LogWarning("ShieldDamageHandler.AddDamage(float damage) called but we are not playing hit FX from this method as we don't have a hit point.");
            PassThroughDamage(damage);
            GeneratorFeedbackFx();
            return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
        {
            PassThroughDamage(damage);
            GeneratorFeedbackFx();
            HitFX(hit.point);
            return DamageResult.Blocked;
        }

        /// <summary>
        /// Spawn the hit FX on the shield.
        /// </summary>
        /// <param name="position">The position of the center of the effect.</param>
        private void HitFX(Vector3 position)
        {
            hitEffect.transform.position = position;
            hitEffect.Emit(1);
        }

        /// <summary>
        /// Create the feedback FX for the generators. This is intended to show the player that the shield is taking damage 
        /// and that they may want to shoot the generators directly.
        /// </summary>
        private void GeneratorFeedbackFx()
        {
            if (generatorFeedbackPrototype == null)
            {
                return;
            }

            for (int i = 0; i < spawner.shieldGenerators.Count; i++)
            {
                if (spawner.shieldGenerators[i].gameObject.activeSelf)
                {
                    ParticleSystem fx = PoolManager.GetPooledObject<ParticleSystem>(generatorFeedbackPrototype, generatorDamageHandlers[i].transform.position, Quaternion.identity);
                    PoolManager.ReturnObjectDelayed(fx.GetComponent<PooledObject>(), 1.5f);
                }
            }
        }

        /// <summary>
        /// Pass hrough some of the damage to the shield generators. This is intended to make the shield generators a target
        /// </summary>
        /// <param name="damage">The amount of damage to the shield, this will be reduced and passed to a generator.</param>
        private void PassThroughDamage(float damage)
        {
            float passThrough = Mathf.Max(1, damage * passThroughPercentage);
            if (generatorDamageHandlers.Last())
            {
                generatorDamageHandlers.Last().AddDamage(passThrough);
            }
            else
            {
                generatorDamageHandlers.RemoveAt(generatorDamageHandlers.Count - 1);
                generatorDamageHandlers.Last().AddDamage(passThrough);
            }
        }
    }
}
