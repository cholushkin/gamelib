using GameLib.Random;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;


namespace Gamelib.Random
{
    [CustomPropertyDrawer(typeof(ShowAsRangeAttribute))]
    [CustomPropertyDrawer(typeof(int2))]
    [CustomPropertyDrawer(typeof(float2))]
    public class RangeDrawer : PropertyDrawer
    {
        static class Content
        {
            public static readonly GUIContent[] labels2 = { new GUIContent("From"), new GUIContent("To") };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (!EditorGUIUtility.wideMode)
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.type != "float2" && property.type != "int2")
            {
                EditorGUI.LabelField(position, label.text, "Use ShowAsRange with float2/int2 only.");
                return;
            }

            var subLabels = Content.labels2;
            var startIter = "x";
            label = EditorGUI.BeginProperty(position, label, property);
            var valuesIterator = property.FindPropertyRelative(startIter);
            MultiPropertyField(position, subLabels, valuesIterator, label);
            EditorGUI.EndProperty();
        }

        void MultiPropertyField(Rect position, GUIContent[] subLabels, SerializedProperty valuesIterator, GUIContent label)
        {
            EditorGUI.MultiPropertyField(position, subLabels, valuesIterator, label, EditorGUI.PropertyVisibility.All);
        }
    }
}