// todo: consider injecting SimpleGUI directly via VContainer instead of using a VitalRouter event if it becomes a core service.
// idea: add a button to clear the SimpleGUI stack or copy it to the clipboard.
using UnityEngine;
using VitalRouter;

namespace GameLib
{

    public class DebugWidgetSimpleGUI : DebugWidgetImageAndText
    {
        public class EventRetrieveSimpleGUIInstance : ICommand
        {
            public SimpleGUI Instance;
        }

        public SimpleGUI SimpleGUI;
        private readonly EventRetrieveSimpleGUIInstance _retrieveEvent = new();

        private void Reset()
        {
            SetText("<b>[SimpleGUI]</b>\n--", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.WhenVisible;
        }

        public override void Tick(float deltaTime)
        {
            if (!SimpleGUI)
            {
                Router.Default.PublishAsync(_retrieveEvent);
                SimpleGUI = _retrieveEvent.Instance;
            }
            else
            {
                ApplyState();
            }
        }

        private void ApplyState()
        {
            var gameStatesStr = $"<b>[SimpleGUI]</b>\n{SimpleGUI.DbgGetStackString()}";
            SetText(gameStatesStr, GetTextColor());
        }
    }
}