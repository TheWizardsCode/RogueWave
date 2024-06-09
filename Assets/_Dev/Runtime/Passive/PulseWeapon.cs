using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class PulseWeapon : PassiveWeapon
    {
        [Header("Pulse")]
        float m_pulseDuration = 1.5f;

        Collider[] colliders = new Collider[50];
        private Material material;
        
        internal override void Awake()
        {
            base.Awake();

            material = model.GetComponent<Renderer>().material;
            transform.localScale = Vector3.one * (range / 5);
        }

        public override void Fire()
        {
            StartCoroutine(Pulse());
        }

        IEnumerator Pulse() {
            float timer = 0;

            while (timer < m_pulseDuration)
            {
                float scale = Mathf.Lerp(0f, 1, timer / m_pulseDuration);
                material.SetFloat("_Size", scale);
                timer += Time.deltaTime;
                
                KeyValuePair<float, Collider> collider = nanobotPawn.PeekDetectedObject();

                if (collider.Value != null && collider.Key <= range * scale)
                {
                    IDamageHandler damageHandler = collider.Value.GetComponent<IDamageHandler>();

                    if (damageHandler != null)
                    {
                        damageHandler.AddDamage(damage);
                        if (hitAudioClip.Length > 0)
                        {
                            NeoFpsAudioManager.PlayEffectAudioAtPosition(hitAudioClip[UnityEngine.Random.Range(0, hitAudioClip.Length)], collider.Value.transform.position);
                        }
                    }
                }

                yield return null;
            }

            material.SetFloat("_Size", 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }   
    }
}