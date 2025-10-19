using UnityEngine;

namespace GameLib.GUI
{
    public class GUIScreenBase : MonoBehaviour
    {
        public enum AnimationType
        {
            None,
            Fast,
            Regular
        }
        public ScreenTransitionProcessor TransitionProcessor;
        public bool IsModal;
        public bool IsDestroyOnPop;

        public Transform ModalBg;

        public SimpleGUI SimpleGui { get; set; }
        public bool IsInTransaction { get; protected set; }

        private int _inputEnabledRefs;


        public bool IsInputEnabled
        {
            get { return _inputEnabledRefs > 0; }
            set
            {
                _inputEnabledRefs += value ? 1 : -1;
                TransitionProcessor.SetBlockInput(_inputEnabledRefs <= 0);
            }
        }

        public int GetRefsCount()
        {
            return _inputEnabledRefs;
        }

        public virtual void Awake()
        {
            SetupBackground(IsModal);
            SimpleGui = GetComponentInParent<SimpleGUI>();
            TransitionProcessor.SetClearState();
        }

        public virtual void OnPushed()
        {
            _inputEnabledRefs = 1;
        }

        public virtual void OnPopped()
        {
        }

        public virtual void StartAppearAnimation(AnimationType anim = AnimationType.Fast)
        {
            IsInTransaction = true;
            IsInputEnabled = false;
            TransitionProcessor.Appear(OnAppear);
        }

        public virtual void StartDisappearAnimation(AnimationType anim = AnimationType.Fast)
        {
            IsInTransaction = true;
            IsInputEnabled = false;
            TransitionProcessor.Disappear(OnDisappear);
        }

        public virtual void DisappearForced()
        {
            IsInTransaction = false;
            IsInputEnabled = false;
            SimpleGui.OnScreenPopped(this);
        }

        public virtual void OnRestore()
        {
        }

        public virtual void OnAppear()
        {
            IsInTransaction = false;
            IsInputEnabled = true;
        }

        public virtual void OnDisappear()
        {
            IsInTransaction = false;
            SimpleGui.OnScreenPopped(this);
        }

        protected void SetupBackground(bool isbgEnabled)
        {
            if (ModalBg)
                ModalBg.gameObject.SetActive(isbgEnabled);
        }

        public virtual void OnBecomeUnderModal(bool isUnder)
        {
            IsInputEnabled = !isUnder;
        }
    }
}
