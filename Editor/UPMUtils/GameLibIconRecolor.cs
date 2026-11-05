
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace GameLib.Editor
{
    // Converts a module manifest icon into a stencil-style render: white
    // pixels become transparent, black pixels become solid green, with
    // everything in between interpolated. Used so manifest icons read
    // consistently in the UPM Utils list and in the module info window.
    public static class GameLibIconRecolor
    {
        private static readonly Color TintColor = new Color(0.30f, 0.85f, 0.35f);

        private static readonly Dictionary<Sprite, Texture2D> Cache =
            new Dictionary<Sprite, Texture2D>();

        public static Texture2D GetGreenIcon(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
                return null;

            if (Cache.TryGetValue(sprite, out var cached) && cached != null)
                return cached;

            var result = Build(sprite);
            Cache[sprite] = result;
            return result;
        }

        private static Texture2D Build(Sprite sprite)
        {
            var source = sprite.texture;

            int width = Mathf.Max(1, Mathf.RoundToInt(sprite.rect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(sprite.rect.height));

            // Sample through a RenderTexture so this works even when the
            // source texture has "Read/Write Enabled" turned off.
            Vector2 scale = new Vector2(
                sprite.rect.width / source.width,
                sprite.rect.height / source.height);

            Vector2 offset = new Vector2(
                sprite.rect.x / source.width,
                sprite.rect.y / source.height);

            var rt = RenderTexture.GetTemporary(
                width, height, 0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);

            // Force nearest-neighbor sampling for the blit so the source
            // sprite is copied 1:1, pixel for pixel, with no bilinear
            // blending at the edges between opaque/black and transparent/
            // white texels.
            rt.filterMode = FilterMode.Point;

            var previousFilterMode = source.filterMode;
            var previousWrapMode = source.wrapMode;

            source.filterMode = FilterMode.Point;
            source.wrapMode = TextureWrapMode.Clamp;

            var previousActive = RenderTexture.active;

            Graphics.Blit(source, rt, scale, offset);
            RenderTexture.active = rt;

            source.filterMode = previousFilterMode;
            source.wrapMode = previousWrapMode;

            var result = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };

            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(rt);

            var pixels = result.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                var sourcePixel = pixels[i];
                float blackness = (1f - sourcePixel.grayscale) * sourcePixel.a;

                pixels[i] = new Color(
                    TintColor.r, TintColor.g, TintColor.b, blackness);
            }

            result.SetPixels(pixels);
            result.Apply();

            return result;
        }
    }
}

#endif
