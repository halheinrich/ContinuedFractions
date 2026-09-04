using System.Numerics;

namespace HalHeinrich.Numerics.ContinuedFractions.Generators;

/// <summary>
/// Factory methods that produce <see cref="PatternCFCoefficientGenerator"/>
/// instances for well-known continued fractions and for arbitrary
/// rationals.
/// </summary>
/// <remarks>
/// Centralising the named CFs here keeps their pattern decompositions
/// in one place and lets call sites read like
/// <c>new ContinuedFraction(Patterns.Phi())</c> rather than spelling
/// out the lanes inline.
/// </remarks>
public static class Patterns
{
    /// <summary>
    /// The golden ratio <c>φ = (1 + √5) / 2 ≈ 1.61803…</c>, with CF
    /// expansion <c>[1; 1, 1, 1, …]</c> — every coefficient is 1.
    /// </summary>
    public static PatternCFCoefficientGenerator Phi() =>
        new(Array.Empty<BigInteger>(), [Lane.Const(1)]);

    /// <summary>
    /// The principal square root <c>√2 ≈ 1.41421…</c>, with CF
    /// expansion <c>[1; 2, 2, 2, …]</c>.
    /// </summary>
    public static PatternCFCoefficientGenerator Sqrt2() =>
        new(new BigInteger[] { 1 }, [Lane.Const(2)]);

    /// <summary>
    /// Euler's number <c>e ≈ 2.71828…</c>, with CF expansion
    /// <c>[2; 1, 2, 1, 1, 4, 1, 1, 6, 1, 1, 8, …]</c> — three lanes
    /// cycling, the middle lane an arithmetic progression
    /// <c>2, 4, 6, 8, …</c>.
    /// </summary>
    public static PatternCFCoefficientGenerator EulersNumber() =>
        new(new BigInteger[] { 2 }, [Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1)]);

    /// <summary>
    /// The simple-continued-fraction expansion of the rational
    /// <c><paramref name="numerator"/> / <paramref name="denominator"/></c>,
    /// produced via the Euclidean algorithm. The resulting pattern has
    /// an empty lane cycle (so <see cref="PatternCFCoefficientGenerator.IsRational"/>
    /// is <see langword="true"/>); the entire CF lives in the
    /// pre-period.
    /// </summary>
    /// <param name="numerator">Any integer.</param>
    /// <param name="denominator">Any non-zero integer.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="denominator"/> is zero.
    /// </exception>
    public static PatternCFCoefficientGenerator Rational(
        BigInteger numerator,
        BigInteger denominator)
    {
        if (denominator.IsZero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(denominator), denominator, "Denominator must be non-zero.");
        }

        var coeffs = new List<BigInteger>();
        var num = numerator;
        var den = denominator;

        while (!den.IsZero)
        {
            // Normalise so the divisor is positive — the Euclidean floor
            // and remainder behave cleanly there.
            if (den.Sign < 0)
            {
                num = -num;
                den = -den;
            }

            var (a, r) = BigInteger.DivRem(num, den);
            if (r.Sign < 0)
            {
                // C# DivRem rounds toward zero; convert to Euclidean
                // (toward −∞) when the dividend was negative.
                a -= BigInteger.One;
                r += den;
            }

            coeffs.Add(a);
            num = den;
            den = r;
        }

        return new PatternCFCoefficientGenerator(coeffs, Array.Empty<Lane>());
    }
}
