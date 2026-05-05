using System.Globalization;
using System.Numerics;

namespace ContinuedFractions.Generators;

/// <summary>
/// A continued-fraction coefficient generator built from a pre-period
/// (a finite sequence of explicit coefficients emitted once, in order)
/// followed by a periodic part composed of <see cref="Lane"/>s that
/// cycle indefinitely. Each lane evolves its emitted value across
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
/// <b>Quadratic-irrational classification.</b> By Lagrange's theorem a
/// real number is a quadratic irrational iff its CF is eventually
/// periodic. For a <see cref="PatternCFCoefficientGenerator"/> the
/// periodicity question reduces to a structural check on the lanes:
/// every lane being <see cref="Operation.Const"/> ⇔ the periodic part
/// genuinely repeats ⇔ the value is a quadratic irrational. Any
/// non-<see cref="Operation.Const"/> lane breaks periodicity, so the
/// value cannot be a quadratic irrational. <see cref="IsQuadraticIrrational"/>
/// exposes this classification — derived purely from the constructor
/// inputs, no coefficient inspection required.
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
    /// At least one <see cref="Lane"/>, cycled indefinitely after the
    /// pre-period is exhausted.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="preperiod"/> or <paramref name="lanes"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="lanes"/> is empty, or a non-leading pre-period
    /// element is not a valid partial quotient.
    /// </exception>
    public PatternCFCoefficientGenerator(
        IReadOnlyList<BigInteger> preperiod,
        IReadOnlyList<Lane> lanes)
    {
        ArgumentNullException.ThrowIfNull(preperiod);
        ArgumentNullException.ThrowIfNull(lanes);

        if (lanes.Count == 0)
        {
            throw new ArgumentException(
                "At least one lane is required (the periodic part).",
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
    /// <see langword="true"/> when the represented value is a quadratic
    /// irrational — equivalently, when every lane is
    /// <see cref="Operation.Const"/> (so the periodic part genuinely
    /// repeats and the CF is eventually periodic). Derived structurally
    /// from the constructor inputs in <c>O(lanes.Count)</c> time.
    /// </summary>
    public bool IsQuadraticIrrational
    {
        get
        {
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
    public override BigInteger this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            if (index < _preperiod.Length)
            {
                return _preperiod[index];
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
        return _preperiod.Length == 0
            ? $"[ ; {cycle}]"
            : $"[{pre}; {cycle}]";
    }
}
