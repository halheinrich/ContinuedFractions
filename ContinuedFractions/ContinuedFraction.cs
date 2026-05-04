using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using ContinuedFractions.Generators;
using HalHeinrich.Numerics;

namespace ContinuedFractions;

/// <summary>
/// A simple continued fraction <c>[a0; a1, a2, ...]</c> where <c>a0</c> is
/// the integer part (any sign) and <c>a1, a2, ...</c> are strictly positive
/// partial quotients. May be finite (representing a rational) or unbounded
/// (representing an irrational).
/// </summary>
/// <remarks>
/// <para>
/// Iterating an instance yields successive convergents <c>p_n / q_n</c>
/// as <see cref="BigRational"/> values. Convergents are memoized
/// internally — repeated indexer access or re-iteration is cheap.
/// </para>
/// <para>
/// For unbounded continued fractions iteration runs indefinitely; the
/// consumer is responsible for terminating via <c>Take</c>, <c>break</c>,
/// or a convergence check.
/// </para>
/// <para>
/// Value equality is not provided. Two continued fractions that happen
/// to represent the same number are not considered equal — equality on
/// generator-backed CFs is undecidable in general. The inherited
/// reference-equality semantics from <see cref="object"/> apply.
/// </para>
/// <para>Instances are not thread-safe.</para>
/// </remarks>
public sealed class ContinuedFraction : IEnumerable<BigRational>
{
    private readonly CFCoefficientGenerator _generator;

    // Memoized convergents and recurrence state. The recurrence is
    //   p_{-1} = 1, p_{-2} = 0
    //   q_{-1} = 0, q_{-2} = 1
    //   p_n = a_n * p_{n-1} + p_{n-2}
    //   q_n = a_n * q_{n-1} + q_{n-2}
    // and the convergent at depth n is p_n / q_n.
    private readonly List<BigRational> _convergents = [];
    private BigInteger _pPrev = BigInteger.One;
    private BigInteger _pPrevPrev = BigInteger.Zero;
    private BigInteger _qPrev = BigInteger.Zero;
    private BigInteger _qPrevPrev = BigInteger.One;

    /// <summary>
    /// The underlying coefficient generator. Iterate or index this for
    /// access to the partial quotients of any continued fraction,
    /// regardless of how it was constructed.
    /// </summary>
    public CFCoefficientGenerator Generator => _generator;

    /// <summary>
    /// Constructs a continued fraction from its coefficient sequence.
    /// </summary>
    /// <param name="coefficients">
    /// The coefficients in order: integer part first, then partial quotients.
    /// At least one coefficient is required; partial quotients (a1 onward)
    /// must be strictly positive.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="coefficients"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The sequence is empty, or a partial quotient is non-positive.
    /// </exception>
    public ContinuedFraction(IEnumerable<BigInteger> coefficients)
    {
        _generator = new ListCFCoefficientGenerator(ValidateAndFreeze(coefficients));
    }

    /// <inheritdoc cref="ContinuedFraction(IEnumerable{BigInteger})"/>
    public ContinuedFraction(params BigInteger[] coefficients)
        : this((IEnumerable<BigInteger>)coefficients)
    {
    }

    /// <summary>
    /// Constructs a continued fraction backed by a coefficient generator.
    /// </summary>
    /// <param name="generator">
    /// The source of partial quotients. Must yield at least one
    /// coefficient (i.e. its <see cref="CFCoefficientGenerator.Length"/>
    /// is either <see langword="null"/> — unbounded — or at least 1).
    /// </param>
    /// <remarks>
    /// CF invariants on partial-quotient values
    /// (<c>a_i</c> strictly positive for <c>i ≥ 1</c>) are not validated
    /// at construction; they are checked lazily when the generator is
    /// consumed by convergent computation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="generator"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="generator"/> has <see cref="CFCoefficientGenerator.Length"/> 0.
    /// </exception>
    public ContinuedFraction(CFCoefficientGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        if (generator.Length is 0)
        {
            throw new ArgumentException(
                "Generator must yield at least one coefficient.",
                nameof(generator));
        }

        _generator = generator;
    }

