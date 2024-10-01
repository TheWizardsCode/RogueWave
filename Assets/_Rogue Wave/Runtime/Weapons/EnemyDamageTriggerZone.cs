using NeoFPS;
using NeoFPS.Constants;
using UnityEngine;

namespace RogueWave
{
    public class EnemyDamageTriggerZone : MonoBehaviour
    {
        [SerializeField, Tooltip("The layers this trigger zone will detect.")]
        private LayerMask m_LayerMask = 0;
        [SerializeField, Tooltip("If true then the source of this trigger will be destroyed.")]
        private bool m_DestroySourceOnTrigger = true;
        [SerializeField, Tooltip("The default audio clip to play when the weapon hits a target.")]
        internal AudioClip[] hitAudioClips = default;

        float damage = 10f;

        internal void SetDamage(float damage)
        {
            this.damage = damage;
        }

        internal void SetHitAudioClips(AudioClip[] clips)
        {
            hitAudioClips = clips;
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((m_LayerMask.value & 1 << other.gameObject.layer) == 0)
            {
                return;
            }
            
            BasicDamageHandler damageHandler = other.GetComponent<BasicDamageHandler>();
            if (damageHandler == null)
            {
                return;
            }

            damageHandler.AddDamage(damage);
;
            FpsSurfaceMaterial surfaceMaterial = FpsSurfaceMaterial.Default;
            BaseSurface surface = other.transform.GetComponent<BaseSurface>();
            if (surface != null)
            {
                surfaceMaterial = surface.GetSurface();
                //SurfaceManager.ShowImpactFX(surfaceMaterial, transform.position, other.transform.forward);
                SurfaceManager.PlayImpactNoiseAtPosition(surfaceMaterial, transform.position, 1);
            }

            if (hitAudioClips != null && hitAudioClips.Length > 0) {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(hitAudioClips[Random.Range(0, hitAudioClips.Length)], transform.position);
            }

            if (m_DestroySourceOnTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
