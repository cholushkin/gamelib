// todo: expose canvas sorting order to inspector for fine-tuned layering
// idea: add tweening/fade transitions for alpha when hiding or showing

using Alchemy.Inspector;
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
            base.Show();
            ProccessGroupHide();

            if (DisableOnHide && Content != null)
                Content.gameObject.SetActive(true);

            CanvasGroup.alpha = 1; 
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }

        public override void Hide()
        {
            base.Hide();
            if (DisableOnHide && Content != null)
                Content.gameObject.SetActive(false);

            CanvasGroup.alpha = 0; 
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }

        public override bool IsShown()
        {
            return CanvasGroup.alpha >= 0.9f;
        }

        public override void SetScale(float overlayScale)
        {
            if (CanvasScaler != null)
                CanvasScaler.scaleFactor = overlayScale;
        }
    }
}