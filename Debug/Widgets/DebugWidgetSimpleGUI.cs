using GameGUI;
using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetSimpleGUI : DebugWidgetImageAndText
    {
        public class EventRetrieveSimpleGUIInstance
        {
            public SimpleGUI Instance;
        }
        
        public SimpleGUI SimpleGUI;
        private readonly EventRetrieveSimpleGUIInstance _retrieveEvent = new();

        public void Update()
        {
            if (!SimpleGUI)
            {
                GlobalEventAggregator.EventAggregator.Publish(_retrieveEvent);
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