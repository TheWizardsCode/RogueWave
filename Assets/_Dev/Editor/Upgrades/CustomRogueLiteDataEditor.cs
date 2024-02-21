#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RogueWave
{
    [CustomEditor(typeof(CustomRogueLiteData), true)]
    public class CustomRogueLiteDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var itr = serializedObject.FindProperty("m_CustomData");
            itr.NextVisible(true);

            do EditorGUILayout.PropertyField(itr, true);
            while (itr.NextVisible(false));

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif