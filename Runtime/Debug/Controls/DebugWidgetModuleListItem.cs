// todo: consider adding a lightweight DOTween punch effect if we want smoother visual feedback later.
// idea: add a highlight graphic/outline component that enables when the item is selected.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    public class DebugWidgetModuleListItem : MonoBehaviour
    {
        public Image Icon;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI VersionText;
        public Button ClickButton;

        private Action<DebugWidgetModuleListItem, GameLibModuleManifest> _onClick;
        private GameLibModuleManifest _manifest;

        public void Setup(GameLibModuleManifest manifest, Action<DebugWidgetModuleListItem, GameLibModuleManifest> onClick)
        {
            _manifest = manifest;
            _onClick = onClick;

            if (manifest.Icon != null)
            {
                Icon.sprite = manifest.Icon;
                Icon.enabled = true;
            }
            else
            {
                Icon.enabled = false;
            }

            NameText.text = manifest.Name;
            VersionText.text = manifest.Version;

            ClickButton.onClick.RemoveAllListeners();
            ClickButton.onClick.AddListener(HandleClick);
        }

        public void SetSelected(bool isSelected)
        {
            // Scale up slightly when selected, revert to normal when unselected
            transform.localScale = isSelected ? new Vector3(1.05f, 1.05f, 1.05f) : Vector3.one;
        }

        private void HandleClick()
        {
            _onClick?.Invoke(this, _manifest);
        }
    }
}