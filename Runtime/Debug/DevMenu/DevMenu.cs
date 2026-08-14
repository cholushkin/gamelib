using UnityEngine;
using System.Collections.Generic;

namespace GameLib
{
    public class DevMenu : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject MenuPanel;
        public Transform MenuContent;
        public GameObject MenuItemPrefab;
        
        [Header("System References")]
        [Tooltip("The root object to destroy when the kill switch is triggered (e.g., the Dev Canvas)")]
        public GameObject RootSystemObject;

        private bool _isMenuOpen = false;
        private List<DevEntryMenuItem> _activeItems = new List<DevEntryMenuItem>();

        // Global Visibility State
        private bool _isSystemHidden = false;
        private HashSet<OverlayActivatorDevEntryButton> _savedActiveOverlays = new HashSet<OverlayActivatorDevEntryButton>();

        // Kill Switch State
        private int _killClicks = 0;
        private float _lastKillClickTime = 0f;
        private const float KILL_CLICK_TIMEOUT = 0.5f;

        void Start()
        {
            MenuPanel.SetActive(false);
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

            // Populate current entries 
            foreach (var activator in OverlayActivatorDevEntryButton.RegisteredActivators)
            {
                if (activator.Overlay == null) continue;

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

            if (_isSystemHidden)
            {
                _savedActiveOverlays.Clear();
                foreach (var activator in OverlayActivatorDevEntryButton.RegisteredActivators)
                {
                    if (activator.Overlay != null && activator.Overlay.IsShown())
                    {
                        // Save the state and hide it
                        _savedActiveOverlays.Add(activator);
                        activator.Overlay.Hide();
                    }
                }
            }
            else
            {
                foreach (var activator in _savedActiveOverlays)
                {
                    if (activator != null && activator.Overlay != null)
                    {
                        // Restore previously active overlays
                        activator.Overlay.Show();
                    }
                }
                _savedActiveOverlays.Clear();
            }

            UpdateUIStatus();
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