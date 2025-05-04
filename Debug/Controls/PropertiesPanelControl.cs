using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    public class PropertiesPanelControl : MonoBehaviour
    {
        public RectTransform PropContainer;
        public PropItem PropItemPrefab;

        private readonly Dictionary<string, PropItem> _propertyItems = new();

        public void UpdateOrAddProperty(string propName, string value)
        {
            if (!_propertyItems.TryGetValue(propName, out var item))
            {
                item = Instantiate(PropItemPrefab, PropContainer);
                item.gameObject.SetActive(true);
                item.name = propName;
                _propertyItems[propName] = item;
            }

            item.SetProperty(propName, value);
        }

        public void ClearAllProperties()
        {
            foreach (var item in _propertyItems.Values)
                Destroy(item.gameObject);

            _propertyItems.Clear();
        }
    }
}