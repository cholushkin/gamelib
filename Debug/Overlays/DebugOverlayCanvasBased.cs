using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    public class DebugOverlayCanvasBased : DebugOverlayBase
    {
        public bool DisableOnHide;
        [Required] public Canvas Canvas;
        [Required] public CanvasGroup CanvasGroup;
        [Required] public CanvasScaler CanvasScaler;

        public override void Show()
        {
            ProccessGroupHide();

            if (DisableOnHide)
                Content.gameObject.SetActive(true);

            CanvasGroup.alpha = 1; // Fully visible
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;

            if (OverlayHandler)
                OverlayHandler.OnOverlayToggle(true);
        }

        public override void Hide()
        {
            if (DisableOnHide)
                Content.gameObject.SetActive(false);

            CanvasGroup.alpha = 0; // Fully invisible
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;

            if (OverlayHandler)
                OverlayHandler.OnOverlayToggle(false);
        }

        public override bool IsShown()
        {
            return CanvasGroup.alpha >= 0.9f;
        }

        public override void SetScale(float overlayScale)
        {
            CanvasScaler.scaleFactor = overlayScale;
        }
    }
}