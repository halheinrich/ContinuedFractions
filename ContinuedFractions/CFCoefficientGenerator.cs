using System.Collections;
using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// A typed source of continued-fraction partial quotients
/// <c>a₀, a₁, a₂, …</c>, indexable by position and enumerable in order.
/// </summary>
/// <remarks>
/// <para>
/// Generators may be finite (when <see cref="Length"/> is non-null) or
/// unbounded (when <see cref="Length"/> is <see langword="null"/>). The
/// enumerator respects <see cref="Length"/> when set, completing
/// naturally once that many values have been yielded. For unbounded
/// generators iteration never terminates on its own — the consumer is
/// responsible for stopping, e.g. via <c>Take</c>, <c>break</c>, or a
/// convergence check on the resulting convergents.
/// </para>
/// <para>
/// CF invariants on the partial-quotient values themselves (<c>a₀</c> any
/// sign; <c>a_i</c> strictly positive for <c>i ≥ 1</c>) are not enforced
/// here — they are enforced when a generator is consumed by
/// <see cref="ContinuedFraction"/>. This keeps generators usable as
/// general integer-sequence sources.
/// </para>
/// <para>Instances are not thread-safe.</para>
/// </remarks>
public abstract class CFCoefficientGenerator : IEnumerable<BigInteger>
{
    /// <summary>
    /// Returns the coefficient at the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Zero-based index into the coefficient sequence.</param>
    /// <returns>The coefficient at <paramref name="index"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative, or — for a finite generator —
    /// is greater than or equal to <see cref="Length"/>.
    /// </exception>
    public abstract BigInteger this[int index] { get; }

    /// <summary>
    /// The number of coefficients this generator yields, or
    /// <see langword="null"/> if the generator is unbounded.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/> (unbounded). Subclasses
    /// representing known-finite sequences override to advertise their
    /// length.
    /// </remarks>
    public virtual int? Length => null;

    /// <summary>A human-readable identification of this generator.</summary>
    /// <remarks>
    /// Subclasses must override. Examples: <c>"φ"</c>, <c>"√2"</c>, or a
    /// canonical bracket notation such as <c>"[1; 2, 3]"</c> for a known
    /// finite list.
    /// </remarks>
    public abstract override string ToString();

    /// <summary>
    /// Yields coefficients in index order. For finite generators iteration
    /// completes after <see cref="Length"/> values; otherwise iteration is
    /// unbounded and the consumer must terminate.
    /// </summary>
    public IEnumerator<BigInteger> GetEnumerator()
    {
        var i = 0;
        while (Length is null || i < Length.Value)
        {
            yield return this[i];
            i++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
