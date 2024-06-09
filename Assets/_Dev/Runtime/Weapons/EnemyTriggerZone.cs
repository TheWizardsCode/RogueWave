using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    public class EnemyTriggerZone : MonoBehaviour
    {
        [SerializeField, Tooltip("The layers this trigger zone will detect.")]
        private LayerMask m_LayerMask = 0;

        float damage = 10f;

        internal void SetDamage(float damage)
        {
            this.damage = damage;
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
        }
    }
}
