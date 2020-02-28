using Baum2;
using UnityEditor;
using UnityEditor.UI;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// @author Kazuma Kuwabara
    /// </summary>
    [CustomEditor(typeof(DotScrollbar), true)]
    public sealed class DotScrollbarEditor : ScrollbarEditor
    {
        private SerializedProperty dotContainer;

        private SerializedProperty dotPrefab;

        private SerializedProperty isAutoLayoutEnableOnEditMode;

        protected override void OnEnable()
        {
            base.OnEnable();

            dotContainer = serializedObject.FindProperty(nameof(dotContainer));
            dotPrefab = serializedObject.FindProperty(nameof(dotPrefab));
            isAutoLayoutEnableOnEditMode = serializedObject.FindProperty(nameof(isAutoLayoutEnableOnEditMode));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Dot Settings");

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(dotContainer);
            EditorGUILayout.PropertyField(dotPrefab);
            EditorGUILayout.PropertyField(isAutoLayoutEnableOnEditMode);
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}