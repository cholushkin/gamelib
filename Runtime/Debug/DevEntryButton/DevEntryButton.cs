using UnityEngine;

namespace GameLib
{
    public class DevEntryButton : MonoBehaviour
    {
        [Header("References")]
        public FloatingWidget Widget;
        public DevMenu Menu;

        void OnEnable()
        {
            if (Widget != null && Menu != null)
            {
                Widget.OnClick.AddListener(Menu.ToggleMenu);
            }
        }

        void OnDisable()
        {
            if (Widget != null && Menu != null)
            {
                Widget.OnClick.RemoveListener(Menu.ToggleMenu);
            }
        }
    }
}