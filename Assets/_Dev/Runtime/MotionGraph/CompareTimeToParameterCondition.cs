using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.Conditions;

namespace RogueWave
{
    [MotionGraphElement("Parameters/Compare Time To Parameter")]
    public class CompareTimeToParameterCondition : MotionGraphCondition
    {
        [SerializeField] private FloatParameter m_TimeValue = null;
        [SerializeField] private FloatParameter m_CompareValue = null;
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.Greater;

        public enum ComparisonType
        {
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_TimeValue != null)
            {
                float diff = Time.time - m_TimeValue.value;
                switch (m_ComparisonType)
                {
                    case ComparisonType.Greater:
                        return diff > m_CompareValue.value;
                    case ComparisonType.GreaterOrEqual:
                        return diff >= m_CompareValue.value;
                    case ComparisonType.Less:
                        return diff < m_CompareValue.value;
                    case ComparisonType.LessOrEqual:
                        return diff <= m_CompareValue.value;
                }
            }

            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_TimeValue = map.Swap(m_TimeValue);
            m_CompareValue = map.Swap(m_CompareValue);
        }
    }
}
