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
        private bool _isSystemHidden = true; // START HIDDEN: Ensures the first single-click reveals the system
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
                MenuPanel.SetActive(_isMenuOpen);
            }
        
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            _allActivators = FindObjectsByType<OverlayActivatorDevMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            // 1. Show the first overlay by default so the first single click does something useful
            if (_allActivators != null && _allActivators.Length > 0)
            {
                var firstActivator = _allActivators
                    .OrderBy(a => GetShortcutSortKey(a))
                    .ThenBy(a => a.GetDisplayName())
                    .FirstOrDefault();
                    
                if (firstActivator != null && firstActivator.Overlay != null)
                {
                    firstActivator.Overlay.Show();
                }
            }

            // 2. Enforce the initial hidden state on all GameObjects so the screen starts clean
            foreach (var activator in _allActivators)
            {
                if (activator != null && activator.Overlay != null)
                {
                    activator.Overlay.gameObject.SetActive(!_isSystemHidden);
                }
            }

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
                // 3. Force the system to be globally visible when the menu opens
                // This ensures any toggled overlays immediately show up over the menu as configured!
                if (_isSystemHidden)
                {
                    ToggleGlobalVisibility();
                }
                
                RefreshMenu();
            }
        }

        private void RefreshMenu()
        {
            foreach (Transform child in MenuContent)
            {
                Destroy(child.gameObject);
            }
            _activeItems.Clear();

            if (_allActivators == null) return;

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

            _currentSortMode = (DevSortMode)(((int)_currentSortMode + 1) % 3);
            
            UpdateSortModeUI();
            
            if (_isMenuOpen)
            {
                RefreshMenu();
            }
        }

        private void UpdateSortModeUI()
        {
            if (SortModeText != null)
            {
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
            
            return "\uFFFF"; 
        }

        public void ToggleGlobalVisibility()
        {
            EnsureInitialized();
            
            _isSystemHidden = !_isSystemHidden;

            foreach (var activator in _allActivators)
            {
                if (activator != null && activator.Overlay != null)
                {
                    activator.Overlay.gameObject.SetActive(!_isSystemHidden);
                }
            }
        }

        public void AttemptKillSystem()
        {
            if (Time.time - _lastKillClickTime > KILL_CLICK_TIMEOUT)
            {
                _killClicks = 0;
            }

            _killClicks++;
            _lastKillClickTime = Time.time;

            if (_killClicks >= 3)
            {
                if (RootSystemObject != null)
                    Destroy(RootSystemObject);
                else
                    Destroy(gameObject);
            }
        }

        #endregion
    }
}