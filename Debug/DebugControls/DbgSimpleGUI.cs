using GameGUI;

namespace GameLib.Dbg
{
    public class DbgSimpleGUI : Pane
    {
        public SimpleGUI SimpleGUI;


        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
        }

        public override void Update()
        {
            var gameStatesStr = $"<b>[SimpleGUI]</b>\n{SimpleGUI.DbgGetStackString()}";
            SetText(gameStatesStr);
        }
    }
}