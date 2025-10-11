using System;
using NaughtyAttributes;
using UnityEngine;

namespace GameLib
{
    interface IDebugWidget
    {
        object GetSaveState();
        void SetSaveState(object state);
    }

    public class DebugWidgetBase : TrackableMonoBehaviourUniqueName<DebugWidgetBase>, IDebugWidget
    {
        [SerializeField, ReadOnly]
        private string _UID;

        public bool UpdateWhileDisabled;
        public bool PersistState = true;

        protected virtual void Reset()
        {
        }

        public void SaveState()
        {
            if (!PersistState) return;
            if (string.IsNullOrEmpty(_UID)) _UID = GenerateUID();

            var obj = GetSaveState();
            if (obj == null)
            {
                PlayerPrefs.DeleteKey(BuildPrefKey(_UID));
                return;
            }

            var json = JsonUtility.ToJson(obj);
            PlayerPrefs.SetString(BuildPrefKey(_UID), json);
            PlayerPrefs.Save();
        }

        public void LoadState()
        {
            if (!PersistState) return;
            if (string.IsNullOrEmpty(_UID)) return;

            var key = BuildPrefKey(_UID);
            if (!PlayerPrefs.HasKey(key)) return;

            var json = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return;

            // Let the derived widget decide how to interpret its own JSON
            SetSaveState(json);
        }
        
        protected void OnApplicationQuit()
        {
            if (!PersistState)
                return;
            SaveState(); 
        }

        // Derived widgets override both methods
        public virtual object GetSaveState() => null;
        public virtual void SetSaveState(object state) { }

        protected override void Awake()
        {
            base.Awake();
            if (PersistState)
                LoadState();
        }

        [Button]
        private void RegenerateUID() => _UID = GenerateUID();

        private string GenerateUID() => Guid.NewGuid().ToString("N");

        private static string BuildPrefKey(string uid) => $"GameLib.DebugWidget.{uid}";

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_UID))
            {
                _UID = GenerateUID();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
