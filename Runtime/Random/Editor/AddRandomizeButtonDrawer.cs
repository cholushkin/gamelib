using UnityEditor;
using UnityEngine;

namespace GameLib.Random
{
    [CustomPropertyDrawer(typeof(AddRandomizeButtonAttribute))]
    public class AddRandomizeButtonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.HelpBox(position, "[AddRandomizeButton] works only on uint fields", MessageType.Error);
                return;
            }

            // Split the rect into two parts: field and button
            var fieldRect = position;
            fieldRect.width -= 80f;

            var buttonRect = position;
            buttonRect.x += position.width - 75f;
            buttonRect.width = 70f;

            EditorGUI.PropertyField(fieldRect, property, label);

            if (UnityEngine.GUI.Button(buttonRect, "🎲 Rand"))
            {
                // Generate new non-trivial state (same logic as RandomHelper.MakeNonZeroState)
                uint raw = (uint)System.DateTime.UtcNow.Ticks;
                uint newState = raw ^ (raw >> 16);
                if (newState == 0) newState = 1;

                property.longValue = newState;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}