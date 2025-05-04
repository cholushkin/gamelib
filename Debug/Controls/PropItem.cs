using TMPro;
using UnityEngine;

namespace GameLib
{
    public class PropItem : MonoBehaviour
    {
        public TextMeshProUGUI Text;
        public string FormatText;

        private void Reset()
        {
            FormatText = "{0} : {1}";
        }

        public void SetProperty(string propName, string valueText)
        {
            Text.text = string.Format(FormatText, propName, valueText);
        }
    }
}