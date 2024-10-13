using NaughtyAttributes;
using NeoFPS;
using RogueWave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// The ExplodeOnContact class is a simple class that can be added to a GameObject to cause it to explode when it comes into contact with another object.
    /// It is assumed that the object will have a collider and a rigidbody.
    /// It is also assumed that this component will have been set up (damage, area of effect, layermask etc.) by a weapon controller, such as a PassiveWeaponController.
    /// </summary>
    public class ExplodeOnContact : MonoBehaviour, IDamageSource
    {
        [InfoBox("This behaviour will cause the object to explode when it comes into contact with another object. The settings of this component (e.g. damage, area of effect and layermask) can be set by calling the `Initiatlize` method, the values shown below are the defaults unless the application is running.")]

        [SerializeField, Tooltip("A description of the damage to use in logs, etc.")]
        private string damageDescription = "Fireball";
        [SerializeField, Tooltip("The damage filter to apply to the damage caused by this explosion. Only teams in this filter will be damaged by this weapon.")]
        private DamageFilter _outDamageFilter = DamageFilter.AllDamageAllTeams;

        [SerializeField, Tooltip("The prototype for the explosion to be created when the object comes into contact with another object."), Required]
        PooledObject enemyHitBehaviourProtype;

        [ShowNonSerializedField, Tooltip("Indicates if this explosion has been initialized. In the editor this will be false but it should be true when the object is enabled in game.")]
        bool isInitialized = false;
        [ShowNonSerializedField, Tooltip("The maximum damage that can be done by this explosion.")]
        private float m_MaxDamage = 10;
        private LayerMask m_LayerMask;
        [ShowNonSerializedField, Tooltip("The maximum force that can be applied by this explosion.")]
        private float m_MaxForce = 5;
        private PooledObject poolObject;

        public DamageFilter outDamageFilter
        {
            get { return _outDamageFilter; }
            set { _outDamageFilter = value; }
        }

        public IController controller
        {
            get { return null; }
        }

        public Transform damageSourceTransform
        {
            get { return transform; }
        }

        public string description
        {
            get { return damageDescription; }
        }

        private void Awake()
        {
            poolObject = GetComponent<PooledObject>(); 
        }

        private void OnDisable()
        {
            Reset();
        }

        private void Reset()
        {
            isInitialized = false;
        }

        internal void Initialize(float maxDamage, LayerMask layerMask)
        {
            isInitialized = true;
            m_MaxDamage = maxDamage;
            // m_MaxForce = maxForce;
            m_LayerMask = layerMask;
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (!isInitialized)
            {
                Debug.LogError($"ExplodeOnContact on {this.name} has not been initialized. This component must  be set up by calling `Initialize(...)` before use. Using default settings for now.");
            }

            // check if the other object is on the layermask
            if ((m_LayerMask.value & 1 << other.gameObject.layer) == 0)
            {
                return;
            }

            RWPooledExplosion explosion = PoolManager.GetPooledObject<RWPooledExplosion>(enemyHitBehaviourProtype, transform.position, Quaternion.identity);
            explosion.Explode(m_MaxDamage, m_MaxForce, this);
            
            poolObject.ReturnToPool();
        }

        private void OnValidate()
        {
            if (enemyHitBehaviourProtype != null && enemyHitBehaviourProtype.GetComponent<RWPooledExplosion>() == null)
            {
                Debug.LogError($"The enemyHitBehaviourProtype in {this.name} must have an RWPooledExplosion component.");
                enemyHitBehaviourProtype = null;
            }
        }
    }
}
