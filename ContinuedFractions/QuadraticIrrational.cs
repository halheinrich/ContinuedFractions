using System.Globalization;
using System.Numerics;

namespace HalHeinrich.Numerics.ContinuedFractions;

/// <summary>
/// A quadratic irrational expressed in canonical form
/// <c>α = (√D − P)/Q</c>, where <c>D ≥ 0</c>, <c>Q ≥ 1</c>, and
/// <c>P ∈ ℤ</c> (any sign), with <c>Q | D − P²</c>.
/// </summary>
/// <remarks>
/// <para>
/// The canonical form is the standard representation under which
/// Lagrange's continued-fraction expansion algorithm operates with exact
/// integer arithmetic — the divisibility constraint <c>Q | D − P²</c>
/// guarantees the recurrence produces integer states throughout.
/// </para>
/// <para>
/// <c>P</c> is signed: <c>P &gt; 0</c> renders as <c>(√D − P)/Q</c> and
/// covers values like <c>(√7 − 2)/3</c>; <c>P &lt; 0</c> renders as
/// <c>(√D + |P|)/Q</c> and covers values like the golden ratio
/// <c>φ = (1 + √5)/2</c>, expressed here as <c>D = 5, P = −1, Q = 2</c>.
/// Together with <c>P = 0</c> (plain square roots) this covers every
/// quadratic irrational.
/// </para>
/// <para>
/// Construction validates the constraints. The <see langword="default"/>
/// value (<c>D = P = Q = 0</c>) violates <c>Q ≥ 1</c> and should not be
/// relied on as a meaningful quadratic irrational; treat it as an
/// "invalid" sentinel.
/// </para>
/// <para>
/// When <c>D</c> is a perfect square the value is rational, not
/// irrational. The type is permissive on this front — perfect-square
/// <c>D</c> is accepted in canonical form even though the resulting
/// value isn't strictly a "quadratic irrational" — but consumers should
/// be aware that operations that assume irrationality (e.g. infinite
/// periodic CF) won't apply.
/// </para>
/// </remarks>
public readonly record struct QuadraticIrrational
{
    /// <summary>The radicand <c>D</c>, a non-negative integer.</summary>
    public BigInteger D { get; }

    /// <summary>
    /// The integer offset <c>P</c>, any sign — positive corresponds to
    /// <c>(√D − P)/Q</c>, negative to <c>(√D + |P|)/Q</c>.
    /// </summary>
    public BigInteger P { get; }

    /// <summary>The denominator <c>Q</c>, strictly positive.</summary>
    public BigInteger Q { get; }

    /// <summary>
    /// Constructs a quadratic irrational <c>(√d − p)/q</c> in canonical
    /// form, validating that <c>d ≥ 0</c>, <c>q ≥ 1</c>, and
    /// <c>q | d − p²</c>. <paramref name="p"/> may be any sign.
    /// </summary>
    /// <param name="d">The radicand. Must be non-negative.</param>
    /// <param name="p">The integer offset. Any sign permitted.</param>
    /// <param name="q">The denominator. Must be strictly positive.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="d"/> is negative or <paramref name="q"/> is
    /// non-positive.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="q"/> does not divide <c>d − p²</c>.
    /// </exception>
    public QuadraticIrrational(BigInteger d, BigInteger p, BigInteger q)
    {
        if (d.Sign < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(d), d, "Radicand D must be non-negative.");
        }
        if (q.Sign <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(q), q, "Denominator Q must be strictly positive.");
        }
        if (!((d - p * p) % q).IsZero)
        {
            throw new ArgumentException(
                $"Denominator Q ({q.ToString(CultureInfo.InvariantCulture)}) " +
                $"must divide D − P² ({(d - p * p).ToString(CultureInfo.InvariantCulture)}).",
                nameof(q));
        }

        D = d;
        P = p;
        Q = q;
    }

    /// <summary>
    /// Renders the value in canonical form, with simplifications:
    /// <c>"√d"</c> for <c>p = 0, q = 1</c>;
    /// <c>"√d/q"</c> for <c>p = 0, q ≠ 1</c>;
    /// <c>"√d − p"</c> or <c>"√d + |p|"</c> for <c>q = 1</c>;
    /// <c>"(√d ± |p|)/q"</c> otherwise.
    /// </summary>
    public override string ToString()
    {
        var sqrt = "√" + D.ToString(CultureInfo.InvariantCulture);
        string body;
        if (P.IsZero)
        {
            body = sqrt;
        }
        else if (P.Sign > 0)
        {
            body = $"{sqrt} − {P.ToString(CultureInfo.InvariantCulture)}";
        }
        else
        {
            body = $"{sqrt} + {(-P).ToString(CultureInfo.InvariantCulture)}";
        }

        if (Q.IsOne)
        {
            return body;
        }

        return P.IsZero
            ? $"{body}/{Q.ToString(CultureInfo.InvariantCulture)}"
            : $"({body})/{Q.ToString(CultureInfo.InvariantCulture)}";
    }
}
