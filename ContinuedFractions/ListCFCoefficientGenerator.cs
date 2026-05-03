using System.Globalization;
using System.Numerics;
using System.Text;

namespace ContinuedFractions;

/// <summary>
/// A finite <see cref="CFCoefficientGenerator"/> backed by a fixed
/// list of coefficients. Used internally as the storage for continued
/// fractions constructed from explicit coefficient sequences.
/// </summary>
internal sealed class ListCFCoefficientGenerator : CFCoefficientGenerator
{
    /// <summary>The wrapped coefficient list.</summary>
    /// <remarks>
    /// The caller is responsible for ensuring the supplied list is
    /// immutable; this generator does not defensively copy.
    /// </remarks>
    public IReadOnlyList<BigInteger> Items { get; }

    /// <summary>
    /// Wraps <paramref name="coefficients"/> as a finite generator.
    /// </summary>
    /// <param name="coefficients">An immutable list of coefficients.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="coefficients"/> is <see langword="null"/>.
    /// </exception>
    public ListCFCoefficientGenerator(IReadOnlyList<BigInteger> coefficients)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        Items = coefficients;
    }

    /// <inheritdoc/>
    public override int? Length => Items.Count;

    /// <inheritdoc/>
    public override BigInteger this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Items.Count);
            return Items[index];
        }
    }

    /// <summary>
    /// Returns the canonical bracket notation
    /// <c>[a₀; a₁, a₂, …]</c> in invariant culture, or <c>"[]"</c> for
    /// an empty list.
    /// </summary>
    public override string ToString()
    {
        if (Items.Count == 0)
        {
            return "[]";
        }

        var builder = new StringBuilder();
        builder.Append('[');
        builder.Append(Items[0].ToString(CultureInfo.InvariantCulture));
        if (Items.Count > 1)
        {
            builder.Append("; ");
            for (var i = 1; i < Items.Count; i++)
            {
                if (i > 1)
                {
                    builder.Append(", ");
                }

                builder.Append(Items[i].ToString(CultureInfo.InvariantCulture));
            }
        }

        builder.Append(']');
        return builder.ToString();
    }
}
