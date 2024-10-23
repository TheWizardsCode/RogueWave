using NaughtyAttributes;
using NeoFPS;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class HitscanFiringBehaviour : LineWeaponBehaviour, IWeaponFiringBehaviour
    {
        // Damage
        [SerializeField, Tooltip("The amount of damage this weapon does when it hits the target."), BoxGroup("Damage")]
        private float damageAmount = 2f;

        public bool DamageOverTime { get => false; }
        public float DamageAmount { get => damageAmount; set => damageAmount = value; }

        public override void StartBehaviour(Transform target)
        {
            base.StartBehaviour(target);
        
            IDamageHandler damageHandler = target.GetComponent<IDamageHandler>();
            if (damageHandler != null)
            {
                damageHandler.AddDamage(damageAmount); ;
            }
        }
    }
}
