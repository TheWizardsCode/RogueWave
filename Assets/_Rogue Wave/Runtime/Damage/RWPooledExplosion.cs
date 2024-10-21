using NaughtyAttributes;
using NeoFPS;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.RogueWave;

namespace RogueWave
{
    [RequireComponent(typeof(PooledObject))]
    public class RWPooledExplosion : PooledExplosion, IDamageSource
    {
        [SerializeField, Tooltip("The Damage Filter that determines what can be damaged by this enemy."), BoxGroup("Damage")]
        private DamageFilter m_ExplosionDamageFilter = DamageFilter.AllNotPlayer;

        [SerializeField, Tooltip("This list of particles will be adjested for the FX based on the object the explosion is triggered by. For example, they will have their colour adapted to match."), BoxGroup("Effects")]
        ParticleSystem[] _customizableParticles;

        [SerializeField, Tooltip("A game event to raise whenever the explosion is triggered."), BoxGroup("Events")]
        GameEvent _onExplosion;

        ParticleSystemRenderer[] _particleRenderers; 

        protected static List<IHealthManager> s_HealthManagers = new List<IHealthManager>(8);

        internal PooledObject PooledObject => m_PooledObject;
        #region IDamageSource implementation
        public new DamageFilter outDamageFilter
        {
            get
            {
                return m_ExplosionDamageFilter;
            }
            set
            {
                m_ExplosionDamageFilter = value;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            _particleRenderers = new ParticleSystemRenderer[_customizableParticles.Length];
            for (int i = 0; i < _customizableParticles.Length; i++)
            {
                _particleRenderers[i] = _customizableParticles[i].GetComponent<ParticleSystemRenderer>();
            }
        }

        internal Material ParticleMaterial
        {
            set
            {
                foreach (ParticleSystemRenderer particleRenderer in _particleRenderers)
                {
                    particleRenderer.material = value;
                }
            }
        }

        public override void Explode(float maxDamage, float maxForce, IDamageSource source = null, Transform ignoreRoot = null)
        {
            _onExplosion?.Raise();

            s_HealthManagers.Clear();
            base.Explode(maxDamage, maxForce, source, ignoreRoot);
        }

        protected override void CheckCollider(Collider c, Vector3 explosionCenter)
        {
            IHealthManager healthManager = c.GetComponentInParent<IHealthManager>();
            if (healthManager != null && s_HealthManagers.Contains(healthManager) == false)
            {
                s_HealthManagers.Add(healthManager);
                base.CheckCollider(c, explosionCenter);
            }
        }

#if UNITY_EDITOR
        internal bool IsValid(out string message, out Component component)
        {
            message = string.Empty;
            component = this;

            if (!ValidateRequiredFields(out message, out component)) return false;


            if (GetComponent<PooledObject>() == null) 
            { 
                message = "`RWPooledExplosion` requires a PooledObject component as a sibling.";
                return false;
            }

            return true;
        }

        private bool ValidateRequiredFields(out string message, out Component component)
        {
            message = string.Empty;
            component = this;

            var fields = GetType().GetFields();
            foreach (var field in fields)
            {
                var required = field.GetCustomAttributes(typeof(RequiredAttribute), true);
                if (required.Length > 0)
                {
                    if (field.GetValue(this) == null)
                    {
                        message = $"Field {field.Name} is required but not set in {name}.";
                        return false;
                    }
                }
            }

            return true;
        }
#endif
    }
}