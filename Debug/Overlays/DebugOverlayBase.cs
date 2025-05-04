using UnityEngine;

namespace GameLib
{
    public class DebugOverlayBase : TrackableMonoBehaviour<DebugOverlayBase>
    {
        public int GroupdIndex;
        public Transform Content;
        public OverlayHandlerBase OverlayHandler;

        public virtual void Show()
        {
            ProccessGroupHide();
            
            if(Content)
                Content.gameObject.SetActive(true);
            
            if(OverlayHandler)
                OverlayHandler.OnOverlayToggle(true);
        }

        public virtual void Hide()
        {
            if(Content)
                Content.gameObject.SetActive(false);
            
            if(OverlayHandler)
                OverlayHandler.OnOverlayToggle(false);
        }

        public virtual bool IsShown()
        {
            return Content.gameObject.activeSelf;
        }

        public virtual void SetScale(float overlayScale)
        {
            
        }

        protected void ProccessGroupHide()
        {
            foreach (var overlay in GetAllInstances())
                if(overlay.GroupdIndex == GroupdIndex)
                    overlay.Hide();
        }
    }
}