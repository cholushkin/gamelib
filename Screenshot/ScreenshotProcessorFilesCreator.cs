using System;
using GameLib.Log;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;


// todo: sandbox test
// todo: test on android

public class ScreenshotProcessorFilesCreator : ScreenshotController.ScreenshotProcessor
{
    public enum EncodeTo
    {
        Png,
        Jpg,
        Exr,
    }
    public string BaseFileName;
    public EncodeTo Format;
    public bool IsAddDate;
    public bool IsAddTime;
    public bool IsAddResolution;
    public bool IsAddNumber;

    public LogChecker Log;
    private int _lastNumber;

    public override void Process(Texture2D texture)
    {
        Assert.IsNotNull(texture);
        string filename = CookFilename(texture);

        while (System.IO.File.Exists(filename) && IsAddNumber)
        {
            if (Log.Normal())
                Debug.Log(string.Format("ScreenshotProcessorFilesCreator: File {0} already exists, incrementing number", filename));
            ++_lastNumber;
            filename = CookFilename(texture);
        }

        byte[] bytes = EncodeTextureToFormat(texture, Format);
        System.IO.File.WriteAllBytes(filename, bytes);
        if (Log.Normal())
            Debug.Log(string.Format("ScreenshotProcessorFilesCreator: Screenshot saved to: {0}", filename));

#if UNITY_EDITOR
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
#endif
    }

    byte[] EncodeTextureToFormat(Texture2D texture, EncodeTo format)
    {
        if (format == EncodeTo.Png)
            return texture.EncodeToPNG();
        if (format == EncodeTo.Jpg)
            return texture.EncodeToJPG();
        if (format == EncodeTo.Exr)
            return texture.EncodeToEXR();
        return null;
    }

    private string CookFilename(Texture2D texture)
    {
        var date = "";
        var time = "";
        if (IsAddDate || IsAddTime)
        {
            DateTime now = DateTime.Now;
            if (IsAddDate)
                date = "_" + now.ToString("dd.MM.yyyy");
            if (IsAddTime)
                time = "_" + now.ToString("HH.mm");
        }
        return string.Format("{0}{1}{2}{3}{4}.{5}",
            BaseFileName,
            date,
            time,
            IsAddResolution ? "_" + texture.width + "x" + texture.height : "",
            IsAddNumber ? "_" + _lastNumber.ToString("D4") : "",
            Format.ToString().ToLower());
    }
}
