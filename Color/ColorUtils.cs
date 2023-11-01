using System.Collections.Generic;
using UnityEngine;

// note: inspired by https://github.com/bgrins/TinyColor ( https://bgrins.github.io/TinyColor/ live demo )
// https://stackblitz.com/edit/tiny-color-testing?file=src%2Fapp%2Fapp.component.ts
// port of it to typescript: 

namespace GameLib
{
    public static partial class ColorUtils
    {
        public static string ToHexString(this Color color, bool includingAlpha = false)
        {
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            int a = Mathf.RoundToInt(color.a * 255f);

            return includingAlpha ? $"{r:X2}{g:X2}{b:X2}{a:X2}" : $"{r:X2}{g:X2}{b:X2}";
        }

        // todo: from hex string

        public static (float h, float s, float v, float a) ToHSV(this Color rgb)
        {
            float max = Mathf.Max(rgb.r, Mathf.Max(rgb.g, rgb.b));
            float min = Mathf.Min(rgb.r, Mathf.Min(rgb.g, rgb.b));
            float h = 0;
            float v = max;
            float d = max - min;
            float s = max == 0 ? 0 : d / max;

            if (Mathf.Approximately(max, min))
            {
                h = 0; // achromatic
            }
            else
            {
                if (Mathf.Approximately(max, rgb.r))
                {
                    h = (rgb.g - rgb.b) / d + (rgb.g < rgb.b ? 6 : 0);
                }
                else if (Mathf.Approximately(max, rgb.g))
                {
                    h = (rgb.b - rgb.r) / d + 2;
                }
                else if (Mathf.Approximately(max, rgb.b))
                {
                    h = (rgb.r - rgb.g) / d + 4;
                }

                h /= 6;
            }

            return (h, s, v, rgb.a);
        }

        public static (float h, float s, float l, float a) ToHSL(this Color rgb)
        {
            var r = Bound01(Mathf.RoundToInt(rgb.r * 255f), 255);
            var g = Bound01(Mathf.RoundToInt(rgb.g * 255f), 255);
            var b = Bound01(Mathf.RoundToInt(rgb.b * 255f), 255);


            float max = Mathf.Max(rgb.r, Mathf.Max(rgb.g, rgb.b));
            float min = Mathf.Min(rgb.r, Mathf.Min(rgb.g, rgb.b));
            float h = 0;
            float s = 0;
            float l = (max + min) / 2;

            if (Mathf.Approximately(max, min))
            {
                h = 0; // achromatic
            }
            else
            {
                float d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                if (Mathf.Approximately(max, rgb.r))
                {
                    h = (rgb.g - rgb.b) / d + (rgb.g < rgb.b ? 6 : 0);
                }
                else if (Mathf.Approximately(max, rgb.g))
                {
                    h = (rgb.b - rgb.r) / d + 2;
                }
                else if (Mathf.Approximately(max, rgb.b))
                {
                    h = (rgb.r - rgb.g) / d + 4;
                }

                h /= 6;
            }

            return (h, s, l, rgb.a);
        }

        public static Color HSLToRGB((float h, float s, float l, float a) HSL)
        {
            float r = 0;
            float g = 0;
            float b = 0;

            float hh = Bound01(HSL.h, 360);
            float ss = Bound01(HSL.s, 100);
            float ll = Bound01(HSL.l, 100);

            if (ss == 0)
            {
                // achromatic
                g = ll;
                b = ll;
                r = ll;
            }
            else
            {
                float q = ll < 0.5f ? ll * (1f + ss) : ll + ss - ll * ss;
                float p = 2f * ll - q;
                r = HueToRGB(p, q, hh + 1f / 3f);
                g = HueToRGB(p, q, hh);
                b = HueToRGB(p, q, hh - 1f / 3f);
            }

            return new Color(r, g, b, HSL.a);
        }

