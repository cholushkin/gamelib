using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AddEmojiAttribute))]
public class AddEmojiDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AddEmojiAttribute emojiAttr = (AddEmojiAttribute)attribute;

        // Reserve space for the property
        Rect fieldRect = position;
        fieldRect.xMax -= 20 * emojiAttr.Emojis.Length; // space for emojis

        // Draw the property field
        EditorGUI.PropertyField(fieldRect, property, label, true);

        // Draw the emojis on the right
        float iconWidth = 20f;
        Rect iconRect = new Rect(fieldRect.xMax + 2, position.y, iconWidth, position.height);

        foreach (var emoji in emojiAttr.Emojis)
        {
            EditorGUI.LabelField(iconRect, emoji);
            iconRect.x += iconWidth;
        }
    }
}