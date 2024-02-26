using NaughtyAttributes;
using NeoFPS;
using System;
using System.Collections;
using UnityEngine;

namespace RogueWave
{
    public class PulseWeapon : PassiveWeapon
    {
        [Header("Damage")]
        [SerializeField, Tooltip("The maximum area of the damage.")]
        float radius = 20f;
        [SerializeField, Tooltip("The damage applied to each enemy within the area of effect, each time the weapon fires.")]
        float damage = 50f;
        [SerializeField, Layer, Tooltip("The layers that the weapon will damage.")]
        int layers;

        [Header("Visuals")]
        [SerializeField, Tooltip("The model to display when the weapon is active.")]
        GameObject model;

        MeshRenderer[] modelRenderers;

        Collider[] colliders = new Collider[50];
        private int layerMask;

        private void Awake()
        {
            layerMask = 1 << layers;
            modelRenderers = model.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in modelRenderers)
            {
                renderer.enabled = false;
            }
        }

        public override void Fire()
        {
            m_NextFireTime = Time.timeSinceLevelLoad + m_Cooldown;

            StartCoroutine(Pulse());
        }

        IEnumerator Pulse() {
            float duration = m_Cooldown / 2;
            float timer = 0;
            float height = radius;
            model.transform.localScale = Vector3.zero;

            foreach (MeshRenderer renderer in modelRenderers)
            {
                renderer.enabled = true;
            }

            Array.Clear(colliders, 0, colliders.Length);

            bool originalQueriesHitTriggers = Physics.queriesHitTriggers;
            Physics.queriesHitTriggers = false;
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, colliders, layerMask);
            Physics.queriesHitTriggers = originalQueriesHitTriggers;

            yield return null;

            int i = 0;
            while (timer < duration || i < count)
            {
                if (i < count)
                {
                    IDamageHandler damageHandler = colliders[i].GetComponent<IDamageHandler>();

                    if (damageHandler != null)
                    {
                        damageHandler.AddDamage(damage);
                    }

                    i++;
                }

                float scale = Mathf.Lerp(1, radius, timer / duration);
                model.transform.localScale = new Vector3(scale, height, scale);

                timer += Time.deltaTime;
             
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            foreach (MeshRenderer renderer in modelRenderers)
            {
                renderer.enabled = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }   
    }
}