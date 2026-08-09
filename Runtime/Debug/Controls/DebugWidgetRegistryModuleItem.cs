// todo: add support for displaying module tags visually (e.g., small colored pills).
// idea: allow clicking the item to open the module's documentation URL if one is added to the manifest.

using GameLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DebugWidgetRegistryModuleItem : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI NameAndVersionText;
    public TextMeshProUGUI DescriptionText;
    public Button ClickButton;

    private GameLibModuleManifest _manifest;

    public void Setup(GameLibModuleManifest manifest)
    {
        _manifest = manifest;

        if (manifest.Icon != null)
        {
            Icon.sprite = manifest.Icon;
            Icon.enabled = true;
        }
        else
        {
            Icon.enabled = false;
        }

        NameAndVersionText.text = $"{manifest.Name}<size=80%>(v{manifest.Version})</size>" ;
        
        if (DescriptionText != null)
            DescriptionText.text = manifest.Description;

        ClickButton.onClick.RemoveAllListeners();
        ClickButton.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        if (_manifest == null) return;
        
        UnityEngine.Debug.Log($"[Module Registry] Name: '{_manifest.Name}' | Version: v{_manifest.Version} | Description: '{_manifest.Description}'");
    }

    private void OnDestroy()
    {
        ClickButton.onClick.RemoveAllListeners();
    }
}