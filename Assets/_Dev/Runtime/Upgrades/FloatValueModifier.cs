using System;
using UnityEngine;

namespace RogueWave
{
    public class FloatValueModifier
    {
        public float multiplier { get; set; } = 1f;
        public float preMultiplyAdd { get; set; } = 0f;
        public float postMultiplyAdd { get; set; } = 0f;

        private float m_PersistentMultiplier = 1f;
        private float m_PersistentPreMultiplyAdd = 0f;
        private float m_PersistentPostMultiplyAdd = 0f;

        public FloatValueModifier(float persistentMultiplier, float persistentPreMultiplyAdd = 0f, float persistentPostMultiplyAdd = 0f)
        {
            m_PersistentMultiplier = persistentMultiplier;
            m_PersistentPreMultiplyAdd = persistentPreMultiplyAdd;
            m_PersistentPostMultiplyAdd = persistentPostMultiplyAdd;
        }

        public void SetPersistentValues(float persistentMultiplier, float persistentPreMultiplyAdd = 0f, float persistentPostMultiplyAdd = 0f)
        {
            m_PersistentMultiplier = persistentMultiplier;
            m_PersistentPreMultiplyAdd = persistentPreMultiplyAdd;
            m_PersistentPostMultiplyAdd = persistentPostMultiplyAdd;
        }

        public float GetModifiedValue(float unmodified)
        {
            return (unmodified + preMultiplyAdd + m_PersistentPreMultiplyAdd) * multiplier * m_PersistentMultiplier + postMultiplyAdd + m_PersistentPostMultiplyAdd;
        }
    }
}