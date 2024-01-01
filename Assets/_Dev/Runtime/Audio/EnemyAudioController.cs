using NeoFPS;
using UnityEngine;

namespace Playground
{
    internal class EnemyAudioController : MonoBehaviour
    {
        [SerializeField, Tooltip("The Enemy Definition to use for default values if any of the values below are not set.")]
        EnemyDefinition defaults = null;

        [Header("Audio Source Configuration")]
        [SerializeField, Tooltip("The audio source for this enemies drone sound.")]
        AudioSource droneSource = null;

        [Header("Audio Clips")]
        [SerializeField, Tooltip("The drone sound to play for this enemy. If empty the default will be used. ")]
        AudioClip droneClip = null;
        [SerializeField, Tooltip("The sound to play when the enemy is killed. " +
            "If empty the settins in the enemy definition will be used.")]
        AudioClip[] deathClips;

        BasicEnemyController m_EnemyController = null;

        private void Awake()
        {
            m_EnemyController = GetComponent<BasicEnemyController>();
        }

        private void OnEnable()
        {
            m_EnemyController.onDeath.AddListener(OnDeath);
        }

        private void Start()
        {
            StartDrone();
        }

        private void OnDisable()
        {
            m_EnemyController.onDeath.RemoveListener(OnDeath);
            StopDrone();
        }

        protected void OnDeath()
        {
            droneSource.Stop();

            if (deathClips.Length > 0)
            {
                PlayOneShot(deathClips[Random.Range(0, deathClips.Length)], transform.position);
            } else
            {
                PlayOneShot(defaults.GetDeathClip(), transform.position);
            }
        }

        private void StartDrone()
        {
            if (droneClip != null)
            {
                droneSource.clip = droneClip;
            }
            else
            {
                droneSource.clip = defaults.droneClip;
            }
            droneSource.loop = true;
            droneSource.Play();
        }

        private void StopDrone()
        {
            droneSource.Stop();
        }

        static void PlayOneShot(AudioClip clip, Vector3 position)
        {
            // OPTIMIZATION Play only a limited number of death sounds within a certain time frame. Perhaps adding chorus or similar on subsequent calls
            NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, position);
        }
    }
}
