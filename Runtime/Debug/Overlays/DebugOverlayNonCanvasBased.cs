// todo: implement proper scaling support for non-canvas elements if needed
// idea: allow non-canvas overlays to use screen space coordinates instead of world space

using UnityEngine;

namespace GameLib
{
    public class DebugOverlayNonCanvasBased : DebugOverlayBase
    {
        public override void Show()
        {
            base.Show();
            ProccessGroupHide();
            
            if (Content)
                Content.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            base.Hide();
            if (Content)
                Content.gameObject.SetActive(false);
        }

        public override bool IsShown()
        {
            return Content != null && Content.gameObject.activeSelf;
        }

        public override void SetScale(float overlayScale)
        {
        }
    }
}