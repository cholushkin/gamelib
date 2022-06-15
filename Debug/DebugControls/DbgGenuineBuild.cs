using UnityEngine;

public class DbgGenuineBuild : Pane
{
    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
        SetText($"Genuine: {Application.genuineCheckAvailable}:{Application.genuine.ToString()}");
    }
}

