using System;
using UnityEngine;

namespace GameLib
{
    public static class Vector2IntEx
    {
        public static readonly Vector2Int Min = new Vector2Int(int.MinValue, int.MinValue);
        public static readonly Vector2Int Max = new Vector2Int(int.MaxValue, int.MaxValue);

        public static Vector2Int Abs(this Vector2Int vecA)
        {
            return new Vector2Int(Math.Abs(vecA.x), Math.Abs(vecA.y));
        }

        public static float DistanceSq(this Vector2Int vecA, Vector2Int vecB)
        {
            var deltaX = vecA.x - vecB.y;
            var deltaY = vecA.x - vecB.y;
            return deltaX * deltaX + deltaY * deltaY;
        }
    }
}