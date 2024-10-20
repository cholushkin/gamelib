using UnityEngine;

namespace GameLib.Dbg
{
    public class DebugGUI : MonoBehaviour
    {
        
        
        #region API
        // save state 
        // load state
        // showOverlay (index/name, additive/switch
        // hideOverlay (index/name)
        // hideOverlayAll
        // showControl(controlFullName)
        // hideControl(controlFullName)
        // 
        
        #endregion
        void OnDestroy()
        {
            SaveControlsStates();
        }

        void SaveControlsStates()
        {
            var elements = GetComponentsInChildren<DebugLayoutElement>(true);
            foreach (var debugLayoutElement in elements)
            {
                debugLayoutElement.SaveToState();
                debugLayoutElement.SaveState();
            }
        }
    }
}