        // Returns the perceived brightness of the color, from 0-255 range.
        public static float GetBrightness(this Color rgb)
        {
            return (rgb.r * 255 * 299 + rgb.g * 255 * 587 + rgb.b * 255 * 114) / 1000;
        }

        // Returns the perceived brightness of the color, from 0-1 range.
        public static float GetBrightnessNorm(this Color rgb)
        {
            return (rgb.r * 299 + rgb.g * 587 + rgb.b * 114) / 1000;
        }

        public static bool IsDark(this Color rgb)
        {
            return rgb.GetBrightness() < 128;
        }

        public static bool IsLight(this Color rgb)
        {
            return !rgb.IsDark();
        }

        // Returns the perceived luminance of a color, from 0-1 range.
        public static float GetLuminance(this Color rgb)
        {
            float R, G, B;

            if (rgb.r <= 0.03928)
            {
                R = rgb.r / 12.92f;
            }
            else
            {
                R = Mathf.Pow((rgb.r + 0.055f) / 1.055f, 2.4f);
            }

            if (rgb.g <= 0.03928)
            {
                G = rgb.g / 12.92f;
            }
            else
            {
                G = Mathf.Pow((rgb.g + 0.055f) / 1.055f, 2.4f);
            }

            if (rgb.b <= 0.03928)
            {
                B = rgb.b / 12.92f;
            }
            else
            {
                B = Mathf.Pow((rgb.b + 0.055f) / 1.055f, 2.4f);
            }

            return 0.2126f * R + 0.7152f * G + 0.0722f * B;
        }

        // Returns whether the color is monochrome.
        public static bool isMonochrome(this Color rgb)
        {
            return Mathf.Approximately(rgb.ToHSL().s, 0f);
        }

        // Brighten the color a given amount
        // amount - valid between 0-1
        public static Color Brighten(this Color rgb, float amount = 0.1f)
        {
            var newClr = new Color(rgb.r, rgb.g, rgb.b, rgb.a);

            rgb.r = Mathf.Clamp01(rgb.r + amount);
            rgb.g = Mathf.Clamp01(rgb.g + amount);
            rgb.b = Mathf.Clamp01(rgb.b + amount);
            return rgb;
        }

        // Darken the color a given amount
        // Providing 1 will always return black.
        // amount - valid between 0-1
        public static Color Darken(this Color rgb, float amount = 0.1f)
        {
            var hsl = rgb.ToHSL();
            hsl.l -= amount;
            hsl.l = Mathf.Clamp01(hsl.l);
            return HSLToRGB(hsl);
        }

        // Mix the color with pure white. 
        // Providing 0 will do nothing, providing 1 will always return white.
        // amount - valid between 0-1
        public static Color Tint(this Color rgb, float amount = 0.1f)
        {
            return Color.Lerp(Color.white, rgb, Mathf.Clamp01(amount));
        }

        // Mix the color with pure black
        // Providing 0 will do nothing, providing 1 will always return black.
        // amount - valid between 0-1
        public static Color Shade(this Color rgb, float amount = 0.1f)
        {
            return Color.Lerp(Color.black, rgb, Mathf.Clamp01(amount));
        }

        // Desaturate the color a given amount
        // Providing 1 is the same as calling greyscale
        // amount - valid between 0-1
        public static Color Desaturate(this Color rgb, float amount = 0.1f)
        {
            var hsl = rgb.ToHSL();
            hsl.s -= amount;
            hsl.s = Mathf.Clamp01(hsl.s);
            return HSLToRGB(hsl);
        }

        // Saturate the color a given amount
        // param amount - valid between 0-1
        public static Color Saturate(this Color rgb, float amount = 0.1f)
        {
            var hsl = rgb.ToHSL();
            hsl.s += amount;
            hsl.s = Mathf.Clamp01(hsl.s);
            return HSLToRGB(hsl);
        }

        // Completely desaturates a color into greyscale.
        // Same as calling Desaturate(1f)
        public static Color Greyscale(this Color rgb)
        {
            return rgb.Desaturate(1f);
        }

