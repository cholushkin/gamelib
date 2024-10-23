using GameLib;
using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetGameState : DebugWidgetImageAndText
    {
        public void Update()
        {
            if(!AppStateManager.Instance)
                return;
            var gameStatesStr =
                $"<b>[GameState]</b>\ncur:{AppStateManager.Instance.GetCurrentState()?.GetName()}\nprev:{AppStateManager.Instance.GetPreviousState()?.GetName()}";
            SetText(gameStatesStr, Color.white);
        }
    }
}