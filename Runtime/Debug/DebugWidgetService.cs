// todo: track Tick execution time per widget to emit a warning if a specific widget is causing a performance bottleneck.
// idea: expose a public manual Save() method so the user can save states via an ImGui button instead of waiting for app quit.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace GameLib
{
    public class DebugWidgetService : IInitializable, ITickable, IDisposable
    {
        private readonly DebugServiceConfig _config;
        private readonly List<DebugWidgetBase> _widgets = new();
        private readonly HashSet<Canvas> _checkedCanvases = new();

        [Serializable]
        private class WidgetStateWrapper
        {
            public string UID;
            public string StateJson;
        }

        [Serializable]
        private class ServiceSaveState
        {
            public List<WidgetStateWrapper> States = new();
        }

        [Inject]
        public DebugWidgetService(DebugServiceConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            Debug.Log("[DebugWidgetService] Booting up...");

            // 1. Auto-discover all widgets in the loaded scenes
            var foundWidgets = Object.FindObjectsByType<DebugWidgetBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _widgets.AddRange(foundWidgets);

            Debug.Log($"[DebugWidgetService] Discovered {_widgets.Count} widgets. Applying states...");

            if (_config.ValidateDebugStateOnStart)
                ValidateAndFixInitialState();

            // 2. Apply persistence
            LoadStates();
        }

        public void Tick()
        {
            if (_widgets.Count == 0) return;

            float dt = Time.deltaTime;

            foreach (var widget in _widgets)
            {
                if (widget == null) continue;

                bool isVisible = widget.gameObject.activeInHierarchy;

                if (widget.UpdateStrategy == WidgetUpdateStrategy.Always ||
                    (widget.UpdateStrategy == WidgetUpdateStrategy.WhenVisible && isVisible))
                {
                    widget.Tick(dt);
                }
            }
        }

        public void Dispose()
        {
            SaveStates();
        }

        private void ValidateAndFixInitialState()
        {
            foreach (var widget in _widgets)
            {
                if (string.IsNullOrEmpty(widget.UID) && _config.PrintWarning)
                {
                    Debug.LogWarning($"[DebugWidgetService] Widget '{widget.name}' has no UID! Persistence will be ignored.");
                }

                var canvas = widget.GetComponentInParent<Canvas>(true);
                if (canvas != null && _checkedCanvases.Add(canvas))
                {
                    if (canvas.sortingOrder < _config.RequiredCanvasSortOrder)
                    {
                        if (_config.FixState)
                        {
                            canvas.sortingOrder = _config.RequiredCanvasSortOrder;
                            if (_config.PrintWarning)
                                Debug.Log($"[DebugWidgetService] Auto-fixed Canvas '{canvas.name}' sort order to {_config.RequiredCanvasSortOrder}.");
                        }
                        else if (_config.PrintWarning)
                        {
                            Debug.LogWarning($"[DebugWidgetService] Canvas '{canvas.name}' has a low sort order ({canvas.sortingOrder}).");
                        }
                    }
                }
            }
        }

        private string GetSavePath() => Path.Combine(Application.persistentDataPath, _config.SaveFileName);

        private void SaveStates()
        {
            var saveState = new ServiceSaveState();

            foreach (var widget in _widgets)
            {
                if (widget == null || !widget.PersistState || string.IsNullOrEmpty(widget.UID)) continue;

                var stateObj = widget.GetSaveState();
                if (stateObj != null)
                {
                    saveState.States.Add(new WidgetStateWrapper
                    {
                        UID = widget.UID,
                        StateJson = JsonUtility.ToJson(stateObj)
                    });
                }
            }

            var json = JsonUtility.ToJson(saveState, true);
            File.WriteAllText(GetSavePath(), json);
        }

        private void LoadStates()
        {
            var path = GetSavePath();
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var saveState = JsonUtility.FromJson<ServiceSaveState>(json);
            if (saveState == null || saveState.States == null) return;

            var stateDict = new Dictionary<string, string>();
            foreach (var wrapper in saveState.States)
            {
                stateDict[wrapper.UID] = wrapper.StateJson;
            }

            foreach (var widget in _widgets)
            {
                if (widget == null || !widget.PersistState || string.IsNullOrEmpty(widget.UID)) continue;

                if (stateDict.TryGetValue(widget.UID, out var stateJson))
                {
                    widget.ApplySaveState(stateJson);
                }
            }
        }
    }
}