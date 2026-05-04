namespace ContinuedFractions;

/// <summary>
/// A strategy for identifying a continued fraction as belonging to a
/// known family — e.g. square roots of integers, rational multiples of
/// <c>e</c> or <c>π</c>, periodic CFs, and so on.
/// </summary>
/// <remarks>
/// <para>
/// Identifiers are typically used by a higher-level harness that
/// dispatches a continued fraction through several identifiers in turn
/// and reports which (if any) recognised it. Each implementation owns
/// its own iteration budget and convergence criteria; the base contract
/// just says "given a CF, return whether you recognise it and at what
/// depth."
/// </para>
/// <para>
/// Implementations should iterate the supplied continued fraction on
/// demand (via <c>foreach</c>, <c>Take</c>, or the convergent indexer)
/// and stop as soon as they have an answer — either a confirmed match,
/// or exhaustion of the identifier's budget. Implementations should be
/// stateless across calls; mutating the supplied CF or relying on shared
/// state across identifiers is not supported.
/// </para>
/// </remarks>
public abstract class CFIdentifier
{
    /// <summary>
    /// Attempts to identify <paramref name="cf"/> as belonging to this
    /// identifier's family.
    /// </summary>
    /// <param name="cf">The continued fraction to classify.</param>
    /// <returns>
    /// An <see cref="IdentificationResult"/> describing whether the
    /// identifier recognised the continued fraction and at what depth.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cf"/> is <see langword="null"/>.
    /// </exception>
    public IdentificationResult TryIdentify(ContinuedFraction cf)
    {
        ArgumentNullException.ThrowIfNull(cf);
        return TryIdentifyCore(cf);
    }

    /// <summary>
    /// Implements the identifier's recognition logic for the given
    /// continued fraction. Called by <see cref="TryIdentify"/> after
    /// null-checking the argument; subclasses can assume
    /// <paramref name="cf"/> is non-null.
    /// </summary>
    /// <param name="cf">A non-null continued fraction.</param>
    /// <returns>The result of the identification attempt.</returns>
    protected abstract IdentificationResult TryIdentifyCore(ContinuedFraction cf);
}
