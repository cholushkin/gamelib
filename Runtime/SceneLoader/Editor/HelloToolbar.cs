using UnityEditor.Toolbars;

public static class HelloToolbar
{
    [MainToolbarElement(
        "GameLib/Hello",
        defaultDockPosition = MainToolbarDockPosition.Middle)]
    public static MainToolbarElement Create()
    {
        return new MainToolbarLabel(
            new MainToolbarContent("Hello"));
    }
}