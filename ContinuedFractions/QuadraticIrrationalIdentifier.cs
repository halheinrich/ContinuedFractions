using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// Identifies a continued fraction as a quadratic irrational of the form
/// <c>(√d − p)/q</c> by brute-force enumeration over candidate triples
/// <c>(d, p, q)</c>, comparing each candidate's CF against the input
/// coefficient-by-coefficient.
/// </summary>
/// <remarks>
/// <para>
/// The identifier subsumes square-root recognition: a continued fraction
/// of <c>√n</c> is matched by the triple <c>(n, 0, 1)</c>, which appears
/// early in the enumeration (level <c>n</c>'s square-root sweep).
/// </para>
/// <para>
/// <b>Enumeration order.</b> Candidate triples are visited in <em>levels</em>
/// indexed by <c>k = 1, 2, 3, …</c>. Level <c>k</c> consists of three
/// sweeps in lex order:
/// <list type="number">
/// <item>
/// the square-root sweep: every <c>(d, 0, q)</c> with <c>d, q ∈ {1..k}</c>
/// and <c>max(d, q) = k</c>;
/// </item>
/// <item>
/// the positive-<c>p</c> general sweep: every <c>(d, p, q) ∈ {1..k}³</c>
/// with <c>max(d, p, q) = k</c>;
/// </item>
/// <item>
/// the negative-<c>p</c> general sweep: every <c>(d, −p, q)</c> for the
/// same set of <c>(d, p, q)</c> triples — these capture quadratic
/// irrationals like <c>(1 + √5)/2</c> whose canonical form has
/// <c>p &lt; 0</c>.
/// </item>
/// </list>
/// </para>
/// <para>
/// <b>Match criterion.</b> For each candidate, Lagrange's algorithm
/// (<see cref="QuadraticIrrationalExpander"/>) produces a periodic CF.
/// A match is declared when the input CF agrees with the candidate's CF
/// (cycling through the candidate's period as needed) on the first
/// <see cref="DefaultMaxComparisonDepth"/> coefficients exactly. Going
/// well beyond <c>preperiod + period</c> is necessary: a short period
/// can prefix many distinct CFs, so a candidate with period
/// <c>[4]</c> would falsely "match" any input whose first two
/// coefficients happen to coincide. The deeper comparison rules out
/// such accidental prefix matches.
/// </para>
/// <para>
/// <b>Budgets.</b> Two ctor parameters bound the search:
/// <see cref="DefaultMaxTriples"/> caps the number of candidate triples
/// evaluated, and <see cref="DefaultMaxComparisonDepth"/> caps both the
/// expander's recurrence-step budget and the coefficient-comparison
/// depth — candidates whose pre-period plus period would exceed this
/// cap are skipped.
/// </para>
/// </remarks>
public sealed class QuadraticIrrationalIdentifier : CFIdentifier
{
    /// <summary>Default cap on candidate triples evaluated.</summary>
    /// <remarks>
    /// Sized to comfortably reach the square-root sweep at level ~21,
    /// covering plain <c>√n</c> for <c>n</c> up to ~21 and a substantial
    /// portion of the general-triple space below that (with both signs
    /// of <c>p</c>). Larger budgets trade evaluation time for broader
    /// coverage.
    /// </remarks>
    public const int DefaultMaxTriples = 20_000;

    /// <summary>
    /// Default cap on the expander's recurrence-step budget and the
    /// comparison depth per candidate.
    /// </summary>
    public const int DefaultMaxComparisonDepth = 64;

    private readonly int _maxTriples;
    private readonly int _maxComparisonDepth;

