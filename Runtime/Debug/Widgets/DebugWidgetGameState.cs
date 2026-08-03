// todo: decouple from AppStateManager.Instance singleton using DI if possible.
// idea: add a visual history log of the last 5 states instead of just current/previous.

using UnityEngine;

namespace GameLib
{
    public class DebugWidgetGameState : DebugWidgetImageAndText
    {
        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            SetText("<b>[GameState]</b>\ncur: --\nprev: --", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.WhenVisible;
        }

        public override void Tick(float deltaTime)
        {
            ApplyState();
        }

        private void ApplyState()
        {
            if (!AppStateManager.Instance) return;
            var gameStatesStr = $"<b>[GameState]</b>\ncur:{AppStateManager.Instance.GetCurrentState()?.GetName()}\nprev:{AppStateManager.Instance.GetPreviousState()?.GetName()}";
            SetText(gameStatesStr, GetTextColor());
        }
    }
}