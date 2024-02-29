using NaughtyAttributes;
using NeoFPS;
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
        [SerializeField, Tooltip("The total damage to apply per second.")]
        private float damagePerSecond = 10f;
        [SerializeField, Tooltip("A description of the damage to use in logs, etc.")]
        private string damageDescription = "Proximity";

        [Header("Audio")]
        [SerializeField, Tooltip("The audio clip to play when the weapon comes into contact with a target.")]
        AudioClip[] contactClip;

        private DamageFilter _outDamageFilter = DamageFilter.AllDamageAllTeams;

        private Dictionary<int, IDamageHandler> damageHandlers = new Dictionary<int, IDamageHandler>();

        protected void Awake()
        {
            _outDamageFilter = new DamageFilter(damageType, DamageTeamFilter.All);
        }

        protected void OnTriggerEnter(Collider other)
        {
            IDamageHandler damageHandler = other.GetComponent<IDamageHandler>();
            if (damageHandler != null && damageHandlers.ContainsKey(other.gameObject.GetInstanceID()) == false)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(contactClip[Random.Range(0, contactClip.Length)], other.transform.position);
                damageHandlers.Add(other.gameObject.GetInstanceID(), damageHandler);
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            IDamageHandler handler;
            if (damageHandlers.TryGetValue(other.gameObject.GetInstanceID(), out handler))
                handler.AddDamage(damagePerSecond * Time.deltaTime, this);
        }

        protected void OnTriggerExit(Collider other)
        {
            damageHandlers.Remove(other.gameObject.GetInstanceID());
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