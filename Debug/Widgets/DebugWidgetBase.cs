using System;
using NaughtyAttributes;
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
        public string UID;
        public bool NeedApplyStateOnLoad() => false;
        public bool NeedUpdateWhileDisabled() => false;

        public void SaveState()
        {
        }

        public void LoadState(string jsontText)
        {
        }

        public virtual void Reset()
        {
            RegenerateUID();
        }

        [Button]
        private void RegenerateUID()
        {
            UID = GenerateUID();
        }
        
        private string GenerateUID()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}