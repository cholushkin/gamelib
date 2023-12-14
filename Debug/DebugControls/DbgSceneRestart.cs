using UnityEngine.SceneManagement;

namespace GameLib.Dbg
{
    public class DbgSceneRestart : Pane
    {
        public string SceneName;

        public override void InitializeState()
        {
            base.InitializeState();
            SetText($"<color=red>Restart '{SceneName}' scene</color>");
        }

        public override void OnClick()
        {
            base.OnClick();
            SceneManager.LoadScene(SceneName);
        }
    }
}