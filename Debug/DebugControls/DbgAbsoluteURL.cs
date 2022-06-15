﻿using UnityEngine;

public class DbgAbsoluteURL : Pane
{
    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
        SetText($"Absolute URL:{Application.absoluteURL}");
    }
}
