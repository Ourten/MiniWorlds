using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCode.Scripts.Utils
{
    public static class ColorUtils
    {
        public static Color32 GetAverageRGB(List<Color32> colors)
        {
            var sumR = 0d;
            var sumG = 0d;
            var sumB = 0d;

            colors.ForEach(color =>
            {
                sumR += color.r * color.r;
                sumG += color.g * color.g;
                sumB += color.b * color.b;
            });

            sumR /= colors.Count;
            sumG /= colors.Count;
            sumB /= colors.Count;

            sumR = Math.Floor(Math.Sqrt(sumR));
            sumG = Math.Floor(Math.Sqrt(sumG));
            sumB = Math.Floor(Math.Sqrt(sumB));

            return new Color32((byte) sumR, (byte) sumG, (byte) sumB, 255);
        }
    }
}