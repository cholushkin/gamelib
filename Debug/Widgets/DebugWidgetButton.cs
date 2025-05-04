using NaughtyAttributes;
using UnityEngine.UI;

namespace GameLib
{
    public class DebugWidgetButton : DebugWidgetImageAndText
    {
        [Required] public Button Button;

        protected override void Awake()
        {
            base.Awake();
            Button.onClick.AddListener(_onButtonPressInternal);
        }

        private void _onButtonPressInternal()
        {
            ButtonPressHandler();
        }

        protected virtual void ButtonPressHandler()
        {
            print("Button press handler is not overriden");
        }
    }
}
