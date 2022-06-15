using UnityEngine;

public class DbgCachePath : Pane
{
    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
        SetText($"Cache: {Application.temporaryCachePath}");
    }
}
