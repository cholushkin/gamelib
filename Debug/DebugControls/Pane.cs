using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GameLib.Dbg
{
    public class Pane : DebugLayoutElement
    {
        public string Text;
        public Color TextColor;

        protected Image _image;
        protected Text _text;
        private Button _button;
        protected int _stateIndex;
        protected int _statesCount;

        public virtual void Reset()
        {
            Text = "";
            TextColor = Color.black;
        }

        public override void InitializeState()
        {
            base.InitializeState();
            _image = GetComponentInChildren<Image>();
            _text = GetComponentInChildren<Text>();
            _button = GetComponentInChildren<Button>();
            Assert.IsNotNull(_image);
            Assert.IsNotNull(_text);
            Assert.IsNotNull(_button);

            _button.onClick.AddListener(OnClick);

            SetText(Text);
            SetTextColor(TextColor);
            SetStatesAmount(2);
        }

        public override string GetPrefabBasedOnName()
        {
            return "Pane";
        }

        public void ChangeState(int index)
        {
            OnStateChanged(_stateIndex);
        }

        public void SetStatesAmount(int statesAmount)
        {
            _statesCount = statesAmount;
        }

        public virtual void OnStateChanged(int stateIndex)
        {

        }

        public override void OnClick()
        {
            base.OnClick();
            _stateIndex = (_stateIndex + 1) % _statesCount;
            ChangeState(_stateIndex);
        }

        public void SetText(string text)
        {
            _text.text = text;
        }

        public void SetTextColor(Color textColor)
        {
            _text.color = textColor;
        }

        public void DisableButton()
        {
            _button.interactable = false;
        }

        public void DisableTextWrap()
        {
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

    }
}