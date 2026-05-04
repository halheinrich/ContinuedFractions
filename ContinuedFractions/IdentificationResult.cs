namespace ContinuedFractions;

/// <summary>
/// The outcome of an attempt by a <see cref="CFIdentifier"/> to identify
/// a continued fraction as belonging to a known family.
/// </summary>
/// <param name="Match">
/// <see langword="true"/> when the identifier recognised the continued
/// fraction; <see langword="false"/> otherwise.
/// </param>
/// <param name="Identification">
/// A human-readable identification of the recognised value (e.g.
/// <c>"√7"</c> or <c>"e/3"</c>) when <paramref name="Match"/> is
/// <see langword="true"/>; <see langword="null"/> otherwise.
/// </param>
/// <param name="Depth">
/// The convergent depth at which the identifier reached its conclusion —
/// either the depth at which a match was confirmed, or the depth at
/// which the identifier exhausted its budget.
/// </param>
/// <remarks>
/// Prefer the <see cref="Matched"/> and <see cref="NotMatched"/> static
/// factories over the public positional constructor; they enforce the
/// invariants (matched results carry a non-null identification, depth
/// is non-negative).
/// </remarks>
public readonly record struct IdentificationResult(
    bool Match,
    string? Identification,
    int Depth)
{
    /// <summary>
    /// Constructs a matched result.
    /// </summary>
    /// <param name="identification">
    /// Human-readable identification of the recognised value.
    /// </param>
    /// <param name="depth">
    /// Convergent depth at which the match was confirmed.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="identification"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="depth"/> is negative.
    /// </exception>
    public static IdentificationResult Matched(string identification, int depth)
    {
        ArgumentNullException.ThrowIfNull(identification);
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        return new IdentificationResult(true, identification, depth);
    }

    /// <summary>
    /// Constructs an unmatched result.
    /// </summary>
    /// <param name="depth">
    /// Convergent depth at which the identifier gave up — typically the
    /// budget exhausted before concluding.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="depth"/> is negative.
    /// </exception>
    public static IdentificationResult NotMatched(int depth)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        return new IdentificationResult(false, null, depth);
    }
}
