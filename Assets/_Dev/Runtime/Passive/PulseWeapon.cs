using NaughtyAttributes;
using NeoFPS;
using System;
using UnityEngine;

namespace RogueWave
{
    public class PulseWeapon : PassiveWeapon
    {
        [SerializeField, Tooltip("The maximum area of the damage.")]
        float radius = 20f;
        [SerializeField, Tooltip("The damage applied to each enemy within the area of effect, each time the weapon fires.")]
        float damage = 50f;
        [SerializeField, Layer, Tooltip("The layers that the weapon will damage.")]
        int layers;

        Collider[] colliders = new Collider[50];
        private int layerMask;

        private void Awake()
        {
            layerMask = 1 << layers;
        }

        public override void Fire()
        {
            Debug.Log("Fire");
            m_NextFireTime = Time.timeSinceLevelLoad + m_Cooldown;

            Array.Clear(colliders, 0, colliders.Length);

            bool originalQueriesHitTriggers = Physics.queriesHitTriggers;
            Physics.queriesHitTriggers = false;
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, colliders, layerMask);
            Physics.queriesHitTriggers = originalQueriesHitTriggers;

            Debug.Log("Count: " + count);
            
            for (int i = 0;i < count; i++)
            {
                Debug.Log($"Checking: {colliders[i]}");

                IDamageHandler damageHandler = colliders[i].GetComponent<IDamageHandler>();

                if (damageHandler != null)
                {
                    Debug.Log($"Damage: {colliders[i]} for {damage}");
                    damageHandler.AddDamage(damage);
                }
                else
                {
                    Debug.Log($"No damage handler found for {colliders[i]}");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }   
    }
}