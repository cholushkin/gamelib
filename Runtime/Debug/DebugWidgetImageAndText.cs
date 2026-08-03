// todo: consider adding support for a lightweight animation library (like DOTween) to fade text/image colors.
// idea: add a format string field directly to this base class so all derived text widgets share the same formatting logic.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    public class DebugWidgetImageAndText : DebugWidgetBase
    {
        [Required] public Image Image;
        [Required] public TextMeshProUGUI Text;

        public void SetImage(Sprite sprite, Color color)
        {
            if (Image != null)
            {
                Image.sprite = sprite;
                Image.color = color;
            }
        }

        public void SetText(string text, Color color)
        {
            if (Text != null)
            {
                Text.text = text;
                Text.color = color;
            }
        }

        public Color GetTextColor() => Text != null ? Text.color : Color.white;
    }
}