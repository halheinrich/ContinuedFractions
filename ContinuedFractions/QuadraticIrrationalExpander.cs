using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// Runs Lagrange's algorithm to produce the continued-fraction expansion
/// of a quadratic irrational <c>α = (√d − p)/q</c>, detecting the period
/// via state-cycle tracking on the <c>(P, Q)</c> pairs of the recurrence.
/// </summary>
/// <remarks>
/// <para>
/// Internally the algorithm uses the standard <c>α = (P + √D)/Q</c>
/// representation with signed <c>P</c>. The user-facing canonical form
/// <c>(√d − p)/q</c> with non-negative <c>p</c> maps to
/// <c>P₀ = −p</c>, <c>Q₀ = q</c>.
/// </para>
/// <para>
/// The recurrence is:
/// <code>
///   aₙ      = ⌊(Pₙ + √D)/Qₙ⌋
///   Pₙ₊₁    = aₙ·Qₙ − Pₙ
///   Qₙ₊₁    = (D − Pₙ₊₁²) / Qₙ
/// </code>
/// The divisibility constraint <c>q | d − p²</c> on the input ensures
/// every <c>Qₙ</c> is an exact integer. For non-perfect-square <c>D</c>
/// only finitely many distinct <c>(P, Q)</c> states are reachable, so
/// the state sequence is eventually periodic — Lagrange's theorem.
/// </para>
/// </remarks>
internal static class QuadraticIrrationalExpander
{
    /// <summary>
    /// Attempts to expand <c>(√d − p)/q</c> as a continued fraction,
    /// returning the pre-period and period coefficient lists when a
    /// cycle is detected within <paramref name="maxSteps"/> recurrence
    /// steps.
    /// </summary>
    /// <param name="d">Radicand. Must be non-negative.</param>
    /// <param name="p">Integer offset. Any sign permitted.</param>
    /// <param name="q">Denominator. Must be strictly positive.</param>
    /// <param name="maxSteps">
    /// Maximum recurrence steps before giving up. Must be strictly
    /// positive. Cycle detection on a quadratic surd needs at least
    /// <c>preperiod + period + 1</c> steps; for a CF with combined
    /// length <c>N</c>, pass <c>maxSteps ≥ N + 1</c>.
    /// </param>
    /// <returns>
    /// A pair of pre-period and period coefficient lists when a cycle
    /// is detected (period non-empty for irrational <c>α</c>; period
    /// empty for perfect-square <c>d</c> resolving to a rational).
    /// <see langword="null"/> if the input is not a valid canonical form
    /// (<c>d &lt; 0</c>, <c>q ≤ 0</c>, or <c>q ∤ d − p²</c>) or if
    /// <paramref name="maxSteps"/> was reached without cycle detection.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxSteps"/> is non-positive.
    /// </exception>
    public static (IReadOnlyList<BigInteger> Preperiod, IReadOnlyList<BigInteger> Period)?
        TryExpand(BigInteger d, BigInteger p, BigInteger q, int maxSteps)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSteps);

        if (d.Sign < 0 || q.Sign <= 0)
        {
            return null;
        }
        if (!((d - p * p) % q).IsZero)
        {
            return null;
        }

        var s = IntegerMath.Sqrt(d);
        var perfectSquare = s * s == d;

        // Perfect-square D ⇒ α = (s − p)/q is rational. The Lagrange
        // recurrence has degenerate cases here (the rationalization step
        // divides by zero whenever the next state's |P| would equal s),
        // so we expand rationals via the Euclidean-style algorithm
        // instead. Result is a pre-period with empty period.
        if (perfectSquare)
        {
            return ExpandRational(s - p, q, maxSteps);
        }

        // Non-square D. Internal state: α = (P + √D)/Q. User's
        // (√d − p)/q ↦ P₀ = −p, Q₀ = q.
        var bigD = d;
        var bigP = -p;
        var bigQ = q;

        var coeffs = new List<BigInteger>();
        var seen = new Dictionary<(BigInteger P, BigInteger Q), int>();

        for (var step = 0; step < maxSteps; step++)
        {
            if (seen.TryGetValue((bigP, bigQ), out var firstSeen))
            {
                var pre = coeffs.GetRange(0, firstSeen);
                var per = coeffs.GetRange(firstSeen, coeffs.Count - firstSeen);
                return (pre, per);
            }
            seen[(bigP, bigQ)] = step;

            // Compute aₙ = ⌊(P + √D)/Q⌋ using integer arithmetic only.
            // √D ∈ (s, s+1) for non-square D, so P + √D ∈ (P + s, P + s + 1):
            //   • Q > 0: floor = ⌊(P + s)/Q⌋. The candidate
            //     ⌊(P + s)/Q⌋ + 1 would require (P + s) + 1 ≤ √D, but
            //     (P + s) + 1 > √D for non-square D, ruling it out.
            //   • Q < 0: dividing by negative Q flips the interval to
            //     ((P + s + 1)/Q, (P + s)/Q). Largest k ≤ α is
            //     ⌊(P + s + 1)/Q⌋ via Euclidean division.
            var floorNumerator = bigQ.Sign > 0 ? bigP + s : bigP + s + BigInteger.One;
            var a = EuclideanDivide(floorNumerator, bigQ);
            coeffs.Add(a);

            // Recurrence step. For non-square D, D − pNext² is never
            // zero (pNext is integer, √D is irrational), so qNext is a
            // non-zero integer.
            var pNext = a * bigQ - bigP;
            var qNext = (bigD - pNext * pNext) / bigQ;
            bigP = pNext;
            bigQ = qNext;
        }

        return null;
    }

    /// <summary>
    /// Expands a rational <c>numerator / denominator</c> as a continued
    /// fraction via Euclidean-style iteration, returning all coefficients
    /// as the pre-period (period is empty). Terminates when the residual
    /// hits zero, which happens within <c>O(log(max(|num|, |den|)))</c>
    /// steps.
    /// </summary>
    private static (IReadOnlyList<BigInteger> Preperiod, IReadOnlyList<BigInteger> Period)?
        ExpandRational(BigInteger numerator, BigInteger denominator, int maxSteps)
    {
        var num = numerator;
        var den = denominator;
        var coeffs = new List<BigInteger>();

        for (var step = 0; step < maxSteps; step++)
        {
            // Normalise the divisor sign so Euclidean floor works on a
            // positive denominator. num/den is invariant under joint
            // sign flip.
            if (den.Sign < 0)
            {
                num = -num;
                den = -den;
            }

            var a = EuclideanDivide(num, den);
            coeffs.Add(a);

            var rem = num - a * den;  // in [0, den) since den > 0 here
            if (rem.IsZero)
            {
                return (coeffs, Array.Empty<BigInteger>());
            }

            // Next: 1 / (α − a) = den / rem.
            num = den;
            den = rem;
        }

        return null;
    }

    /// <summary>
    /// Floor (toward −∞) division for any non-zero divisor sign, where
    /// <see cref="BigInteger.DivRem(BigInteger, BigInteger)"/> rounds
    /// toward zero.
    /// </summary>
    private static BigInteger EuclideanDivide(BigInteger a, BigInteger b)
    {
        var (quot, rem) = BigInteger.DivRem(a, b);
        if (!rem.IsZero && rem.Sign != b.Sign)
        {
            quot -= BigInteger.One;
        }
        return quot;
    }
}
