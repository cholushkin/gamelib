using System;
using GameLib.Random;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gamelib
{
    interface IDebugWidget
    {
        bool NeedApplyStateOnLoad();
        bool NeedUpdateWhileDisabled();
        void SaveState();
        void LoadState(string jsontText);
    }

    public class DebugWidgetBase : TrackableMonoBehaviour<DebugWidgetBase>, IDebugWidget 
    {
        [ReadOnly]
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