using NaughtyAttributes;
using NeoFPS.ModularFirearms;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueWave
{
    /// <summary>
    /// A passive weapon is one which will fire automatically based on a timer.
    /// </summary>
    public class PassiveWeapon : NanobotPawnUpgrade
    {
        [Header("Firing")]
        [SerializeField, Tooltip("Cooldown between trigger pulls, in game seconds.")]
        internal float m_Cooldown = 5f;

        [Header("Damage")]
        [SerializeField, Tooltip("The maximum range of the weapon. For area of effect weapons this is the radius, for other weapons it is the total range.")]
        [FormerlySerializedAs("radius")]
        internal float range = 20f;
        [SerializeField, Tooltip("The damage applied to each enemy within the area of effect, each time the weapon fires.")]
        internal float damage = 50f;
        [SerializeField, Layer, Tooltip("The layers that the weapon will damage.")]
        internal int layers;

        [Header("Visuals")]
        [SerializeField, Tooltip("The model to display when the weapon is active.")]
        internal GameObject model;
        [SerializeField, Tooltip("The offset from the transform position to start the beam.")]
        protected Vector3 positionOffset = new Vector3(0, 0.7f, 0);

        [Header("Audio")]
        [SerializeField, Tooltip("The audio clip to play when the weapon fires.")]
        internal AudioClip[] fireAudioClip = default;
        [SerializeField, Tooltip("The default audio clip to play when the weapon hits a target.")]
        internal AudioClip[] hitAudioClip = default;

        internal float m_NextFireTime = 0;
        internal int layerMask;
        ModularFirearm m_Firearm;
        AudioSource m_AudioSource;

        internal virtual void Awake()
        {
            layerMask = 1 << layers;
            m_Firearm = GetComponent<ModularFirearm>();
            m_AudioSource = GetComponent<AudioSource>();
            if (model != null)
            {
                model.transform.position += positionOffset;
            }
        }

        public virtual bool isValid
        {
            get { return m_NextFireTime < Time.timeSinceLevelLoad; }
        }

        protected virtual void Update()
        {
            if (isValid)
            {
                Fire();
            }
        }

        public virtual void Fire()
        {
            PlayFireSFX();

            m_NextFireTime = Time.timeSinceLevelLoad + m_Cooldown;

            if (m_Firearm != null)
            {
                m_Firearm.trigger.Press();
            }
#if UNITY_EDITOR
            else if (this.GetType() == typeof(PassiveWeapon))
            {
                Debug.LogError($"{this.name} is equipped but there is no ModularFirearm component and no override of the `Fire` method.");
            }
#endif
        }

        internal void PlayFireSFX()
        {
            if (fireAudioClip.Length == 0)
            {
                return;
            }

            if (m_AudioSource != null && fireAudioClip.Length > 0)
            {
                m_AudioSource.clip = fireAudioClip[Random.Range(0, fireAudioClip.Length)];
                m_AudioSource.Play();
            }
        }
    }
}