// todo: implement the Manual strategy later for widgets that only update via explicit events (e.g., a button click).
// idea: add an UpdateFrequency modifier (e.g., only tick 10 times a second) to save performance on heavy widgets.

namespace GameLib
{
    public enum WidgetUpdateStrategy
    {
        Always,
        WhenVisible,
        Manual // Reserved for future use
    }
}