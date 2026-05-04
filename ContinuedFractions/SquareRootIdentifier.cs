using System.Globalization;
using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// Identifies a continued fraction as the principal square root √n of a
/// positive integer.
/// </summary>
/// <remarks>
/// <para>
/// Strategy: walk the convergents <c>p_k / q_k</c>. At each step,
/// take the nearest integer to <c>(p_k / q_k)²</c> as the candidate
/// <c>n</c>, then check the Pell-like residual
/// <c>|p_k² − n·q_k²|</c>. For genuine convergents of √n the residual
/// is bounded by <c>2√n + 1</c> at every depth — non-square-root
/// irrationals fail this bound after a few convergents at most.
/// </para>
/// <para>
/// A match is reported when the candidate <c>n</c> stays the same
/// across <see cref="RequiredStableDepth"/> consecutive convergents
/// that all pass the residual bound. The default budget of
/// <see cref="DefaultMaxDepth"/> convergents is ample for any
/// practical √n; even sluggish cases like √2 reach stability by
/// depth 3.
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
    /// candidate <c>n</c> (and pass the residual bound) before the
    /// match is accepted.
    /// </summary>
    public const int RequiredStableDepth = 3;

    private readonly int _maxDepth;

    /// <summary>
    /// Constructs an identifier with the given convergent budget.
    /// </summary>
    /// <param name="maxDepth">
    /// Maximum convergent depth to examine before giving up.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxDepth"/> is less than
    /// <see cref="RequiredStableDepth"/>.
    /// </exception>
    public SquareRootIdentifier(int maxDepth = DefaultMaxDepth)
    {
        if (maxDepth < RequiredStableDepth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDepth),
                maxDepth,
                $"Maximum depth must be at least {RequiredStableDepth.ToString(CultureInfo.InvariantCulture)}.");
        }

        _maxDepth = maxDepth;
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

            // Reject non-positive candidates and overly-large residuals.
            // For genuine √n convergents the residual is bounded by
            // 2√n + 1 at every depth; we use 2·⌊√n⌋ + 2 as a safe
            // integer upper bound.
            var keep = false;
            if (n.Sign > 0)
            {
                var residual = BigInteger.Abs(p2 - n * q2);
                var bound = (2 * IntegerSqrt(n)) + 2;
                if (residual <= bound)
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

    /// <summary>
    /// Returns <c>⌊√n⌋</c> for non-negative <paramref name="n"/> via
    /// Newton's method on <see cref="BigInteger"/>.
    /// </summary>
    private static BigInteger IntegerSqrt(BigInteger n)
    {
        if (n.Sign < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), n, "n must be non-negative.");
        }
        if (n < 2)
        {
            return n;
        }

        // Initial estimate: any value ≥ ⌈√n⌉. Start with n itself; Newton converges quickly.
        var x = n;
        var y = (x + 1) / 2;
        while (y < x)
        {
            x = y;
            y = (x + (n / x)) / 2;
        }
        return x;
    }
}
