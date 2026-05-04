using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// The golden ratio φ = (1 + √5) / 2 ≈ 1.61803…, with continued
/// fraction expansion <c>[1; 1, 1, 1, …]</c> — every partial quotient
/// is 1.
/// </summary>
/// <remarks>
/// φ is the simplest irrational from a continued-fraction standpoint
/// and the slowest to converge: its convergents are the ratios of
/// successive Fibonacci numbers (1/1, 2/1, 3/2, 5/3, 8/5, 13/8, …).
/// </remarks>
public sealed class Phi : CFCoefficientGenerator
{
    /// <inheritdoc/>
    public override BigInteger this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            return BigInteger.One;
        }
    }

    /// <inheritdoc/>
    public override string ToString() => "φ";
}
