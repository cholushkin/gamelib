using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamelib
{
    public class DebugWidgetImageAndText : DebugWidgetBase
    {
        [Required] public Image Image;
        [Required] public TextMeshProUGUI Text;

        // SetImage - Set the sprite and color (including alpha) for the Image component
        public void SetImage(Sprite sprite, Color color)
        {
            if (Image != null)
            {
                Image.sprite = sprite;       // Set the sprite for the Image
                Image.color = color;         // Set the color (including alpha)
            }
        }

        // SetText - Set the text and color for the TextMeshProUGUI component
        public void SetText(string text, Color color)
        {
            if (Text != null)
            {
                Text.text = text;            // Set the text content
                Text.color = color;          // Set the color for the text
            }
        }

        public Color GetTextColor() => Text.color;
    }
}