    /// <summary>
    /// Constructs an identifier with the given enumeration and
    /// comparison budgets.
    /// </summary>
    /// <param name="maxTriples">
    /// Maximum number of candidate triples evaluated before giving up.
    /// Must be strictly positive.
    /// </param>
    /// <param name="maxComparisonDepth">
    /// Maximum coefficient-comparison depth per candidate; also bounds
    /// the Lagrange expander's recurrence-step budget. Must be strictly
    /// positive.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Either parameter is non-positive.
    /// </exception>
    public QuadraticIrrationalIdentifier(
        int maxTriples = DefaultMaxTriples,
        int maxComparisonDepth = DefaultMaxComparisonDepth)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTriples);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxComparisonDepth);

        _maxTriples = maxTriples;
        _maxComparisonDepth = maxComparisonDepth;
    }

    /// <inheritdoc/>
    protected override IdentificationResult TryIdentifyCore(ContinuedFraction cf)
    {
        var typed = TryIdentifyQuadraticCore(cf);
        return typed.Match
            ? IdentificationResult.Matched(typed.Value!.Value.ToString(), typed.Depth)
            : IdentificationResult.NotMatched(typed.Depth);
    }

    /// <summary>
    /// Attempts to identify <paramref name="cf"/> as a quadratic
    /// irrational, returning the recovered canonical-form
    /// <see cref="QuadraticIrrational"/> as structured data when a match
    /// is found.
    /// </summary>
    /// <param name="cf">The continued fraction to classify.</param>
    /// <returns>
    /// A <see cref="QuadraticIdentificationResult"/> carrying the matched
    /// <see cref="QuadraticIrrational"/> (or <see langword="null"/>) and
    /// the depth at which the identifier concluded.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cf"/> is <see langword="null"/>.
    /// </exception>
    public QuadraticIdentificationResult TryIdentifyQuadratic(ContinuedFraction cf)
    {
        ArgumentNullException.ThrowIfNull(cf);
        return TryIdentifyQuadraticCore(cf);
    }

    private QuadraticIdentificationResult TryIdentifyQuadraticCore(ContinuedFraction cf)
    {
        var triplesTried = 0;
        var maxIndexExamined = -1;

        // Expander needs preperiod + period + 1 steps to detect a cycle
        // whose combined length is preperiod + period — pass one extra
        // step so the budget aligns with comparison depth.
        var expanderBudget = _maxComparisonDepth + 1;

        foreach (var (d, p, q) in EnumerateTriples())
        {
            if (triplesTried >= _maxTriples)
            {
                break;
            }
            triplesTried++;

            var expansion = QuadraticIrrationalExpander.TryExpand(d, p, q, expanderBudget);
            if (expansion is null)
            {
                continue;
            }

            var (preperiod, period) = expansion.Value;

            // A rational candidate (perfect-square d, empty period) can
            // only match a finite input CF whose coefficients agree
            // exactly. We don't pursue that here — the brute force is
            // aimed at quadratic irrationals.
            if (period.Count == 0)
            {
                continue;
            }

            var (matched, indexExamined) = TryMatch(cf, preperiod, period, _maxComparisonDepth);
            if (indexExamined > maxIndexExamined)
            {
                maxIndexExamined = indexExamined;
            }

            if (matched)
            {
                return QuadraticIdentificationResult.Matched(
                    new QuadraticIrrational(d, p, q),
                    indexExamined);
            }
        }

        return QuadraticIdentificationResult.NotMatched(Math.Max(maxIndexExamined, 0));
    }

    /// <summary>
    /// Compares the input CF's coefficients against a candidate's
    /// expanded pre-period and (cyclic) period, up to
    /// <paramref name="compareDepth"/> coefficients. Returns whether
    /// every position matched, plus the deepest coefficient index
    /// actually examined.
    /// </summary>
    private static (bool Matched, int IndexExamined) TryMatch(
        ContinuedFraction cf,
        IReadOnlyList<BigInteger> preperiod,
        IReadOnlyList<BigInteger> period,
        int compareDepth)
    {
        var generator = cf.Generator;
        var inputLength = generator.Length;
        var lastIndex = -1;

        for (var i = 0; i < compareDepth; i++)
        {
            if (inputLength is int len && i >= len)
            {
                // Finite input ran out before the candidate's CF closed
                // — can't be the same value (candidate has infinite CF).
                return (false, lastIndex);
            }

            var input = generator[i];
            var candidate = i < preperiod.Count
                ? preperiod[i]
                : period[(i - preperiod.Count) % period.Count];

            if (input != candidate)
            {
                return (false, i);
            }
            lastIndex = i;
        }

        return (true, lastIndex);
    }

    /// <summary>
    /// Yields candidate triples in level-then-sweep order: for each
    /// <c>k = 1, 2, 3, …</c>, first the square-root sweep
    /// <c>(d, 0, q)</c> with <c>max(d, q) = k</c> in lex order on
    /// <c>(d, q)</c>, then the positive-<c>p</c> general sweep
    /// <c>(d, p, q) ∈ {1..k}³</c> with <c>max(d, p, q) = k</c> in lex
    /// order on <c>(d, p, q)</c>, then the negative-<c>p</c> general
    /// sweep <c>(d, −p, q)</c> in the same order.
    /// </summary>
    private static IEnumerable<(BigInteger D, BigInteger P, BigInteger Q)> EnumerateTriples()
    {
        for (var k = 1; ; k++)
        {
            // Square-root sweep: (d, 0, q), max(d, q) = k.
            for (var d = 1; d <= k; d++)
            {
                for (var q = 1; q <= k; q++)
                {
                    if (Math.Max(d, q) == k)
                    {
                        yield return (new BigInteger(d), BigInteger.Zero, new BigInteger(q));
                    }
                }
            }

            // Positive-p general sweep: (d, p, q) ∈ {1..k}³, max(d, p, q) = k.
            for (var d = 1; d <= k; d++)
            {
                for (var p = 1; p <= k; p++)
                {
                    for (var q = 1; q <= k; q++)
                    {
                        if (Math.Max(Math.Max(d, p), q) == k)
                        {
                            yield return (new BigInteger(d), new BigInteger(p), new BigInteger(q));
                        }
                    }
                }
            }

            // Negative-p general sweep: (d, −p, q) for the same lex set.
            for (var d = 1; d <= k; d++)
            {
                for (var p = 1; p <= k; p++)
                {
                    for (var q = 1; q <= k; q++)
                    {
                        if (Math.Max(Math.Max(d, p), q) == k)
                        {
                            yield return (new BigInteger(d), -new BigInteger(p), new BigInteger(q));
                        }
                    }
                }
            }
        }
    }
}
