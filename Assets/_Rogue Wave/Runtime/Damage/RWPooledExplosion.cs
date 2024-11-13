using NaughtyAttributes;
using NeoFPS;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.RogueWave;

namespace RogueWave
{
    [RequireComponent(typeof(PooledObject))]
    public class RWPooledExplosion : PooledExplosion, IDamageSource
    {
        [SerializeField, Tooltip("The Damage Filter that determines what can be damaged by this enemy."), BoxGroup("Damage")]
        private DamageFilter m_ExplosionDamageFilter = DamageFilter.AllNotPlayer;

        // Juice
        [SerializeField, Tooltip("Audio clips to select from when this expolosion occurs."), BoxGroup("Juice")]
        AudioClip[] _audioClips = new AudioClip[0];
        [SerializeField, Tooltip("This list of particles will be adjested for the FX based on the object the explosion is triggered by. For example, they will have their colour adapted to match."), BoxGroup("Juice")]
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

        protected override bool raycastCheck
        {
            get { return false; }
        }

        public override void Explode(float maxDamage, float maxForce, IDamageSource source = null, Transform ignoreRoot = null)
        {
            _onExplosion?.Raise();

            if (_audioClips.Length > 0)
            {
                AudioManager.Play3DOneShot(_audioClips[Random.Range(0, _audioClips.Length)], transform.position);
            }

            s_HealthManagers.Clear();
            base.Explode(maxDamage, maxForce, this, ignoreRoot);
        }

        protected override void ApplyExplosionDamageEffect(DamageHandlerInfo info)
        {
            CoroutineHelper.Instance.InvokeMethodWithDelay(ApplyExplosionDamageEffectDelayed, info, info.falloff / 3);
        }

        void ApplyExplosionDamageEffectDelayed(DamageHandlerInfo info)
        {
            float damage = maxDamage * info.falloff * info.damageShare;
            if (info.damageHandler != null && info.damageHandler.enabled)
                info.damageHandler.AddDamage(damage, this);
        }

        protected override void ApplyExplosionForceEffect(ImpactHandlerInfo info, Vector3 center)
        {
            CoroutineHelper.Instance.InvokeMethodWithDelay(ApplyExplosionForceEffectDelayed, info, center, info.falloff / 3);
        }

        protected void ApplyExplosionForceEffectDelayed(ImpactHandlerInfo info, Vector3 explosionCenter) {
            if (info.impactHandler != null)
                info.impactHandler.HandlePointImpact(info.collider.bounds.center, info.direction * (info.falloff * maxForce));
            else
            {
                var rigidbody = info.collider.attachedRigidbody;
                if (rigidbody != null)
                    rigidbody.AddExplosionForce(maxForce, explosionCenter, radius, 0.25f, ForceMode.Impulse);
            }
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
        internal bool IsValid(out string message)
        {
            message = string.Empty;

            if (GetComponent<PooledObject>() == null) 
            { 
                message = "`RWPooledExplosion` requires a PooledObject component as a sibling.";
                return false;
            }

            return true;
        }
#endif
    }
}