using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// The principal square root √2 ≈ 1.41421…, with continued fraction
/// expansion <c>[1; 2, 2, 2, …]</c> — integer part 1, every subsequent
/// partial quotient 2.
/// </summary>
public sealed class Sqrt2 : CFCoefficientGenerator
{
    /// <inheritdoc/>
    public override BigInteger this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            return index == 0 ? BigInteger.One : new BigInteger(2);
        }
    }

    /// <inheritdoc/>
    public override string ToString() => "√2";
}
