// todo: add a search/filter input field to find modules quickly by name or tag.
// idea: add a default placeholder sprite to the left panel if the selected module has no icon assigned.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace GameLib
{
    public class DebugWidgetGameLibModules : DebugWidgetBase
    {
        [Header("List configuration")] public RectTransform ListContainer;
        public DebugWidgetModuleListItem ItemPrefab;

        [Header("Selected Item Panel")] public Image SelectedIcon;
        public TextMeshProUGUI SelectedName;
        public TextMeshProUGUI SelectedVersion;
        public TextMeshProUGUI SelectedDescription;

        private IGameLibModuleRegistry _registry;
        private readonly List<DebugWidgetModuleListItem> _listItems = new();

        [Inject]
        public void Construct(IGameLibModuleRegistry registry)
        {
            _registry = registry;
        }

        private void Start()
        {
            PopulateList();
        }

        private void Reset()
        {
            // This widget only updates on UI clicks, so it doesn't need to be ticked by the service
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        private void PopulateList()
        {
            foreach (Transform child in ListContainer)
                Destroy(child.gameObject);

            _listItems.Clear();

            if (_registry == null || _registry.Modules == null || _registry.Modules.Count == 0)
            {
                ClearSelectionPanel();
                return;
            }

            foreach (var module in _registry.Modules)
            {
                var item = Instantiate(ItemPrefab, ListContainer);
                item.Setup(module, OnItemClicked);
                _listItems.Add(item);
            }

            // Auto-select the first module to populate the panel and trigger the scale effect
            if (_listItems.Count > 0)
            {
                OnItemClicked(_listItems[0], _registry.Modules[0]);
            }
        }

        private void OnItemClicked(DebugWidgetModuleListItem clickedItem, GameLibModuleManifest manifest)
        {
            // Update selection state (scaling) for all items in the list
            foreach (var item in _listItems)
            {
                item.SetSelected(item == clickedItem);
            }

            SelectedName.text = manifest.Name;
            SelectedVersion.text = $"v{manifest.Version}";
            SelectedDescription.text = manifest.Description;

            if (manifest.Icon != null)
            {
                SelectedIcon.sprite = manifest.Icon;
                SelectedIcon.enabled = true;
            }
            else
            {
                SelectedIcon.enabled = false;
            }
        }

        private void ClearSelectionPanel()
        {
            SelectedName.text = "No modules found";
            SelectedVersion.text = string.Empty;
            SelectedDescription.text = string.Empty;
            SelectedIcon.enabled = false;
        }
    }
}