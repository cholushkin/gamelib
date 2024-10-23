using NaughtyAttributes;
using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetsManager : MonoBehaviour
    {

        #region API

        [Button]
        public void DestroyDebug()
        {
            Destroy(gameObject);
        }

        #endregion

    }
}
