using UnityEngine;
using System.Collections.Generic;

namespace GameLib
{
    public class OverlayActivatorDevEntryButton : OverlayActivatorBase
    {
        [Tooltip("Leave empty to use the Overlay's GameObject name")]
        public string DisplayName;

        // Static registry
        public static readonly List<OverlayActivatorDevEntryButton> RegisteredActivators = new List<OverlayActivatorDevEntryButton>();

        void OnEnable()
        {
            if (!RegisteredActivators.Contains(this))
                RegisteredActivators.Add(this);
        }

        void OnDisable()
        {
            RegisteredActivators.Remove(this);
        }

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(DisplayName) && Overlay != null ? Overlay.name : DisplayName;
        }
    }
}