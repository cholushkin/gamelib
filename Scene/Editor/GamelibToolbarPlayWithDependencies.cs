using System.Linq;
using UnityEngine;
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using UnityEditor;
using UnityEditor.SceneManagement;

[EditorToolbarElement(id, typeof(SceneView))]
class EditorPlayWithDependencies : EditorToolbarButton
{
    public const string id = "Gamelib/Scene/EditoPlayWithDependencies"; // This ID is used to populate toolbar elements.

    public EditorPlayWithDependencies()
    {
        icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Libs/GameLib/Scene/Editor/Textures/PlayButton.png");
        tooltip = "Play current active scene loading its dependencies";
        clicked += OnClick;
    }

    void OnClick()
    {
        EditorSceneManager.playModeStartScene = null;
        SessionState.EraseString("SceneWithDeps");

        var activeScene = EditorSceneManager.GetActiveScene();
        Debug.Log($"Running '{activeScene.name}' with dependencies");

        var conf = ScriptableObjectUtility.GetInstanceOfSingletonScriptableObject<SceneDevDependenciesConfig>();
        if (conf == null)
        {
            Debug.LogWarning($"SceneDevDependenciesConfig is not found. Please create one using Right click in the project tree -> Create -> GameLib -> Scene -> SceneDevDependenciesConfig");
            EditorApplication.isPlaying = true;
            return;
        }

        if (conf.StartScene.name == activeScene.name)
        {
            Debug.Log("Start scene contains it's dependencies in SceneLoader object");
            EditorApplication.isPlaying = true;
            return;
        }

        SessionState.SetString("SceneWithDeps", activeScene.name);
        EditorSceneManager.playModeStartScene = conf.StartScene;
        EditorApplication.isPlaying = true;
    }
}

[Overlay(typeof(SceneView), "Gamelib toolbar")]
[Icon("Assets/unity.png")] // todo: gamelib icon
public class EditorGamelibToolbar : ToolbarOverlay
{
    EditorGamelibToolbar() : base(EditorPlayWithDependencies.id)
    { }
}


public static class SceneLoadingHook
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void OnBeforeSplashScreen()
    {
        if (SessionState.GetString("SceneWithDeps", null) == null)
            EditorSceneManager.playModeStartScene = null;
    }
}
