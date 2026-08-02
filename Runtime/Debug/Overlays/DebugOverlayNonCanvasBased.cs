using UnityEngine;

namespace GameLib
{
    public class DebugOverlayNonCanvasBased : DebugOverlayBase
    {
        public override void Show()
        {
            ProccessGroupHide();
            
            if(Content)
                Content.gameObject.SetActive(true);
            
            if(OverlayHandler)
                OverlayHandler.OnOverlayToggle(true);
        }

        public override void Hide()
        {
            if(Content)
                Content.gameObject.SetActive(false);
            
            if(OverlayHandler)
                OverlayHandler.OnOverlayToggle(false);
        }

        public override bool IsShown()
        {
            return Content.gameObject.activeSelf;
        }

        public override void SetScale(float overlayScale)
        {
        }
    }
}
