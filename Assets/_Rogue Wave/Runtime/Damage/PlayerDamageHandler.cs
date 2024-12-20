using NeoFPS;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class PlayerDamageHandler : ShieldedArmouredDamageHandler
    {
        [SerializeField, Tooltip("The Damage Filter which deternines who can damage this player.")]
        DamageFilter m_PlayerInDamageFilter = DamageFilter.AllNotPlayer;

        void Start()
        {
            inDamageFilter = m_PlayerInDamageFilter;
        }
    }
}
