using UnityEngine;

public class DbgBundleIdentifier : Pane
{
    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
        SetText($"Bundle ID: {Application.identifier}");
    }
}

