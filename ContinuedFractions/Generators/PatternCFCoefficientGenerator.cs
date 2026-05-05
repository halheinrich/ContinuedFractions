using System.Globalization;
using System.Numerics;

namespace ContinuedFractions.Generators;

/// <summary>
/// A continued-fraction coefficient generator built from a pre-period
/// (a finite sequence of explicit coefficients emitted once, in order)
/// followed by an optional periodic part composed of <see cref="Lane"/>s
/// that cycle indefinitely. Each lane evolves its emitted value across
/// visits according to its <see cref="Operation"/>.
/// </summary>
/// <remarks>
/// <para>
/// The generator is the natural source for hunting parametric CF
/// patterns: many "interesting" CFs (such as Euler's number
/// <c>e = [2; 1, 2, 1, 1, 4, 1, 1, 6, …]</c>) decompose into a short
/// pre-period plus a small set of cycling lanes whose values evolve
/// linearly or geometrically.
/// </para>
/// <para>
/// <b>Structural classification.</b> The generator's classifier
/// properties partition every CF it can produce into three disjoint
/// classes, derived purely from the constructor inputs:
/// <list type="bullet">
/// <item>
/// <see cref="IsRational"/> — empty lane cycle, just the finite
/// pre-period: the CF is a finite rational expansion.
/// </item>
/// <item>
/// <see cref="IsQuadraticIrrational"/> — non-empty lane cycle with every
/// lane being <see cref="Operation.Const"/>. By Lagrange's theorem the
/// CF is eventually periodic ⇔ the value is a quadratic irrational.
/// </item>
/// <item>
/// Neither — at least one lane is <see cref="Operation.Plus"/> or
/// <see cref="Operation.Multiply"/> (etc.), so the lane values grow,
/// the CF is not periodic, and the value is something other than a
/// quadratic irrational (often transcendental).
/// </item>
/// </list>
/// The "neither" bucket is what the discovery harness investigates.
/// </para>
/// <para>
/// <b>Indexing.</b> Position <c>i</c> of the CF resolves to:
/// <list type="bullet">
/// <item>
/// <c>preperiod[i]</c> when <c>i &lt; preperiod.Count</c>;
/// </item>
/// <item>
/// <c>lanes[laneIndex].ValueAt(visit)</c> otherwise, where
/// <c>j = i − preperiod.Count</c>, <c>laneIndex = j mod lanes.Count</c>,
/// and <c>visit = j / lanes.Count</c>.
/// </item>
/// </list>
/// </para>
/// </remarks>
public sealed class PatternCFCoefficientGenerator : CFCoefficientGenerator
{
    private readonly BigInteger[] _preperiod;
    private readonly Lane[] _lanes;

    /// <summary>
    /// Constructs a pattern generator with the given pre-period and lane
    /// cycle.
    /// </summary>
    /// <param name="preperiod">
    /// A finite sequence of explicit coefficients emitted once, in
    /// order. May be empty (in which case lane emission begins at index
    /// 0). The first element (if any) corresponds to the integer part
    /// <c>a₀</c> and may be any integer; every subsequent element must
    /// be a valid partial quotient (<c>≥ 1</c>).
    /// </param>
    /// <param name="lanes">
    /// Zero or more <see cref="Lane"/>s, cycled indefinitely after the
    /// pre-period is exhausted. An empty list represents a finite
    /// (rational) CF whose entire content is the pre-period.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="preperiod"/> or <paramref name="lanes"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Both <paramref name="preperiod"/> and <paramref name="lanes"/>
    /// are empty (a CF must have at least one coefficient), or a
    /// non-leading pre-period element is not a valid partial quotient.
    /// </exception>
    public PatternCFCoefficientGenerator(
        IReadOnlyList<BigInteger> preperiod,
        IReadOnlyList<Lane> lanes)
    {
        ArgumentNullException.ThrowIfNull(preperiod);
        ArgumentNullException.ThrowIfNull(lanes);

        if (preperiod.Count == 0 && lanes.Count == 0)
        {
            throw new ArgumentException(
                "A CF must have at least one coefficient — either the " +
                "pre-period or the lane cycle must be non-empty.",
                nameof(lanes));
        }

        // Pre-period[0] (the integer part) may be any sign; subsequent
        // entries must be valid partial quotients (≥ 1).
        for (var i = 1; i < preperiod.Count; i++)
        {
            if (preperiod[i].Sign <= 0)
            {
                throw new ArgumentException(
                    $"Pre-period entry at index {i.ToString(CultureInfo.InvariantCulture)} " +
                    $"({preperiod[i].ToString(CultureInfo.InvariantCulture)}) " +
                    "must be a valid partial quotient (≥ 1).",
                    nameof(preperiod));
            }
        }

        _preperiod = [.. preperiod];
        _lanes = [.. lanes];
    }

    /// <summary>
    /// Convenience constructor accepting <paramref name="preperiod"/> and
    /// <paramref name="lanes"/> as <see langword="params"/> arrays.
    /// </summary>
    public PatternCFCoefficientGenerator(BigInteger[] preperiod, params Lane[] lanes)
        : this(
            (IReadOnlyList<BigInteger>)(preperiod ?? throw new ArgumentNullException(nameof(preperiod))),
            (IReadOnlyList<Lane>)(lanes ?? throw new ArgumentNullException(nameof(lanes))))
    {
    }

    /// <summary>
    /// The fixed leading sequence of coefficients, emitted once before
    /// the lane cycle begins. May be empty.
    /// </summary>
    public IReadOnlyList<BigInteger> Preperiod => _preperiod;

    /// <summary>The cycling lanes that produce coefficients past the pre-period.</summary>
    public IReadOnlyList<Lane> Lanes => _lanes;

    /// <summary>
    /// <see langword="true"/> when the lane cycle is empty — the CF is
    /// finite and represents a rational number. The pre-period itself
    /// is the entire CF expansion.
    /// </summary>
    public bool IsRational => _lanes.Length == 0;

    /// <summary>
    /// <see langword="true"/> when the represented value is a quadratic
    /// irrational — equivalently, the lane cycle is non-empty and every
    /// lane is <see cref="Operation.Const"/> (so the periodic part
    /// genuinely repeats and, by Lagrange's theorem, the value is a
    /// quadratic irrational). Derived structurally from the constructor
    /// inputs in <c>O(lanes.Count)</c> time.
    /// </summary>
    public bool IsQuadraticIrrational
    {
        get
        {
            if (_lanes.Length == 0)
            {
                return false;
            }
            foreach (var lane in _lanes)
            {
                if (lane.Operation != Operation.Const)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <inheritdoc/>
    public override int? Length => _lanes.Length == 0 ? _preperiod.Length : null;

    /// <inheritdoc/>
    public override BigInteger this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            if (index < _preperiod.Length)
            {
                return _preperiod[index];
            }

            if (_lanes.Length == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    $"Index out of range for finite CF (length {_preperiod.Length.ToString(CultureInfo.InvariantCulture)}).");
            }

            var j = index - _preperiod.Length;
            var laneIndex = j % _lanes.Length;
            var visit = j / _lanes.Length;
            return _lanes[laneIndex].ValueAt(visit);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var pre = string.Join(
            ", ",
            _preperiod.Select(p => p.ToString(CultureInfo.InvariantCulture)));
        var cycle = string.Join(", ", _lanes.Select(l => l.ToString()));
        if (_lanes.Length == 0)
        {
            return $"[{pre}]";
        }
        return _preperiod.Length == 0
            ? $"[ ; {cycle}]"
            : $"[{pre}; {cycle}]";
    }
}
