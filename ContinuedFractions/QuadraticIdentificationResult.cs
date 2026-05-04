namespace ContinuedFractions;

/// <summary>
/// The structured outcome of an attempt by a
/// <see cref="QuadraticIrrationalIdentifier"/> to identify a continued
/// fraction as a quadratic irrational, carrying the recovered
/// <see cref="QuadraticIrrational"/> as typed data.
/// </summary>
/// <param name="Match">
/// <see langword="true"/> when the identifier recognised the continued
/// fraction; <see langword="false"/> otherwise.
/// </param>
/// <param name="Value">
/// The recovered canonical-form <see cref="QuadraticIrrational"/> when
/// <paramref name="Match"/> is <see langword="true"/>;
/// <see langword="null"/> otherwise.
/// </param>
/// <param name="Depth">
/// The convergent depth at which the identifier reached its conclusion —
/// either the depth at which a match was confirmed, or the deepest
/// coefficient compared before the identifier exhausted its budget.
/// </param>
/// <remarks>
/// <para>
/// This is the typed companion to <see cref="IdentificationResult"/>:
/// the same logical answer, but with the matched value exposed as
/// structured <c>(D, P, Q)</c> data rather than a string. The non-typed
/// <see cref="IdentificationResult"/> contract on
/// <see cref="CFIdentifier"/> still applies for harness use; this type
/// is for direct callers who want to compute with the recovered triple.
/// </para>
/// <para>
/// Prefer the <see cref="Matched"/> and <see cref="NotMatched"/> static
/// factories over the public positional constructor; they enforce the
/// invariants (matched results carry a non-null value; depth is
/// non-negative).
/// </para>
/// </remarks>
public readonly record struct QuadraticIdentificationResult(
    bool Match,
    QuadraticIrrational? Value,
    int Depth)
{
    /// <summary>Constructs a matched result.</summary>
    /// <param name="value">The recovered quadratic irrational.</param>
    /// <param name="depth">The convergent depth at which the match was confirmed.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="depth"/> is negative.
    /// </exception>
    public static QuadraticIdentificationResult Matched(QuadraticIrrational value, int depth)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        return new QuadraticIdentificationResult(true, value, depth);
    }

    /// <summary>Constructs an unmatched result.</summary>
    /// <param name="depth">
    /// The deepest coefficient examined before giving up.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="depth"/> is negative.
    /// </exception>
    public static QuadraticIdentificationResult NotMatched(int depth)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        return new QuadraticIdentificationResult(false, null, depth);
    }
}
