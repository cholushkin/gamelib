// todo: consider injecting LayeredInputConfig to dynamically resolve the "Debug" layer string safely
// idea: add an optional transition delay before blocking input

using UnityEngine;
using VContainer;

namespace GameLib
{
    public abstract class DebugOverlayBase : TrackableMonoBehaviour<DebugOverlayBase>
    {
        public Transform Content;
        public int GroupdIndex;

        protected ILayeredInputService InputService;

        /// Overlays that merely display information and shouldn't interrupt gameplay can override this to false.
        protected virtual bool BlocksInput => true;

        [Inject]
        public void Construct(ILayeredInputService inputService)
        {
            InputService = inputService;
        }

        public virtual void Show()
        {
            if (BlocksInput && InputService != null)
            {
                InputService.SetMinimumActiveLayer("Debug", this);
            }
        }

        public virtual void Hide()
        {
            if (BlocksInput && InputService != null)
            {
                InputService.ReleaseLayerBlock(this);
            }
        }

        public abstract bool IsShown();

        public abstract void SetScale(float overlayScale);

        protected void ProccessGroupHide()
        {
            foreach (var overlay in GetAllInstances())
            {
                if (overlay.GroupdIndex != GroupdIndex)
                {
                    overlay.Hide();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (BlocksInput && InputService != null)
            {
                InputService.ReleaseLayerBlock(this);
            }
        }
    }
}