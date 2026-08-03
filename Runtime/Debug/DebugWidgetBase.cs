// todo: add an Editor script to automatically generate the _UID if it is empty.
// idea: add an OnVisible() / OnHidden() callback so widgets can subscribe/unsubscribe from heavy events when toggled.

using UnityEngine;

namespace GameLib
{
    public abstract class DebugWidgetBase : MonoBehaviour
    {
        [SerializeField] private string _UID;
        public string UID => _UID;

        [Header("Configuration")]
        public bool PersistState = true;
        public WidgetUpdateStrategy UpdateStrategy = WidgetUpdateStrategy.WhenVisible;

        public virtual void Tick(float deltaTime) { }

        public virtual object GetSaveState() => null;
        public virtual void ApplySaveState(string jsonState) { }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(_UID))
            {
                _UID = System.Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}