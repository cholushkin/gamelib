// idea: different color of bg based on name of the overlay (colors come form predefined palette)

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameLib
{
    [RequireComponent(typeof(Button))]
    public class DevEntryMenuItem : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The single text element. Uses rich text for bold name, shortcuts, and group info.")]
        public TextMeshProUGUI MainText;
        
        [Header("Status Icons")]
        public GameObject VisibleIcon;
        public GameObject InvisibleIcon;

        private OverlayActivatorDevEntryButton _activator;
        private DevMenu _parentMenu;
        private Button _rootButton;

        void Awake()
        {
            _rootButton = GetComponent<Button>();
            _rootButton.onClick.AddListener(OnToggleClicked);
        }

        public void Initialize(OverlayActivatorDevEntryButton activator, DevMenu parentMenu)
        {
            _activator = activator;
            _parentMenu = parentMenu;
            
            string displayName = _activator.GetDisplayName();
            int groupIndex = _activator.Overlay != null ? _activator.Overlay.GroupdIndex : 0; 
            
            // Attempt to fetch an attached keyboard activator to display the shortcut
            string shortcutText = "";
            var keyboardActivator = _activator.GetComponent<OverlayActivatorKeyboard>();
            
            if (keyboardActivator != null && keyboardActivator.Keys != null && keyboardActivator.Keys.Length > 0)
            {
                // Adds a yellow highlighted shortcut tag, e.g., " [F1]"
                shortcutText = $" <color=#FFFF00>[{keyboardActivator.Keys[0]}]</color>";
            }
            
            MainText.text = $"<b>{displayName}</b>{shortcutText}\n<size=75%>Group {groupIndex}</size>";
            
            RefreshStatus();
        }

        private void OnToggleClicked()
        {
            if (_activator == null) return;
            
            _activator.ToggleOverlay();
            
            if (_parentMenu != null)
            {
                _parentMenu.UpdateUIStatus();
            }
            else
            {
                RefreshStatus(); 
            }
        }

        public void RefreshStatus()
        {
            if (_activator == null || _activator.Overlay == null) return;

            bool isShown = _activator.Overlay.IsShown();
            
            if (VisibleIcon != null) VisibleIcon.SetActive(isShown);
            if (InvisibleIcon != null) InvisibleIcon.SetActive(!isShown);
        }
    }
}