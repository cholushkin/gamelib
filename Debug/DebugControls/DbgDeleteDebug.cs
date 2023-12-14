using UnityEngine.Assertions;

namespace GameLib.Dbg
{
    public class DbgDeleteDebug : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            SetText(
                "<b><color=red>Delete debug tools.</color></b>\nNote that you need to reload the game again to be able to use debug tools again");
        }

        public override void OnClick()
        {
            base.OnClick();
            var debugGUI = GetComponentInParent<DebugGUI>();
            Assert.IsNotNull(debugGUI);
            Destroy(debugGUI.gameObject);
        }
    }
}