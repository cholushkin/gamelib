using UnityEngine;

namespace GameLib
{
    public static class Vector3Ex
    {
        public static float DistanceTo(this Vector3 vecA, Vector3 vecB)
        {
            return (vecA - vecB).magnitude;
        }

        public static Vector3 ToVector3(this Vector2 v2, float z)
        {
            return new Vector3(v2.x, v2.y, z);
        }
   }
}