    /// <summary>The integer part a0 of the continued fraction.</summary>
    public BigInteger IntegerPart => _generator[0];

    // ---------- convergents ----------

    /// <summary>
    /// The convergent at the given <paramref name="depth"/> — the rational
    /// value <c>p_depth / q_depth</c> formed by truncating the continued
    /// fraction at that index.
    /// </summary>
    /// <param name="depth">Zero-based depth at which to evaluate.</param>
    /// <returns>The depth-th convergent.</returns>
    /// <remarks>
    /// First access at a given depth costs O(depth) <see cref="BigInteger"/>
    /// ops past the existing cache; subsequent accesses are O(1).
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="depth"/> is negative, or — for a finite continued
    /// fraction — greater than or equal to the generator's length.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Computing the convergent required pulling a partial quotient
    /// <c>a_n</c> with <c>n ≥ 1</c> that was non-positive, violating the
    /// CF invariant.
    /// </exception>
    public BigRational this[int depth]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(depth);
            if (_generator.Length is int n && depth >= n)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(depth),
                    depth,
                    $"Depth must be less than the generator's length ({n.ToString(CultureInfo.InvariantCulture)}).");
            }

            EnsureComputedThrough(depth);
            return _convergents[depth];
        }
    }

    /// <summary>
    /// Yields successive convergents <c>p_0/q_0, p_1/q_1, p_2/q_2, …</c>.
    /// </summary>
    /// <remarks>
    /// For finite continued fractions iteration completes after the last
    /// coefficient. For unbounded continued fractions iteration runs
    /// indefinitely — the consumer must terminate via <c>Take</c>,
    /// <c>break</c>, or a convergence check.
    /// </remarks>
    public IEnumerator<BigRational> GetEnumerator()
    {
        var length = _generator.Length;
        var i = 0;
        while (length is null || i < length.Value)
        {
            EnsureComputedThrough(i);
            yield return _convergents[i];
            i++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Extends the convergent cache to at least <paramref name="depth"/> + 1
    /// entries, pulling and validating new partial quotients from the
    /// generator as needed.
    /// </summary>
    private void EnsureComputedThrough(int depth)
    {
        while (_convergents.Count <= depth)
        {
            var n = _convergents.Count;
            var an = _generator[n];

            if (n >= 1 && an.Sign <= 0)
            {
                throw new InvalidOperationException(
                    $"Partial quotient a{n.ToString(CultureInfo.InvariantCulture)} " +
                    $"must be positive (got {an.ToString(CultureInfo.InvariantCulture)}).");
            }

            var pn = an * _pPrev + _pPrevPrev;
            var qn = an * _qPrev + _qPrevPrev;
            _convergents.Add(new BigRational(pn, qn));

            _pPrevPrev = _pPrev;
            _pPrev = pn;
            _qPrevPrev = _qPrev;
            _qPrev = qn;
        }
    }

    // ---------- formatting ----------

    /// <summary>
    /// Returns the generator's identification (e.g. <c>"φ"</c>,
    /// <c>"√2"</c>, or <c>"[1; 2, 3]"</c> for a list-backed CF).
    /// Depth-bounded rendering of an unbounded CF is the caller's
    /// responsibility — iterate <see cref="Generator"/> or this CF
    /// directly.
    /// </summary>
    public override string ToString() => _generator.ToString();

    private static ReadOnlyCollection<BigInteger> ValidateAndFreeze(
        IEnumerable<BigInteger> coefficients)
    {
        ArgumentNullException.ThrowIfNull(coefficients);

        var list = coefficients.ToArray();
        if (list.Length == 0)
        {
            throw new ArgumentException(
                "At least one coefficient is required.",
                nameof(coefficients));
        }

        for (var i = 1; i < list.Length; i++)
        {
            if (list[i].Sign <= 0)
            {
                throw new ArgumentException(
                    $"Partial quotient a{i.ToString(CultureInfo.InvariantCulture)} " +
                    $"must be positive (got {list[i].ToString(CultureInfo.InvariantCulture)}).",
                    nameof(coefficients));
            }
        }

        return new ReadOnlyCollection<BigInteger>(list);
    }
}
