// todo: add an optional confirmation dialog step before executing destructive button actions.
// idea: add a visual cooldown or disable state to the button to prevent rapid double-clicks.

using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{

    public class DebugWidgetButton : DebugWidgetImageAndText
    {
        [Required] public Button Button;

        protected virtual void Start()
        {
            if (Button != null)
            {
                Button.onClick.AddListener(_onButtonPressInternal);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Button != null)
            {
                Button.onClick.RemoveListener(_onButtonPressInternal);
            }
        }

        private void Reset()
        {
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        private void _onButtonPressInternal()
        {
            ButtonPressHandler();
        }

        protected virtual void ButtonPressHandler()
        {
            Debug.Log("Button press handler is not overridden");
        }
    }
}