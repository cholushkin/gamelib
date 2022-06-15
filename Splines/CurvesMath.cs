using UnityEngine;


public static class HermiteUtils
{

    public static Vector3 Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, double amount)
    {
        return new Vector3(
            (float)Hermite(value1.x, tangent1.x, value2.x, tangent2.x, amount),
            (float)Hermite(value1.y, tangent1.y, value2.y, tangent2.y, amount),
            (float)Hermite(value1.z, tangent1.z, value2.z, tangent2.z, amount)
        );
    }

    public static double Hermite(double value1, double tangent1, double value2, double tangent2, double amount)
    {
        // All transformed to double not to lose precision
        // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
        double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
        double sCubed = s * s * s;
        double sSquared = s * s;

        if (amount == 0f)
            result = value1;
        else if (amount == 1f)
            result = value2;
        else
            result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                     (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                     t1 * s +
                     v1;
        return (double)result;
    }
}

public static class Bezier
{

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * p0 +
            2f * oneMinusT * t * p1 +
            t * t * p2;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return
            2f * (1f - t) * (p1 - p0) +
            2f * t * (p2 - p1);
    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float OneMinusT = 1f - t;
        return
            OneMinusT * OneMinusT * OneMinusT * p0 +
            3f * OneMinusT * OneMinusT * t * p1 +
            3f * OneMinusT * t * t * p2 +
            t * t * t * p3;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }

    public static float GetLengthFast(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var chord = Vector3.Distance(p3, p0);
        var cont_net = Vector3.Distance(p0, p1) + Vector3.Distance(p2, p1) + Vector3.Distance(p3, p2);
        return (cont_net + chord) / 2;
    }
}

