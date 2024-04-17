using NaughtyAttributes;
using NeoFPS.ModularFirearms;
using NeoFPS.WieldableTools;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueWave
{
    /// <summary>
    /// A passive weapon is one which will fire automatically based on a timer.
    /// </summary>
    public class PassiveWeapon : MonoBehaviour
    {
        [Header("Firing")]
        [SerializeField, Tooltip("Cooldown between trigger pulls, in game seconds.")]
        internal float m_Cooldown = 5f;

        [Header("Damage")]
        [SerializeField, Tooltip("The maximum area of the damage.")]
        [FormerlySerializedAs("radius")]
        internal float range = 20f;
        [SerializeField, Tooltip("The damage applied to each enemy within the area of effect, each time the weapon fires.")]
        internal float damage = 50f;
        [SerializeField, Layer, Tooltip("The layers that the weapon will damage.")]
        internal int layers;

        [Header("Visuals")]
        [SerializeField, Tooltip("The model to display when the weapon is active.")]
        internal GameObject model;

        [Header("Audio")]
        [SerializeField, Tooltip("The audio clip to play when the weapon fires.")]
        internal AudioClip[] fireAudioClip = default;
        [SerializeField, Tooltip("The default audio clip to play when the weapon hits a target.")]
        internal AudioClip[] hitAudioClip = default;

        internal float m_NextFireTime = 0;
        ModularFirearm m_Firearm;
        AudioSource m_AudioSource;

        internal virtual void Awake()
        {
            m_Firearm = GetComponent<ModularFirearm>();
            m_AudioSource = GetComponent<AudioSource>();
        }

        public virtual bool isValid
        {
            get { return m_NextFireTime < Time.timeSinceLevelLoad; }
        }

        protected void Update()
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