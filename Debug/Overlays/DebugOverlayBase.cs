using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Gamelib
{
    public class DebugOverlayBase : TrackableMonoBehaviour<DebugOverlayBase>
    {
        [Required] public Canvas Canvas;
        [Required] public CanvasGroup CanvasGroup;
        [Required] public CanvasScaler CanvasScaler;
        public int GroupdIndex;

        public void Show()
        {
            foreach (var overlay in GetAllInstances())
                if(overlay.GroupdIndex == GroupdIndex)
                    overlay.Hide();
            
            CanvasGroup.alpha = 1; // Fully visible
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            CanvasGroup.alpha = 0; // Fully invisible
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }

        public bool IsShown()
        {
            return CanvasGroup.alpha >= 0.9f;
        }

        public void SetScale(float overlayScale)
        {
            CanvasScaler.scaleFactor = overlayScale;
        }
    }
}