        // Spin takes a positive or negative amount within [-360, 360] indicating the change of hue.
        // Values outside of this range will be wrapped into this range.
        public static Color ColorSpin(this Color rgb, float amount = 0.1f) 
        {
            var hsl = rgb.ToHSL();
            var hue = (hsl.h + amount) % 360f;
            hsl.h = hue < 0 ? 360 + hue : hue;
            return HSLToRGB(hsl);
        }

        // This method calculates a series of colors that are similar to the original color
        // by varying the hue while keeping the saturation and lightness constant.
        // These analogous colors can be used for various purposes in color schemes and designs.
        //public static List<Color> Analogous(this Color rgb, int results = 6, int slices = 30) 
        //{
        //    var hsl =rgb.ToHSL();
        //    float part = 360.0f / slices;
        //    List<Color> ret = new List<Color>(results);

        //    ret.Add(rgb);
        //    //for (hsl.h = (hsl.h - ((part * results) >> 1) + 720) % 360; --results;)
        //    //{
        //    //    hsl.h = (hsl.h + part) % 360;
        //    //    ret.push(new TinyColor(hsl));
        //    //}


        //    hsl.h = (hsl.h - ((int)((part * results) / 2)) + 720) % 360;
        //    for (int i = 0; i < results-1; ++i)
        //    {
        //        hsl.h = (hsl.h + part) % 360;
        //        ret.Add(HSLToRGB(hsl));
        //    }

        //    return ret;
        //}

        public static List<Color> Analogous(this Color rgb, int results = 6, int slices = 30)
        {
            var hsl = ColorUtils.RGBtoHSL(rgb);
            float part = 360.0f / slices;
            List<Color> ret = new List<Color>(results);

            ret.Add(rgb);
            //for (hsl.h = (hsl.h - ((part * results) >> 1) + 720) % 360; --results;)
            //{
            //    hsl.h = (hsl.h + part) % 360;
            //    ret.push(new TinyColor(hsl));
            //}


            hsl.h = (hsl.h - ((int)((part * results) / 2)) + 720) % 360;
            for (int i = 0; i < results - 1; ++i)
            {
                hsl.h = (hsl.h + part) % 360;
                ret.Add(ColorUtils.HSLtoRGB(hsl));
            }

            return ret;
        }



        // Calculate and return the complementary color of the current color.
        // In color theory, the complementary color is the color that is directly opposite
        // to the original color on the color wheel. When you mix a color with its complement, you get shades of gray.
        public static Color Complement(this Color rgb)
        {
            var hsl = rgb.ToHSL();
            hsl.h = (hsl.h + 180) % 360;
            return HSLToRGB(hsl);
        }


        private static float Bound01(float n, int max)
        {
            float numericN = max == 360 ? n : Mathf.Min(max, Mathf.Max(0, n));

            // Handle floating point rounding errors
            if (Mathf.Abs(numericN - max) < 0.000001)
            {
                return 1.0f;
            }

            // Convert into [0, 1] range if it isn't already
            if (max == 360)
            {
                // If n is a hue given in degrees,
                // wrap around out-of-range values into [0, 360] range
                // then convert into [0, 1].
                numericN = (numericN < 0 ? (numericN % max) + max : numericN % max) / max;
            }
            else
            {
                // If n not a hue given in degrees
                // Convert into [0, 1] range if it isn't already.
                numericN = (numericN % max) / max;
            }

            return numericN;
        }

        private static float HueToRGB(float p, float q, float t)
        {
            if (t < 0)
            {
                t += 1;
            }

            if (t > 1)
            {
                t -= 1f;
            }

            if (t < 1f / 6f)
            {
                return p + (q - p) * (6 * t);
            }

            if (t < 1f / 2f)
            {
                return q;
            }

            if (t < 2f / 3f)
            {
                return p + (q - p) * (2f / 3f - t) * 6;
            }

            return p;
        }
    }
}