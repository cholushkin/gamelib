using UnityEngine;

namespace GameLib
{
    public abstract class DebugOverlayBase : TrackableMonoBehaviour<DebugOverlayBase>
    {
        public Transform Content;
        public int GroupdIndex;
        public OverlayHandlerBase OverlayHandler;

        public abstract void Show();

        public abstract void Hide();

        public abstract bool IsShown();

        public abstract void SetScale(float overlayScale);

        protected void ProccessGroupHide()
        {
            foreach (var overlay in GetAllInstances())
                if(overlay.GroupdIndex == GroupdIndex)
                    overlay.Hide();
        }
    }
}