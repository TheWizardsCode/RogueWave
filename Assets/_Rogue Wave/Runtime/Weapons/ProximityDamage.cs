using NeoFPS;
using PlasticGui.WorkspaceWindow;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// A basic weapon that continues to do damage while in contact.
    /// </summary>
    public class ProximityDamage : MonoBehaviour, IDamageSource
    {
        [SerializeField, Tooltip("The type of damage. You can manage damage types in the Neo FPS hub.")]
        private DamageType damageType = DamageType.Default;
        [SerializeField, Tooltip("The delay before damage is applied after contact is made. This is used to allow animations and effects time to play.")]
        private float damageDelay = 0f;
        [SerializeField, Tooltip("The total damage to apply per second.")]
        private float damagePerSecond = 10f;
        [SerializeField, Tooltip("The maximum amount of damage to apply. Once this is reached the enemy can no longer consume energy from the target. They will return to their spawn point. If this is set to 0 then there is no limit on the damage done.")]
        private float maxDamage = 10f;
        [SerializeField, Tooltip("A description of the damage to use in logs, etc.")]
        private string damageDescription = "Proximity";

        [Header("FX")]
        [SerializeField, Tooltip("The audio clip to play when the weapon comes into contact with a target.")]
        AudioClip[] contactClip;
        [SerializeField, Tooltip("The particle systemt to play when the weapon comes into contact with a target.")]
        ParticleSystem contactEffect;

        private DamageFilter _outDamageFilter = DamageFilter.AllDamageAllTeams;
        BasicEnemyController owner;

        private Dictionary<int, IDamageHandler> damageHandlers = new Dictionary<int, IDamageHandler>();
        private float inflictedDamageSinceRecharge;
        private float timeOfFirstContact;

        protected void Awake()
        {
            _outDamageFilter = new DamageFilter(damageType, DamageTeamFilter.All);
            owner = GetComponentInParent<BasicEnemyController>();
        }

        protected void OnTriggerEnter(Collider other)
        {
            IDamageHandler damageHandler = other.GetComponent<IDamageHandler>();
            if (damageHandler != null && damageHandlers.ContainsKey(other.gameObject.GetInstanceID()) == false)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(contactClip[Random.Range(0, contactClip.Length)], other.transform.position);
                if (contactEffect != null)
                {
                    contactEffect.Play();
                }
                damageHandlers.Add(other.gameObject.GetInstanceID(), damageHandler);

                timeOfFirstContact = Time.time;
            }
        }

        protected void OnTriggerStay(Collider other)
        {   
            if (timeOfFirstContact + damageDelay > Time.time)
            {
                return;
            }

            IDamageHandler handler;
            if (damageHandlers.TryGetValue(other.gameObject.GetInstanceID(), out handler))
            {
                float damage = damagePerSecond * Time.deltaTime / damageHandlers.Count;

                handler.AddDamage(damage, this);

                if (maxDamage > 0)
                {
                    inflictedDamageSinceRecharge += damage;
                    if (inflictedDamageSinceRecharge >= maxDamage)
                    {
                        inflictedDamageSinceRecharge = 0;
                        owner.IsRecharging = true;
                    }
                }
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            damageHandlers.Remove(other.gameObject.GetInstanceID());

            if (contactEffect != null)
            {
                contactEffect.Stop();
            }
        }

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
    }
}