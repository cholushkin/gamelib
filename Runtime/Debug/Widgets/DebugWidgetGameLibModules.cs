// todo: add a search bar above the container to filter the instantiated module items by name.
// idea: add a sorting dropdown to order modules alphabetically or by version number.

using GameLib;
using System.Linq;
using UnityEngine;
using VContainer;

public class DebugWidgetGameLibModules : DebugWidgetBase
{
    public DebugWidgetRegistryModuleItem ItemPrefab;

    private IGameLibModuleRegistry _registry;

    [Inject]
    public void Construct(IGameLibModuleRegistry registry)
    {
        _registry = registry;
    }

    protected void Start()
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
        // Clear existing children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (_registry == null || _registry.Modules == null || _registry.Modules.Count == 0)
        {
            return;
        }

        // Sort modules alphabetically by name
        var sortedModules = _registry.Modules.OrderBy(module => module.Name);

        foreach (var module in sortedModules)
        {
            // Instantiate directly under this widget's transform
            var item = Instantiate(ItemPrefab, transform);
            item.Setup(module);
        }
    }
}