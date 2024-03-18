using NaughtyAttributes;
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
        [SerializeField, OnValueChanged("OnDefaultValueChangedCallback"), Tooltip("The default value of the stat.")]
        float m_DefaultValue = 0;

        [ShowNonSerializedField]
        int m_intValue;
        [ShowNonSerializedField]
        float m_floatValue;

        public string Key => m_Key;
        public StatType Type => m_StatType;

        internal void Reset()
        {
            m_intValue = 0;
            m_floatValue = 0;
        }

        public void SetValue(int value)
        {
            if (m_StatType == StatType.Float)
            {
                Debug.LogWarning("Asking to set an int value on a float stat. This will be cast to a float.");
                SetValue((float)value);
            }

            m_intValue = value;
        }

        public int GetIntValue()
        {
            if (m_StatType == StatType.Float)
            {
                Debug.LogWarning("Asking for an int value from a float stat. Float has been rounded to an int.");
                return Mathf.RoundToInt(m_floatValue);
            }
            return m_intValue;
        }

        public void SetValue(float value)
        {
            if (m_StatType == StatType.Int)
            {
                Debug.LogWarning("Asking to set a float value on an int stat. This will be rounded to an int.");
                value = Mathf.RoundToInt(value);
            }
            m_floatValue = value;
        }

        public float GetFloatValue()
        {
            if (m_StatType == StatType.Int)
            {
                Debug.LogWarning("Asking for a float value from an int stat. Int has been cast to a float.");
                return m_intValue;
            }
            return m_floatValue;
        }

        /// <summary>
        /// Increments an integer value by a set amount.
        /// </summary>
        /// <param name="amount">The amoun to increment the stat.</param>
        /// <returns>The new value of the stat.</returns>
        internal int Increment(int amount)
        {
            m_intValue += amount;
            return m_intValue;
        }

        /// <summary>
        /// Increments an float value by a set amount.
        /// </summary>
        /// <param name="amount">The amoun to increment the stat.</param>
        /// <returns>The new value of the stat.</returns>
        internal float Increment(float amount)
        {
            m_floatValue += amount;
            return m_floatValue;
        }

        private void OnDefaultValueChangedCallback()
        {
           switch (m_StatType)
            {
                case StatType.Int:
                    m_DefaultValue = Mathf.RoundToInt(m_DefaultValue);
                    break;
                case StatType.Float:
                    break;
            }
        }
    }
}
