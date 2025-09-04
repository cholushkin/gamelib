using UnityEngine;

namespace GameGUI
{
    public class GUIScreenAnimated : GUIScreenBase
    {
        public Animator Animator;
        private readonly string AnimationKey = "IsInScreen";

        public override void StartAppearAnimation(AnimationType anim)
        {
            if (anim == AnimationType.Fast)
            {
                base.StartAppearAnimation(anim);
                return;
            }

            IsInTransaction = true;
            IsInputEnabled = false;
            if (Animator)
                Animator.SetBool(AnimationKey, true);
            TransitionProcessor.Appear(null);
        }

        public override void StartDisappearAnimation(AnimationType anim)
        {
            if (anim == AnimationType.Fast)
            {
                base.StartDisappearAnimation(anim);
                return;
            }
            IsInTransaction = true;
            IsInputEnabled = false;
            if (Animator)
                Animator.SetBool(AnimationKey, false);
        }


        // note: called from the animation event
        public void OnAnimationAppeared()
        {
            OnAppear();
        }

        // note: called from the animation event
        public void OnAnimationDisappeared()
        {
            OnDisappear();
        }
    }
}