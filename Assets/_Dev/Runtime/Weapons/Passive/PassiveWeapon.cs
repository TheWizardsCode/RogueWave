using NaughtyAttributes;
using RogueWave;
using UnityEngine;
using UnityEngine.Serialization;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// A passive weapon is one which will fire automatically based on a timer.
    /// </summary>
    public class PassiveWeapon : NanobotPawnUpgrade
    {
        [Header("Firing")]
        [SerializeField, Tooltip("Cooldown between trigger pulls, in game seconds.")]
        internal float m_Cooldown = 5f;

        [Header("Visuals")]
        [SerializeField, Tooltip("The model to display when the weapon is ready to fire.")]
        internal GameObject model;
        [SerializeField, Tooltip("The offset from the transform position to start the beam.")]
        protected Vector3 positionOffset = new Vector3(0, 0.7f, 0);
        [SerializeField, Tooltip("The muzzle flash to display when the weapon fires.")]
        internal ParticleSystem muzzleFlash;

        [Header("Audio")]
        [SerializeField, Tooltip("The audio source to play weapon fire sounds from.")]
        AudioSource m_FiringAudioSource;
        [SerializeField, Tooltip("The audio clip to play when the weapon fires.")]
        internal AudioClip[] fireAudioClip = default;
        [SerializeField, Tooltip("The default audio clip to play when the weapon hits a target.")]
        internal AudioClip[] hitAudioClip = default;

        [Header("Movement Behaviour")]
        [SerializeField, Tooltip("The speed of the weapons movement.")]
        internal float m_Speed = 5f;

        [Header("Ammo Behaviour")]
        [SerializeField, Tooltip("The layers that the weapon will damage.")]
        internal LayerMask layers;
        [SerializeField, Tooltip("The maximum range of the weapon. For area of effect weapons this is the radius, for other weapons it is the distance from the firing point the weapon will reach.")]
        internal float range = 20f;
        [SerializeField, Tooltip("The damage applied to each enemy within the area of effect, each time the weapon fires.")]
        internal float damage = 50f;

        internal float m_NextFireTime = 0;

        internal enum State
        {
            Ready,
            Firing
        }
        internal State m_currentState = State.Ready;
        internal virtual State currentState
        {
            get
            {
                return m_currentState;
            }
            set
            {
                if (m_currentState == value)
                {
                    return;
                }
                m_currentState = value;
            }
        }

        internal virtual void Awake()
        {
            if (m_FiringAudioSource == null)
            {
                m_FiringAudioSource = GetComponent<AudioSource>();
            }
            if (model != null)
            {
                model.transform.position += positionOffset;
            }

            m_NextFireTime = Time.timeSinceLevelLoad + (m_Cooldown / 4);
        }

        public virtual bool isReadyToFire
        {
            get { 
                if (currentState == State.Firing)
                {
                    return false;
                }
                else
                {
                    return m_NextFireTime < Time.timeSinceLevelLoad;
                }
            }
        }

        protected virtual void Update()
        {
            if (isReadyToFire)
            {
                m_NextFireTime = Time.timeSinceLevelLoad + m_Cooldown;
                Fire();
            }
        }

        public virtual void Fire()
        {
            PlayFireSFX();

            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }
        }


        internal void PlayFireSFX()
        {
            if (fireAudioClip.Length == 0)
            {
                return;
            }

            if (m_FiringAudioSource != null && fireAudioClip.Length > 0)
            {
                m_FiringAudioSource.clip = fireAudioClip[Random.Range(0, fireAudioClip.Length)];
                m_FiringAudioSource.Play();
            }
        }
    }
}