using GameLib.Alg;
using NaughtyAttributes;
using UnityEngine;

namespace GameLib
{
    // - Saves/loads states 
    public class DebugWidgetsManager : Singleton<DebugWidgetsManager>
    {
        protected override void Awake()
        {
            base.Awake();

            // some checks
            transform.ForEachChildren(t =>
            {
                if (!t.gameObject.activeSelf)
                    Debug.LogWarning(
                        $"{t.name} is disabled on init. Normal starting state should CanvasGroup.alpha==0 or Content game object disabled for non-canvas overlays");
            });
        }


        #region API

        [Button]
        public void DestroyDebug()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}