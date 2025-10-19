using GameLib.GUI;
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

        public void Update()
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
            SetText(gameStatesStr, Color.white);
        }
    }
}