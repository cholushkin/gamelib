using Events;
using UnityEngine;

public class DbgScreenResolution : Pane, IHandle<AspectRatioHelper.EventScreenOrientationChanged>
{
    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
        GlobalEventAggregator.EventAggregator.Subscribe(this);
        SetText($"Resolution: {Screen.width}x{Screen.height}");
    }

    public void Handle(AspectRatioHelper.EventScreenOrientationChanged message)
    {
        SetText($"Resolution: {Screen.width}x{Screen.height}");
    }
}
