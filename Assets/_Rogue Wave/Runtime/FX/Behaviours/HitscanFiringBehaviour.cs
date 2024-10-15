using NaughtyAttributes;
using NeoFPS;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class HitscanFiringBehaviour : LineWeaponBehaviour
    {
        // Damage
        [SerializeField, Tooltip("The amount of damage this weapon will do to the player per second."), BoxGroup("Damage")]
        private float damageAmount = 2f;

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
