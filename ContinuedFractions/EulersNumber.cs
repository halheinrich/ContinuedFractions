using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// Euler's number e ≈ 2.71828…, with continued fraction expansion
/// <c>[2; 1, 2, 1, 1, 4, 1, 1, 6, 1, 1, 8, …]</c> — integer part 2,
/// followed by the periodic block <c>(1, 2k, 1)</c> for
/// <c>k = 1, 2, 3, …</c>.
/// </summary>
public sealed class EulersNumber : CFCoefficientGenerator
{
    /// <inheritdoc/>
    public override BigInteger this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            if (index == 0)
            {
                return new BigInteger(2);
            }

            // Within the (1, 2k, 1) block the "2k" sits at index 3k − 1 —
            // i.e. where (index + 1) is divisible by 3.
            if ((index + 1) % 3 == 0)
            {
                return new BigInteger(2 * ((index + 1) / 3));
            }

            return BigInteger.One;
        }
    }

    /// <inheritdoc/>
    public override string ToString() => "e";
}
