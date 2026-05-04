using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// Rounding mode for <see cref="IntegerMath.Sqrt(BigInteger, IntegerSqrtRounding)"/>.
/// </summary>
public enum IntegerSqrtRounding
{
    /// <summary>
    /// Round toward −∞: the largest non-negative integer <c>k</c> with
    /// <c>k² ≤ n</c>.
    /// </summary>
    Floor,

    /// <summary>
    /// Round toward +∞: the smallest non-negative integer <c>k</c> with
    /// <c>k² ≥ n</c>.
    /// </summary>
    Ceiling,

    /// <summary>
    /// Round to the nearest integer. The result is unambiguous for any
    /// non-negative integer input — for non-square <c>n</c> the value
    /// <c>√n</c> is irrational, so the midpoint between two consecutive
    /// integers is never reached, and there are no ties to break.
    /// </summary>
    Nearest,
}

/// <summary>
/// Integer-arithmetic helpers built on <see cref="BigInteger"/>.
/// </summary>
public static class IntegerMath
{
    /// <summary>
    /// Computes the integer square root of <paramref name="value"/> with
    /// the given <paramref name="rounding"/> mode. The result is exact
    /// (no floating-point intermediate).
    /// </summary>
    /// <param name="value">A non-negative integer.</param>
    /// <param name="rounding">
    /// How to round when <paramref name="value"/> is not a perfect
    /// square. Defaults to <see cref="IntegerSqrtRounding.Floor"/> —
    /// the most common need (e.g. as input to integer-arithmetic
    /// algorithms operating in <c>(s, s+1)</c> intervals around
    /// <c>√n</c>).
    /// </param>
    /// <returns>
    /// The non-negative integer <c>k</c> determined by
    /// <paramref name="rounding"/> applied to <c>√value</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative, or <paramref name="rounding"/>
    /// is not a defined enum value.
    /// </exception>
    public static BigInteger Sqrt(
        BigInteger value,
        IntegerSqrtRounding rounding = IntegerSqrtRounding.Floor)
    {
        if (value.Sign < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Cannot take the square root of a negative integer.");
        }

        var floor = FloorSqrt(value);

        return rounding switch
        {
            IntegerSqrtRounding.Floor => floor,
            IntegerSqrtRounding.Ceiling =>
                floor * floor == value ? floor : floor + BigInteger.One,
            IntegerSqrtRounding.Nearest =>
                // n in [floor², (floor+1)²); midpoint of those squares is
                // floor² + floor + 0.5. So n − floor² ≤ floor ⇒ closer to
                // floor; n − floor² ≥ floor + 1 ⇒ closer to floor + 1.
                value - floor * floor <= floor ? floor : floor + BigInteger.One,
            _ => throw new ArgumentOutOfRangeException(
                nameof(rounding),
                rounding,
                "Unknown rounding mode."),
        };
    }

    /// <summary>
    /// Computes <c>⌊√n⌋</c> for non-negative <paramref name="n"/> via
    /// Newton-Raphson iteration with integer arithmetic. Converges in
    /// <c>O(log n)</c> iterations.
    /// </summary>
    private static BigInteger FloorSqrt(BigInteger n)
    {
        if (n.IsZero)
        {
            return BigInteger.Zero;
        }
        if (n.IsOne)
        {
            return BigInteger.One;
        }

        // Newton iteration: x_{k+1} = ⌊(x_k + ⌊n/x_k⌋) / 2⌋. Starts above
        // √n and decreases monotonically until y >= x signals stability;
        // at that point x = ⌊√n⌋.
        var x = n;
        var y = (x + BigInteger.One) >> 1;
        while (y < x)
        {
            x = y;
            y = (x + n / x) >> 1;
        }
        return x;
    }
}
