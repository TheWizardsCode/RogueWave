#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using NeoFPSEditor.CharacterMotion;
using NeoFPS.CharacterMotion.Conditions;
using NeoFPS.CharacterMotion.Parameters;

namespace RogueWave
{
    [MotionGraphConditionDrawer(typeof(CompareTimeToParameterCondition))]
    public class CompareTimeToParameterConditionDrawer : MotionGraphConditionDrawer
    {
        protected override void Inspect(Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            Rect r3 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.2f;
            r3.width *= 0.3f;
            r2.x += r1.width;
            r3.x += r1.width + r2.width + 2f;
            r3.width -= 2f;
            r1.width -= 2f;

            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(r1, graphRoot, serializedObject.FindProperty("m_TimeValue"));

            // Draw the comparison type dropdown
            var comparisonTypeString = GetComparisonTypeString(serializedObject.FindProperty("m_ComparisonType").enumValueIndex);
            if (EditorGUI.DropdownButton(r2, new GUIContent(comparisonTypeString), FocusType.Passive))
            {
                GenericMenu gm = new GenericMenu();
                for (int i = 0; i < 4; ++i)
                    gm.AddItem(new GUIContent(GetComparisonTypeString(i)), false, OnComparisonTypeDropdownSelect, i);
                gm.ShowAsContext();
            }

            // Draw the compare value property
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(r3, graphRoot, serializedObject.FindProperty("m_CompareValue"));
        }

        void OnComparisonTypeDropdownSelect(object o)
        {
            int index = (int)o;
            serializedObject.FindProperty("m_ComparisonType").enumValueIndex = index;
            serializedObject.ApplyModifiedProperties();
        }

        string GetComparisonTypeString(int i)
        {
            switch (i)
            {
                case 0:
                    return ">";
                case 1:
                    return "> or =";
                case 2:
                    return "<";
                case 3:
                    return "< or =";
            }
            return "=";
        }
    }
}

#endif