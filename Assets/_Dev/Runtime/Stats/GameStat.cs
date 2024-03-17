using UnityEngine;

namespace WizardsCode.GameStats
{
    [CreateAssetMenu(fileName = "New GameStat", menuName = "Rogue Wave/Stats/Game Stat", order = 1)]
    public class GameStat : ScriptableObject
    {
        public enum StatType
        {
            Int,
            Float
        }

        [SerializeField, Tooltip("The key to use to store this stat in the GameStatsManager.")]
        string m_Key;
        [SerializeField, Tooltip("The type of stat.")]
        StatType m_StatType;

        public string Key => m_Key;
        public StatType Type => m_StatType;
    }
}
