// todo: transparent/opaque bg for overlays override
// idea: add a search/filter bar at the top if the list of registered overlays grows too large
// idea: persist the _isSystemHidden state between play sessions using PlayerPrefs or your save system

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace GameLib
{
    public class DevMenu : MonoBehaviour
    {
        public enum DevSortMode
        {
            Shortcut,
            Name,
            GroupAndName
        }

        [Header("UI References")]
        public GameObject MenuPanel;
        public Transform MenuContent;
        public GameObject MenuItemPrefab;
        
        [Header("Global Visibility Icons")]
        public GameObject GlobalVisibleIcon;
        public GameObject GlobalInvisibleIcon;
        
        [Header("Sorting References")]
        [Tooltip("Optional text label to display the current sort mode on your cycle button")]
        public TextMeshProUGUI SortModeText;
        
        [Header("System References")]
        [Tooltip("The root object to destroy when the kill switch is triggered (e.g., the Dev Canvas)")]
        public GameObject RootSystemObject;

        private bool _isMenuOpen = false;
        private List<DevEntryMenuItem> _activeItems = new List<DevEntryMenuItem>();

        // Cached Activators
        private OverlayActivatorDevMenu[] _allActivators;

        // System States
        private bool _isSystemHidden = false;
        private DevSortMode _currentSortMode = DevSortMode.Shortcut;
        private bool _isInitialized = false;

        // Kill Switch State
        private int _killClicks = 0;
        private float _lastKillClickTime = 0f;
        private const float KILL_CLICK_TIMEOUT = 0.5f;

        void Start()
        {
            if (MenuPanel != null)
            {
                // Sync the panel state with the bool, rather than hardcoding 'false'.
                // If ToggleMenu() woke this object up, _isMenuOpen will already be true!
                MenuPanel.SetActive(_isMenuOpen);
            }
        
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            // Find and cache all activators in the scene once.
            // FindObjectsInactive.Include ensures we find them even if they start disabled.
            _allActivators = FindObjectsByType<OverlayActivatorDevMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            UpdateGlobalVisibilityIcons();
            UpdateSortModeUI();

            _isInitialized = true;
        }

        public void ToggleMenu()
        {
            EnsureInitialized();

            _isMenuOpen = !_isMenuOpen;
            
            if (MenuPanel != null)
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

            if (_allActivators == null) return;

            // Apply sorting logic
            IEnumerable<OverlayActivatorDevMenu> sortedActivators = _allActivators;

            switch (_currentSortMode)
            {
                case DevSortMode.Shortcut:
                    sortedActivators = _allActivators
                        .OrderBy(a => GetShortcutSortKey(a))
                        .ThenBy(a => a.GetDisplayName());
                    break;
                case DevSortMode.Name:
                    sortedActivators = _allActivators
                        .OrderBy(a => a.GetDisplayName());
                    break;
                case DevSortMode.GroupAndName:
                    sortedActivators = _allActivators
                        .OrderBy(a => a.Overlay != null ? a.Overlay.GroupdIndex : int.MaxValue)
                        .ThenBy(a => a.GetDisplayName());
                    break;
            }

            // Populate current entries 
            foreach (var activator in sortedActivators)
            {
                if (activator == null || activator.Overlay == null) continue;

                var go = Instantiate(MenuItemPrefab, MenuContent);
                var menuItem = go.GetComponent<DevEntryMenuItem>();
                
                if (menuItem != null)
                {
                    menuItem.Initialize(activator, this);
                    _activeItems.Add(menuItem);
                }
            }
        }
        
        public void UpdateUIStatus()
        {
            if (!_isMenuOpen) return;
            
            foreach(var item in _activeItems)
            {
                if (item != null)
                {
                    item.RefreshStatus();
                }
            }
        }

        #region Additional Dev Logic

        public void CycleSortMode()
        {
            EnsureInitialized();

            // Cycle through the enum values (0, 1, 2)
            _currentSortMode = (DevSortMode)(((int)_currentSortMode + 1) % 3);
            
            UpdateSortModeUI();
            
            // Re-render the menu immediately if it's currently open
            if (_isMenuOpen)
            {
                RefreshMenu();
            }
        }

        private void UpdateSortModeUI()
        {
            if (SortModeText != null)
            {
                // Formats it nicely, e.g., "Sort: Group And Name"
                SortModeText.text = $"Sort: {_currentSortMode}";
            }
        }

        private string GetShortcutSortKey(OverlayActivatorDevMenu activator)
        {
            var keyboardActivator = activator.GetComponent<OverlayActivatorKeyboard>();
            
            if (keyboardActivator != null && keyboardActivator.Keys != null && keyboardActivator.Keys.Length > 0)
            {
                return keyboardActivator.Keys[0].ToString();
            }
            
            // Return a high Unicode character so items without shortcuts get pushed to the bottom of the list
            return "\uFFFF"; 
        }

        public void ToggleGlobalVisibility()
        {
            EnsureInitialized();

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