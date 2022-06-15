using UnityEngine;


public class Screenshoter : MonoBehaviour
{
    public Camera OriginalCamera;

    public int Width;
    public int Height;
    private Camera _workingCamera;

    void Awake()
    {
        _workingCamera = GetComponent<Camera>();
    }

    public Texture2D GetScreenTexture()
    {
        // copy all params from OriginalCamera except the rect
        _workingCamera.CopyFrom(OriginalCamera);
        _workingCamera.rect = new Rect(0,0,1,1);

        var resWidth = Width;
        var resHeight = Height;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 32);
        _workingCamera.targetTexture = rt;

        _workingCamera.Render();

        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        texture.Apply();
        _workingCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        return texture;
    }
}
