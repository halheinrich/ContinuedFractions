using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// A <see cref="CFCoefficientGenerator"/> whose coefficients are produced
/// by an externally supplied function of the index.
/// </summary>
/// <remarks>
/// Use this for ad-hoc generators where defining a dedicated subclass is
/// overkill — quick experiments, test fixtures, or one-off explorations.
/// For canonical generators (φ, √2, e, …) prefer a dedicated sealed
/// subclass that overrides <see cref="ToString"/> with a meaningful name.
/// </remarks>
public sealed class FuncCFCoefficientGenerator : CFCoefficientGenerator
{
    private readonly Func<int, BigInteger> _generator;
    private readonly string _description;

    /// <summary>
    /// Constructs a generator that produces coefficients via
    /// <paramref name="generator"/>, identified by
    /// <paramref name="description"/>.
    /// </summary>
    /// <param name="description">
    /// Human-readable identification, returned from <see cref="ToString"/>.
    /// </param>
    /// <param name="generator">
    /// Function from non-negative index to coefficient. Called once per
    /// access via <see cref="this[int]"/> or iteration; results are not
    /// cached at this layer.
    /// </param>
    /// <param name="length">
    /// Optional length. When non-null, iteration completes after
    /// <paramref name="length"/> values and the indexer rejects indices
    /// at or beyond it. <see langword="null"/> (the default) marks the
    /// generator as unbounded.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="description"/> or <paramref name="generator"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="length"/> is negative.
    /// </exception>
    public FuncCFCoefficientGenerator(
        string description,
        Func<int, BigInteger> generator,
        int? length = null)
    {
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(generator);
        if (length is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                "Length must be non-negative.");
        }

        _description = description;
        _generator = generator;
        Length = length;
    }

    /// <inheritdoc/>
    public override int? Length { get; }

    /// <inheritdoc/>
    public override BigInteger this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            if (Length is int n && index >= n)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    $"Index must be less than Length ({n}).");
            }

            return _generator(index);
        }
    }

    /// <inheritdoc/>
    public override string ToString() => _description;
}
