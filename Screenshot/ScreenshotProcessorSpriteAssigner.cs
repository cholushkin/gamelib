using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ScreenshotProcessorSpriteAssigner : ScreenshotController.ScreenshotProcessor
{
    public SpriteRenderer TargetSprite;

    public override void Process(Texture2D texture)
    {
        Assert.IsNotNull(texture);
        Assert.IsNotNull(TargetSprite);
        TargetSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }
}
