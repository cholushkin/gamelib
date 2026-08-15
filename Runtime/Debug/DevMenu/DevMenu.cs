using UnityEngine;
using System.Collections.Generic;

// todo: sorting button, click cycles over different sorting modes(by shortcut, by name, by group and name)
// todo: transparent/opaque bg for overlays override
//
namespace GameLib
{
    public class DevMenu : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject MenuPanel;
        public Transform MenuContent;
        public GameObject MenuItemPrefab;
        
        [Header("Global Visibility Icons")]
        public GameObject GlobalVisibleIcon;
        public GameObject GlobalInvisibleIcon;
        
        [Header("System References")]
        [Tooltip("The root object to destroy when the kill switch is triggered (e.g., the Dev Canvas)")]
        public GameObject RootSystemObject;

        private bool _isMenuOpen = false;
        private List<DevEntryMenuItem> _activeItems = new List<DevEntryMenuItem>();

        // Cached Activators
        private OverlayActivatorDevEntryButton[] _allActivators;

        // Global Visibility State
        private bool _isSystemHidden = false;

        // Kill Switch State
        private int _killClicks = 0;
        private float _lastKillClickTime = 0f;
        private const float KILL_CLICK_TIMEOUT = 0.5f;

        void Start()
        {
            MenuPanel.SetActive(false);
            
            // Find and cache all activators in the scene once at startup.
            // FindObjectsInactive.Include ensures we find them even if they start disabled.
            _allActivators = FindObjectsByType<OverlayActivatorDevEntryButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            UpdateGlobalVisibilityIcons();
        }

        public void ToggleMenu()
        {
            _isMenuOpen = !_isMenuOpen;
            MenuPanel.SetActive(_isMenuOpen);

            if (_isMenuOpen)
            {
                RefreshMenu();
            }
        }

        private void RefreshMenu()
        {
            // Clear old entries
            foreach (Transform child in MenuContent)
            {
                Destroy(child.gameObject);
            }
            _activeItems.Clear();

            // Populate current entries using our cached array
            foreach (var activator in _allActivators)
            {
                if (activator == null || activator.Overlay == null) continue;

                var go = Instantiate(MenuItemPrefab, MenuContent);
                var menuItem = go.GetComponent<DevEntryMenuItem>();
                
                menuItem.Initialize(activator, this);
                _activeItems.Add(menuItem);
            }
        }
        
        public void UpdateUIStatus()
        {
            if (!_isMenuOpen) return;
            
            foreach(var item in _activeItems)
            {
                item.RefreshStatus();
            }
        }

        #region Additional Dev Logic

        public void ToggleGlobalVisibility()
        {
            _isSystemHidden = !_isSystemHidden;

            // Iterate through our cached array.
            // Since it's an array and not a dynamically updating list, 
            // disabling objects won't cause any collection modification crashes!
            foreach (var activator in _allActivators)
            {
                if (activator != null && activator.Overlay != null)
                {
                    activator.Overlay.gameObject.SetActive(!_isSystemHidden);
                }
            }

            UpdateGlobalVisibilityIcons();
        }

        private void UpdateGlobalVisibilityIcons()
        {
            if (GlobalVisibleIcon != null) GlobalVisibleIcon.SetActive(!_isSystemHidden);
            if (GlobalInvisibleIcon != null) GlobalInvisibleIcon.SetActive(_isSystemHidden);
        }

        public void AttemptKillSystem()
        {
            // Reset click count if too much time has passed
            if (Time.time - _lastKillClickTime > KILL_CLICK_TIMEOUT)
            {
                _killClicks = 0;
            }

            _killClicks++;
            _lastKillClickTime = Time.time;

            if (_killClicks >= 3)
            {
                // Nuke the entire system
                if (RootSystemObject != null)
                    Destroy(RootSystemObject);
                else
                    Destroy(gameObject); // Fallback to just destroying the menu
            }
        }

        #endregion
    }
}