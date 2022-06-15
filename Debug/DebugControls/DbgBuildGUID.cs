using UnityEngine;

public class DbgBuildGUID : Pane
{
    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
        SetText($"Build GUID: {Application.buildGUID}");
    }
}
