using NaughtyAttributes;
using NeoFPS;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueWave
{
    public class PulseWeapon : PassiveWeapon
    {
        [Header("Pulse")]
        float m_pulseDuration = 1.5f;

        Collider[] colliders = new Collider[50];
        private int layerMask;
        private Material material;
        
        internal override void Awake()
        {
            base.Awake();
            layerMask = 1 << layers;
            material = model.GetComponent<Renderer>().material;
        }

        public override void Fire()
        {
            base.Fire();

            StartCoroutine(Pulse());
        }

        IEnumerator Pulse() {
            float timer = 0;

            Array.Clear(colliders, 0, colliders.Length);
            bool originalQueriesHitTriggers = Physics.queriesHitTriggers;
            Physics.queriesHitTriggers = false;
            int count = Physics.OverlapSphereNonAlloc(transform.position, range * 2, colliders, layerMask);
            Physics.queriesHitTriggers = originalQueriesHitTriggers;

            yield return null;

            int i = 0;
            while (timer < m_pulseDuration || i < count)
            {
                if (i < count && colliders[i] != null)
                {
                    IDamageHandler damageHandler = colliders[i].GetComponent<IDamageHandler>();

                    if (damageHandler != null)
                    {
                        damageHandler.AddDamage(damage);
                        if (hitAudioClip.Length > 0)
                        {
                            NeoFpsAudioManager.PlayEffectAudioAtPosition(hitAudioClip[UnityEngine.Random.Range(0, hitAudioClip.Length)], colliders[i].transform.position);
                        }
                    }

                    i++;
                }

                float scale = Mathf.Lerp(0f, 1, timer / m_pulseDuration);
                model.GetComponent<Renderer>().material.SetFloat("_Size", scale);

                timer += Time.deltaTime;
             
                yield return null;


                model.GetComponent<Renderer>().material.SetFloat("_Size", 0f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }   
    }
}