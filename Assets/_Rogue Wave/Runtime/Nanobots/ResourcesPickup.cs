using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using NeoFPS;

namespace RogueWave
{
    [RequireComponent(typeof(AudioSource))]
    public class ResourcesPickup : Pickup, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The type of resources to collect.")]
        private NanobotManager.ResourceType m_ResourceType = NanobotManager.ResourceType.Refined;
        [SerializeField, Tooltip("The amount of resources to collect.")]
        private int m_ResourcesAmount = 10;
        [SerializeField, Tooltip("A multiplier curve for the resources amount based on the current difficulty. This will be applied to the amount of resources collected.")]
        AnimationCurve resourceMultiplierByLevel = new AnimationCurve(new Keyframe(0, 3), new Keyframe(0.4f, 1), new Keyframe(1, 0.5f));
        [SerializeField, Tooltip("An event called when a character collects these resources.")]
        private UnityEvent m_OnResourcesCollected = null;

        private static readonly NeoSerializationKey k_EnabledKey = new NeoSerializationKey("pickupEnabled");

        private AudioSource m_AudioSource = null;
        private Collider m_Collider = null;
        private PooledObject m_PooledObject = null;
        private IEnumerator m_DelayedSpawnCoroutine = null;

        protected void Awake()
        {
            m_Collider = GetComponent<Collider>();
            m_AudioSource = GetComponent<AudioSource>();
            m_PooledObject = GetComponent<PooledObject>();
        }

        protected void OnEnable()
        {
            EnablePickup(true);
        }

        public override void Trigger(ICharacter character)
        {
            var nanobotManager = character.GetComponent<NanobotManager>();
            if (nanobotManager == null)
                return;

            m_OnResourcesCollected.Invoke();

            nanobotManager.CollectResources(Mathf.RoundToInt(m_ResourcesAmount * resourceMultiplierByLevel.Evaluate(FpsSettings.playstyle.difficulty)), m_ResourceType);
            DestroyPickup();
        }

        void DestroyPickup ()
        {
            if (m_DelayedSpawnCoroutine != null)
                StopCoroutine(m_DelayedSpawnCoroutine);
            
            if (m_AudioSource.clip != null)
                NeoFpsAudioManager.PlayEffectAudioAtPosition(m_AudioSource.clip, transform.position);
            
            if (m_PooledObject != null)
                m_PooledObject.ReturnToPool();
            else
                Destroy(gameObject);
        }

        public virtual void EnablePickup(bool value)
        {
            m_Collider.enabled = value;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (!m_Collider.enabled)
                writer.WriteValue(k_EnabledKey, false);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            bool pickupEnabled = true;
            reader.TryReadValue(k_EnabledKey, out pickupEnabled, true);
            EnablePickup(pickupEnabled);
        }
    }
}