#if UNITY_EDITOR

using UnityEditor;

namespace RogueWave
{
    [CustomEditor(typeof(CustomRogueLiteData), true)]
    public class CustomRogueLiteDataEditor : UnityEditor.Editor
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