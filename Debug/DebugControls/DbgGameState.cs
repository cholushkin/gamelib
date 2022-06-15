public class DbgGameState : Pane
{
    public AppStateManager AppStateManager;


    public override void InitializeState()
    {
        base.InitializeState();
        DisableButton();
    }

    public override void Update()
    {
        var gameStatesStr = $"<b>[GameState]</b>\ncur:{AppStateManager.GetCurrentState()?.GetName()}\nprev:{AppStateManager.GetPreviousState()?.GetName()}";
        SetText(gameStatesStr);
    }
}
