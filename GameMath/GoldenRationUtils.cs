
using System;

namespace Gamelib;
    
public static class GoldenRationUtils
{
    /// Small golden constant ≈ 0.146
    public const double GoldenA = 0.146;

    /// Small golden constant ≈ 0.236
    public const double GoldenB = 0.236;

    /// Small golden constant ≈ 0.382
    public const double GoldenC = 0.382;

    /// Golden inverse constant ≈ 0.618
    public const double GoldenD = 0.618;

    /// Neutral scale = 1
    public const double GoldenE = 1.0;

    /// Golden ratio φ ≈ 1.618
    public const double GoldenF = 1.618;

    /// Golden ratio φ² ≈ 2.618
    public const double GoldenG = 2.618;

    /// Golden ratio φ³ ≈ 4.236
    public const double GoldenH = 4.236;

    /// Golden ratio φ⁴ ≈ 6.854
    public const double GoldenI = 6.854;

    /// Golden ratio φ⁵ ≈ 11.090
    public const double GoldenJ = 11.090;

    /// Sequential array of golden constants  
    /// (0.146, 0.236, 0.382, 0.618, 1, 1.618, 2.618, 4.236, 6.854, 11.090)
    public static readonly double[] Sequence = new double[]
    {
        GoldenA, // 0.146
        GoldenB, // 0.236
        GoldenC, // 0.382
        GoldenD, // 0.618
        GoldenE, // 1
        GoldenF, // 1.618
        GoldenG, // 2.618
        GoldenH, // 4.236
        GoldenI, // 6.854
        GoldenJ  // 11.090
    };
    
    /// Returns the closest golden constant from the sequence
    public static double NearestGolden(double value)
    {
        double best = Sequence[0];
        double bestDiff = Math.Abs(value - best);

        foreach (double c in Sequence)
        {
            double d = Math.Abs(value - c);
            if (d < bestDiff)
            {
                bestDiff = d;
                best = c;
            }
        }
        return best;
    }
}


