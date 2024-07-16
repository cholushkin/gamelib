using System;
using Events;
using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgScreenAspectRatio : Pane, IHandle<AspectRatioHelper.EventScreenOrientationChanged>
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            GlobalEventAggregator.EventAggregator.Subscribe(this);
            var aspectRatio = AspectRatioHelper.GetAspectRatio();
            SetText(
                $"Screen aspect ratio: {AspectRatioHelper.GetAspectRatioString(aspectRatio, true)}, {Screen.orientation}");
        }

        public void Handle(AspectRatioHelper.EventScreenOrientationChanged message)
        {
            var aspectRatio = AspectRatioHelper.GetAspectRatio();
            SetText(
                $"Screen aspect ratio: {AspectRatioHelper.GetAspectRatioString(aspectRatio, true)}, {Screen.orientation}");
        }
    }

    // todo: move to separate helper file
    public static class AspectRatioHelper
    {
        public class EventScreenOrientationChanged // Spawned when screen orientation change detected
        {
        }

        public enum AspectRatio
        {
            AspectRatio4On3OrOther = 0,
            AspectRatio3On2 = 1,
            AspectRatio16On10 = 2,
            AspectRatio5On3 = 3,
            AspectRatio16On9 = 4,
            AspectRatio19n5On9 = 5,
        }

        public static AspectRatio GetAspectRatio()
        {
            var max = Mathf.Max(Screen.height, Screen.width);
            var min = Mathf.Min(Screen.height, Screen.width);
            float ratio = max / (float) min;

            if (ratio >= 1.87)
                return AspectRatio.AspectRatio19n5On9;
            if (ratio >= 1.74)
                return AspectRatio.AspectRatio16On9;
            if (ratio > 1.6)
                return AspectRatio.AspectRatio5On3;
            if (Math.Abs(ratio - 1.6) < Mathf.Epsilon)
                return AspectRatio.AspectRatio16On10;
            if (ratio >= 1.5)
                return AspectRatio.AspectRatio3On2;
            return AspectRatio.AspectRatio4On3OrOther;
        }

        public static float GetAspectRatioFactor()
        {
            var max = Mathf.Max(Screen.height, Screen.width);
            var min = Mathf.Min(Screen.height, Screen.width);
            return max / (float) min;
        }

        public static string GetAspectRatioString(AspectRatio aspectRatio, bool verboseString)
        {
            var ratio = GetAspectRatioFactor();
            switch (aspectRatio)
            {
                case AspectRatio.AspectRatio4On3OrOther:
                    return verboseString ? "4:3" : $"4:3 or other ({ratio:F3})";
                case AspectRatio.AspectRatio3On2:
                    return verboseString ? "3:2" : $"3:2 ({ratio:F3})";
                case AspectRatio.AspectRatio16On10:
                    return verboseString ? "16:10" : $"16:10 ({ratio:F3})";
                case AspectRatio.AspectRatio5On3:
                    return verboseString ? "5:3" : $"5:3 ({ratio:F3})";
                case AspectRatio.AspectRatio16On9:
                    return verboseString ? "16:9" : $"16:9 ({ratio:F3})";
                case AspectRatio.AspectRatio19n5On9:
                    return verboseString ? "19.5:9" : $"19.5:9 ({ratio:F3})";
            }

            throw new NotImplementedException();
        }
    }
}