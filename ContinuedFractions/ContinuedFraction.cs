using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace ContinuedFractions;

/// <summary>
/// A finite simple continued fraction <c>[a0; a1, a2, ...]</c> where
/// <c>a0</c> is the integer part (any sign) and <c>a1, a2, ...</c> are
/// strictly positive partial quotients.
/// </summary>
/// <remarks>
/// Instances are immutable. Two continued fractions are equal when their
/// coefficient sequences are equal element-by-element; canonicalization
/// (e.g. the [..., a, 1] vs [..., a+1] identity) is not performed here.
/// </remarks>
public sealed class ContinuedFraction : IEquatable<ContinuedFraction>, IFormattable
{
    private readonly CFCoefficientGenerator _generator;

    /// <summary>The coefficient sequence <c>[a0, a1, a2, ...]</c>.</summary>
    /// <exception cref="InvalidOperationException">
    /// The continued fraction is not backed by an explicit coefficient
    /// list. Use <see cref="Generator"/> for generator-based access.
    /// </exception>
    public IReadOnlyList<BigInteger> Coefficients =>
        _generator is ListCFCoefficientGenerator lg
            ? lg.Items
            : throw new InvalidOperationException(
                "Coefficients is only available for list-backed continued " +
                "fractions; use the Generator property for generator-based access.");

    /// <summary>
    /// The underlying coefficient generator. Iterate or index this for
    /// access to the partial quotients of any continued fraction,
    /// regardless of how it was constructed.
    /// </summary>
    public CFCoefficientGenerator Generator => _generator;

    /// <summary>
    /// Constructs a continued fraction from its coefficient sequence.
    /// </summary>
    /// <param name="coefficients">
    /// The coefficients in order: integer part first, then partial quotients.
    /// At least one coefficient is required; partial quotients (a1 onward)
    /// must be strictly positive.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="coefficients"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The sequence is empty, or a partial quotient is non-positive.
    /// </exception>
    public ContinuedFraction(IEnumerable<BigInteger> coefficients)
    {
        _generator = new ListCFCoefficientGenerator(ValidateAndFreeze(coefficients));
    }

    /// <inheritdoc cref="ContinuedFraction(IEnumerable{BigInteger})"/>
    public ContinuedFraction(params BigInteger[] coefficients)
        : this((IEnumerable<BigInteger>)coefficients)
    {
    }

    /// <summary>
    /// Constructs a continued fraction backed by a coefficient generator.
    /// </summary>
    /// <param name="generator">
    /// The source of partial quotients. Must yield at least one
    /// coefficient (i.e. its <see cref="CFCoefficientGenerator.Length"/>
    /// is either <see langword="null"/> — unbounded — or at least 1).
    /// </param>
    /// <remarks>
    /// CF invariants on partial-quotient values
    /// (<c>a_i</c> strictly positive for <c>i ≥ 1</c>) are not validated
    /// at construction; they are checked lazily when the generator is
    /// consumed by convergent computation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="generator"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="generator"/> has <see cref="CFCoefficientGenerator.Length"/> 0.
    /// </exception>
    public ContinuedFraction(CFCoefficientGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        if (generator.Length is 0)
        {
            throw new ArgumentException(
                "Generator must yield at least one coefficient.",
                nameof(generator));
        }

        _generator = generator;
    }

    /// <summary>The integer part a0 of the continued fraction.</summary>
    public BigInteger IntegerPart => _generator[0];

    /// <summary>
    /// True when the continued fraction is finite with exactly one
    /// coefficient (i.e. it represents an integer). Unbounded
    /// continued fractions are never integers; finite continued
    /// fractions of length &gt; 1 are not either.
    /// </summary>
    public bool IsInteger => _generator.Length == 1;

    private static ReadOnlyCollection<BigInteger> ValidateAndFreeze(
        IEnumerable<BigInteger> coefficients)
    {
        ArgumentNullException.ThrowIfNull(coefficients);

        var list = coefficients.ToArray();
        if (list.Length == 0)
        {
            throw new ArgumentException(
                "At least one coefficient is required.",
                nameof(coefficients));
        }

        for (var i = 1; i < list.Length; i++)
        {
            if (list[i].Sign <= 0)
            {
                throw new ArgumentException(
                    $"Partial quotient a{i.ToString(CultureInfo.InvariantCulture)} " +
                    $"must be positive (got {list[i].ToString(CultureInfo.InvariantCulture)}).",
                    nameof(coefficients));
            }
        }

        return new ReadOnlyCollection<BigInteger>(list);
    }

    // ---------- equality ----------

    /// <summary>
    /// Two continued fractions are equal when their coefficient sequences
    /// match element-by-element. No canonicalization is performed.
    /// </summary>
    public bool Equals(ContinuedFraction? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (Coefficients.Count != other.Coefficients.Count)
        {
            return false;
        }
        for (var i = 0; i < Coefficients.Count; i++)
        {
            if (Coefficients[i] != other.Coefficients[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ContinuedFraction cf && Equals(cf);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = default(HashCode);
        foreach (var coefficient in Coefficients)
        {
            hash.Add(coefficient);
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Returns true when both operands are null, or both are non-null and
    /// have equal coefficient sequences.
    /// </summary>
    public static bool operator ==(ContinuedFraction? left, ContinuedFraction? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        return left is not null && left.Equals(right);
    }

    /// <summary>Returns the negation of <c>operator ==</c>.</summary>
    public static bool operator !=(ContinuedFraction? left, ContinuedFraction? right)
        => !(left == right);

    // ---------- formatting ----------

    /// <summary>
    /// Returns the canonical rendering <c>[a0; a1, a2, ...]</c> using the
    /// current culture.
    /// </summary>
    public override string ToString() => ToString(null, CultureInfo.CurrentCulture);

    /// <inheritdoc cref="ToString(string?, IFormatProvider?)"/>
    public string ToString(IFormatProvider? formatProvider)
        => ToString(null, formatProvider);

    /// <summary>
    /// Returns the canonical rendering <c>[a0; a1, a2, ...]</c>.
    /// <paramref name="format"/> is forwarded to
    /// <see cref="BigInteger.ToString(string?, IFormatProvider?)"/> for each
    /// coefficient.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        var builder = new StringBuilder();
        builder.Append('[');
        builder.Append(Coefficients[0].ToString(format, formatProvider));
        if (Coefficients.Count > 1)
        {
            builder.Append("; ");
            for (var i = 1; i < Coefficients.Count; i++)
            {
                if (i > 1)
                {
                    builder.Append(", ");
                }
                builder.Append(Coefficients[i].ToString(format, formatProvider));
            }
        }
        builder.Append(']');
        return builder.ToString();
    }
}
