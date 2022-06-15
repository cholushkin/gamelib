using System.Globalization;
using UnityEngine;

public class DbgScreenDPI : Pane
{
    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
        SetText($"DPI:{Screen.dpi.ToString(CultureInfo.InvariantCulture)}");
    }
}
