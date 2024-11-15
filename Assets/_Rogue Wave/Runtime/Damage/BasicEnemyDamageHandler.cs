using NeoFPS;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class BasicEnemyDamageHandler : BasicDamageHandler
    {
        [SerializeField, Tooltip("The Damage Filter which deternines who can damage this enemy and what with.")]
        DamageFilter m_InDamageFilter = DamageFilter.AllNotPlayer;

        void Start()
        {
            inDamageFilter = m_InDamageFilter;
        }
    }
}
