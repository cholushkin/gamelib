using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameLib
{
    [RequireComponent(typeof(Button))]
    public class DevEntryMenuItem : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The single text element. Uses rich text for bold name and small group info.")]
        public TextMeshProUGUI MainText;
        
        [Header("Status Icons")]
        public GameObject VisibleIcon;
        public GameObject InvisibleIcon;

        private OverlayActivatorDevEntryButton _activator;
        private DevMenu _parentMenu;
        private Button _rootButton;

        void Awake()
        {
            // Grab the button on the root object
            _rootButton = GetComponent<Button>();
            _rootButton.onClick.AddListener(OnToggleClicked);
        }

        public void Initialize(OverlayActivatorDevEntryButton activator, DevMenu parentMenu)
        {
            _activator = activator;
            _parentMenu = parentMenu;
            
            // Format the text using TextMeshPro rich text tags
            string displayName = _activator.GetDisplayName();
            int groupIndex = _activator.Overlay != null ? _activator.Overlay.GroupdIndex : 0; 
            
            // <b> makes the first line bold
            // <size=75%> shrinks the second line
            // <alpha=#AA> (optional) would make the second line slightly transparent if you wanted
            MainText.text = $"<b>{displayName}</b>\n<size=75%>Group {groupIndex}</size>";
            
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
            
            // Swap the active icon
            if (VisibleIcon != null) VisibleIcon.SetActive(isShown);
            if (InvisibleIcon != null) InvisibleIcon.SetActive(!isShown);
        }
    }
}