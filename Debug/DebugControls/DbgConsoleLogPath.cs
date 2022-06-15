using UnityEngine;

public class DbgConsoleLogPath : Pane
{
    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
        SetText($"Log: {Application.consoleLogPath}");
    }
}
