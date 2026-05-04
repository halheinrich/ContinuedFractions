using System.Globalization;
using System.Numerics;
using HalHeinrich.Numerics;

namespace ContinuedFractions;

/// <summary>
/// Identifies a continued fraction as the principal square root √n of a
/// positive integer.
/// </summary>
/// <remarks>
/// <para>
/// Strategy: walk the convergents <c>p_k / q_k</c>. At each step, take
/// the nearest integer to <c>(p_k / q_k)²</c> as the candidate
/// <c>n</c>, then check whether the convergent's square is within
/// <c>tolerance</c> of <c>n</c>:
/// <c>|(p_k / q_k)² − n| &lt; tolerance</c>. For genuine convergents
/// of √n this distance shrinks toward zero as <c>q_k</c> grows, so the
/// tolerance check is eventually satisfied. Non-square-root
/// irrationals don't approach any integer square, so the check fails.
/// </para>
/// <para>
/// A match is reported when the candidate <c>n</c> stays the same
/// across <see cref="RequiredStableDepth"/> consecutive convergents
/// that all pass the tolerance check. Stability protects against
/// spurious early matches — e.g. the depth-0 convergent is always an
/// integer and trivially satisfies any positive tolerance for
/// <c>n = a₀²</c>.
/// </para>
/// <para>
/// Tighter tolerances require deeper convergents to confirm: with the
/// default <see cref="DefaultTolerance"/> of <c>10⁻⁶</c>, √2 matches
/// near depth 11; tightening to <c>10⁻⁹</c> pushes that out to roughly
/// depth 16. The default budget of <see cref="DefaultMaxDepth"/>
/// convergents is comfortable for any practical √n at this tolerance.
/// </para>
/// <para>
/// Finite continued fractions (representing rationals) almost never
/// match — their convergents don't approach an integer square. The
/// degenerate case of a length-1 CF representing an integer is also
/// not matched, because stability across multiple convergents cannot
/// be established.
/// </para>
/// </remarks>
public sealed class SquareRootIdentifier : CFIdentifier
{
    /// <summary>Default maximum convergent depth before giving up.</summary>
    public const int DefaultMaxDepth = 50;

    /// <summary>
    /// Number of consecutive convergents that must agree on the
    /// candidate <c>n</c> (and pass the tolerance check) before the
    /// match is accepted.
    /// </summary>
    public const int RequiredStableDepth = 3;

    /// <summary>
    /// Default tolerance for the convergent-square-distance check:
    /// <c>10⁻⁶</c> (one part per million).
    /// </summary>
    public static readonly BigRational DefaultTolerance = new(1, 1_000_000);

    private readonly int _maxDepth;
    private readonly BigInteger _toleranceNum;
    private readonly BigInteger _toleranceDen;

    /// <summary>
    /// Constructs an identifier with the given convergent budget and
    /// match tolerance.
    /// </summary>
    /// <param name="maxDepth">
    /// Maximum convergent depth to examine before giving up.
    /// </param>
    /// <param name="tolerance">
    /// The maximum permitted distance between a convergent's square
    /// and its rounded integer for the convergent to count as evidence
    /// of the match. <see langword="null"/> uses
    /// <see cref="DefaultTolerance"/> (10⁻⁶). Must be strictly positive.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxDepth"/> is less than
    /// <see cref="RequiredStableDepth"/>, or
    /// <paramref name="tolerance"/> is non-positive.
    /// </exception>
    public SquareRootIdentifier(
        int maxDepth = DefaultMaxDepth,
        BigRational? tolerance = null)
    {
        if (maxDepth < RequiredStableDepth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDepth),
                maxDepth,
                $"Maximum depth must be at least {RequiredStableDepth.ToString(CultureInfo.InvariantCulture)}.");
        }

        var t = tolerance ?? DefaultTolerance;
        if (t.Numerator.Sign <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tolerance),
                t,
                "Tolerance must be strictly positive.");
        }

        _maxDepth = maxDepth;
        _toleranceNum = t.Numerator;
        _toleranceDen = t.Denominator;
    }

    /// <inheritdoc/>
    protected override IdentificationResult TryIdentifyCore(ContinuedFraction cf)
    {
        BigInteger candidate = default;
        var hasCandidate = false;
        var stableCount = 0;
        var depth = -1;

        foreach (var convergent in cf)
        {
            depth++;

            var p = convergent.Numerator;
            var q = convergent.Denominator;
            var p2 = p * p;
            var q2 = q * q;

            // n = round(p² / q²) using integer DivRem and a half-up rule.
            var (n, rem) = BigInteger.DivRem(p2, q2);
            if (rem * 2 >= q2)
            {
                n += BigInteger.One;
            }

            // Tolerance check: |p² − n·q²| / q² < tolNum / tolDen.
            // Cross-multiplied (q² > 0, tolDen > 0):
            //     |p² − n·q²| · tolDen < tolNum · q²
            // — pure integer arithmetic, exact.
            var keep = false;
            if (n.Sign > 0)
            {
                var residualAbs = BigInteger.Abs(p2 - n * q2);
                if (residualAbs * _toleranceDen < _toleranceNum * q2)
                {
                    keep = true;
                }
            }

            if (!keep)
            {
                hasCandidate = false;
                stableCount = 0;
            }
            else if (hasCandidate && candidate == n)
            {
                stableCount++;
                if (stableCount >= RequiredStableDepth)
                {
                    return IdentificationResult.Matched(
                        $"√{n.ToString(CultureInfo.InvariantCulture)}",
                        depth);
                }
            }
            else
            {
                candidate = n;
                hasCandidate = true;
                stableCount = 1;
            }

            if (depth >= _maxDepth)
            {
                return IdentificationResult.NotMatched(depth);
            }
        }

        // Iterator completed naturally (finite CF) without reaching stability.
        return IdentificationResult.NotMatched(Math.Max(depth, 0));
    }
}
