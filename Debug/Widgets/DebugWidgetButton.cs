using NaughtyAttributes;
using UnityEngine.UI;

namespace Gamelib
{
    public class DebugWidgetButton : DebugWidgetImageAndText
    {
        [Required] public Button Button;

        public virtual void Awake()
        {
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
