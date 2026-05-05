using System.Collections;
using System.Globalization;
using System.Numerics;
using ContinuedFractions.Generators;
using HalHeinrich.Numerics;

namespace ContinuedFractions;

/// <summary>
/// A simple continued fraction <c>[a0; a1, a2, ...]</c> where <c>a0</c> is
/// the integer part (any sign) and <c>a1, a2, ...</c> are strictly positive
/// partial quotients. Constructed from a
/// <see cref="PatternCFCoefficientGenerator"/>, which describes the
/// CF's structure (pre-period plus optional cycling lanes) and exposes
/// the structural classifiers
/// <see cref="PatternCFCoefficientGenerator.IsRational"/> and
/// <see cref="PatternCFCoefficientGenerator.IsQuadraticIrrational"/>.
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
    private readonly PatternCFCoefficientGenerator _generator;

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
    /// The underlying pattern generator. Iterate or index this for
    /// access to the partial quotients, and consult its
    /// <see cref="PatternCFCoefficientGenerator.IsRational"/> /
    /// <see cref="PatternCFCoefficientGenerator.IsQuadraticIrrational"/>
    /// classifiers for structural facts about the CF.
    /// </summary>
    public PatternCFCoefficientGenerator Generator => _generator;

    /// <summary>
    /// Constructs a continued fraction backed by a
    /// <see cref="PatternCFCoefficientGenerator"/>.
    /// </summary>
    /// <param name="generator">
    /// The pattern describing this CF's structure. The pattern is the
    /// sole CF construction surface — to express a value, build the
    /// pattern (directly or via a factory in
    /// <see cref="Patterns"/>) and pass it here.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="generator"/> is <see langword="null"/>.
    /// </exception>
    public ContinuedFraction(PatternCFCoefficientGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
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
    /// Returns the underlying pattern's rendering.
    /// </summary>
    public override string ToString() => _generator.ToString();
}
