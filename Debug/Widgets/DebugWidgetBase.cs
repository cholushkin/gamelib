using UnityEngine;

namespace Gamelib
{
    interface IDebugWidget
    {
        bool NeedApplyStateOnLoad();
        bool NeedUpdateWhileDisabled();
        void SaveState();
        void LoadState(string jsontText);
    }

    public class DebugWidgetBase : MonoBehaviour, IDebugWidget
    {
        public bool NeedApplyStateOnLoad() => false;
        public bool NeedUpdateWhileDisabled() => false;

        public void SaveState()
        {
        }

        public void LoadState(string jsontText)
        {
        }
    }
}