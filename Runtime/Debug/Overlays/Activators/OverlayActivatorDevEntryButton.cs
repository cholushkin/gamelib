using UnityEngine;
using System.Collections.Generic;

namespace GameLib
{
    public class OverlayActivatorDevEntryButton : OverlayActivatorBase
    {
        [Tooltip("Leave empty to use the Overlay's GameObject name")]
        public string DisplayName;

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(DisplayName) && Overlay != null ? Overlay.name : DisplayName;
        }
    }
}