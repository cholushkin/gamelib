using UnityEngine;
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


[EditorToolbarElement(id, typeof(SceneView))]
class EditorPlayWithDependencies : EditorToolbarButton
{
    public const string id = "Gamelib/Scene/EditoPlayWithDependencies"; // This ID is used to populate toolbar elements.

    public EditorPlayWithDependencies()
    {
        icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Libs/GameLib/Scene/Editor/Textures/PlayButton.png");
        tooltip = "Play current active scene with its dependencies";
        clicked += OnClick;
    }

    void OnClick()
    {
        EditorSceneManager.playModeStartScene = null;
        SessionState.EraseString("DevSceneWithDeps");

        if (Application.isPlaying)
            return;

        var activeScene = EditorSceneManager.GetActiveScene();
        Debug.Log($"Running '{activeScene.name}' with dependencies");

        SessionState.SetString("DevSceneWithDeps", activeScene.name);
        EditorPlayAsRelease.RunStartScene();
    }
}


[EditorToolbarElement(id, typeof(SceneView))]
class EditorPlayAsRelease : EditorToolbarButton
{
    public const string id = "Gamelib/Scene/EditorPlayAsRelease"; // This ID is used to populate toolbar elements.

    public EditorPlayAsRelease()
    {
        icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Libs/GameLib/Scene/Editor/Textures/PlayButtonRelease.png");
        tooltip = "Play the game as it will play in RELEASE";
        clicked += OnClick;
    }

    void OnClick()
    {
        EditorSceneManager.playModeStartScene = null;
        SessionState.EraseString("DevSceneWithDeps");

        if (Application.isPlaying)
            return;

        Debug.Log("Running release scene order");
        RunStartScene();
    }

    internal static void RunStartScene()
    {
        var startingScenePath = SceneUtility.GetScenePathByBuildIndex(0);

        // Note:  Unity 2023.1.4f1 bug ?
        // Remove scene from hierarchy if it's game starting scene and it has "not loaded" state in hierarchy,
        // otherwise Unity Player won't start - some loaded scene will stuck with 'is unloading' status in hierarchy.
        // Probably Unity trying to remove "not loaded" scene and load it in the same time and it has some conflict.
        // Other possible workaround could be: load this scene additively here and unload it again to "not loaded" state when game stops
        foreach (var sceneSetup in EditorSceneManager.GetSceneManagerSetup())
            if (startingScenePath == sceneSetup.path && !sceneSetup.isLoaded)
            {
                EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(startingScenePath), true);
                break;
            }

        SceneAsset startingSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(startingScenePath);
        EditorSceneManager.playModeStartScene = startingSceneAsset;
        EditorApplication.isPlaying = true;
    }
}





[Overlay(typeof(SceneView), "Gamelib toolbar")]
[Icon("Assets/unity.png")] // todo: gamelib icon
public class EditorGamelibToolbar : ToolbarOverlay
{
    EditorGamelibToolbar() : base(EditorPlayWithDependencies.id, EditorPlayAsRelease.id)
    { }
}


public static class SceneLoadingHook
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void OnBeforeSplashScreen()
    {
        if (string.IsNullOrEmpty(SessionState.GetString("DevSceneWithDeps", null)))
            EditorSceneManager.playModeStartScene = null;
        else
            Debug.Log($"Starting dev scene '{SessionState.GetString("DevSceneWithDeps", null)}'");
    }
}
