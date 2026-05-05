using System.Globalization;
using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions;

/// <summary>
/// Identifies a continued fraction as a member of one of the patterned
/// e-family shapes — <c>tanh(1/m)</c>, <c>coth(1/m)</c>, <c>e + n</c>,
/// or <c>1/(e + n)</c> — by recognising the shape in the input's
/// coefficients and recovering the integer parameter.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="EFamilyCatalogueIdentifier"/>, which compares
/// against a fixed list of named values, this identifier is
/// parameterised: it catches every <c>tanh(1/m)</c> for any positive
/// integer <c>m</c>, every <c>coth(1/m)</c>, and every
/// integer-shifted <c>e</c>.
/// </para>
/// <para>
/// Recognisers tried, in order:
/// <list type="number">
/// <item>
/// <c>tanh(1/m)</c> — CF shape <c>[0; m, 3m, 5m, 7m, …]</c>; recover
/// <c>m</c> from <c>a₁</c>.
/// </item>
/// <item>
/// <c>coth(1/m)</c> — CF shape <c>[m; 3m, 5m, 7m, …]</c>; recover
/// <c>m</c> from <c>a₀</c>.
/// </item>
/// <item>
/// <c>e + n</c> — CF shape <c>[k; 1, 2, 1, 1, 4, 1, 1, 6, …]</c>
/// (Euler three-lane shape after a single integer-part entry); recover
/// <c>n = k − 2</c> so the value is <c>e + n</c>.
/// </item>
/// <item>
/// <c>1/(e + n)</c> — CF shape <c>[0, k; 1, 2, 1, 1, 4, 1, 1, 6, …]</c>
/// (Euler three-lane shape after the two-element preperiod
/// <c>[0, k]</c>); recover <c>n = k − 2</c>.
/// </item>
/// </list>
/// </para>
/// <para>
/// Each recogniser verifies the full shape up to
/// <see cref="DefaultMaxComparisonDepth"/> coefficients before declaring
/// a match — a single mismatched coefficient invalidates the candidate.
/// </para>
/// </remarks>
public sealed class EFamilyShapeIdentifier : CFIdentifier
{
    /// <summary>
    /// Default cap on coefficients verified per shape recogniser.
    /// </summary>
    public const int DefaultMaxComparisonDepth = 64;

    private readonly int _maxComparisonDepth;

    /// <summary>
    /// Constructs the identifier with the given verification budget.
    /// </summary>
    /// <param name="maxComparisonDepth">
    /// Maximum coefficient-comparison depth per recogniser. Must be
    /// strictly positive.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxComparisonDepth"/> is non-positive.
    /// </exception>
    public EFamilyShapeIdentifier(int maxComparisonDepth = DefaultMaxComparisonDepth)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxComparisonDepth);
        _maxComparisonDepth = maxComparisonDepth;
    }

    /// <inheritdoc/>
    protected override IdentificationResult TryIdentifyCore(ContinuedFraction cf)
    {
        var generator = cf.Generator;

        // Finite CFs are rational — no shape recogniser can match them.
        if (generator.Length is int len && len < _maxComparisonDepth)
        {
            return IdentificationResult.NotMatched(Math.Max(len - 1, 0));
        }

        if (TryTanh(generator) is { } tanhMatch)
        {
            return tanhMatch;
        }
        if (TryCoth(generator) is { } cothMatch)
        {
            return cothMatch;
        }
        if (TryEulerShifted(generator) is { } eulerMatch)
        {
            return eulerMatch;
        }
        if (TryReciprocalEulerShifted(generator) is { } reciprocalMatch)
        {
            return reciprocalMatch;
        }

        return IdentificationResult.NotMatched(_maxComparisonDepth - 1);
    }

    /// <summary>
    /// Recognises <c>tanh(1/m)</c>: CF <c>[0; m, 3m, 5m, 7m, …]</c>
    /// with positive integer <c>m</c>.
    /// </summary>
    private IdentificationResult? TryTanh(CFCoefficientGenerator gen)
    {
        if (!gen[0].IsZero)
        {
            return null;
        }
        var m = gen[1];
        if (m.Sign <= 0)
        {
            return null;
        }

        for (var i = 1; i < _maxComparisonDepth; i++)
        {
            var expected = new BigInteger(2 * i - 1) * m;
            if (gen[i] != expected)
            {
                return null;
            }
        }

        return IdentificationResult.Matched(
            $"tanh(1/{m.ToString(CultureInfo.InvariantCulture)})",
            _maxComparisonDepth - 1);
    }

    /// <summary>
    /// Recognises <c>coth(1/m)</c>: CF <c>[m; 3m, 5m, 7m, 9m, …]</c>
    /// with positive integer <c>m</c>.
    /// </summary>
    private IdentificationResult? TryCoth(CFCoefficientGenerator gen)
    {
        var m = gen[0];
        if (m.Sign <= 0)
        {
            return null;
        }

        for (var i = 1; i < _maxComparisonDepth; i++)
        {
            var expected = new BigInteger(2 * i + 1) * m;
            if (gen[i] != expected)
            {
                return null;
            }
        }

        return IdentificationResult.Matched(
            $"coth(1/{m.ToString(CultureInfo.InvariantCulture)})",
            _maxComparisonDepth - 1);
    }

    /// <summary>
    /// Recognises <c>e + (k − 2)</c>: CF
    /// <c>[k; 1, 2, 1, 1, 4, 1, 1, 6, …]</c> — Euler three-lane shape
    /// after the integer-part entry. Outputs <c>"e"</c>, <c>"e + n"</c>,
    /// or <c>"e − n"</c>.
    /// </summary>
    private IdentificationResult? TryEulerShifted(CFCoefficientGenerator gen)
    {
        var k = gen[0];

        for (var i = 1; i < _maxComparisonDepth; i++)
        {
            if (gen[i] != EulerLaneAt(visit: (i - 1) / 3, lane: (i - 1) % 3))
            {
                return null;
            }
        }

        return IdentificationResult.Matched(
            FormatEPlus(k - new BigInteger(2)),
            _maxComparisonDepth - 1);
    }

    /// <summary>
    /// Recognises <c>1/(e + (k − 2))</c>: CF
    /// <c>[0, k; 1, 2, 1, 1, 4, 1, 1, 6, …]</c> — Euler three-lane shape
    /// after the two-element preperiod <c>[0, k]</c>. Outputs
    /// <c>"1/e"</c>, <c>"1/(e + n)"</c>, or <c>"1/(e − n)"</c>.
    /// </summary>
    private IdentificationResult? TryReciprocalEulerShifted(CFCoefficientGenerator gen)
    {
        if (!gen[0].IsZero)
        {
            return null;
        }
        var k = gen[1];
        if (k.Sign <= 0)
        {
            return null;
        }

        for (var i = 2; i < _maxComparisonDepth; i++)
        {
            if (gen[i] != EulerLaneAt(visit: (i - 2) / 3, lane: (i - 2) % 3))
            {
                return null;
            }
        }

        return IdentificationResult.Matched(
            FormatReciprocalE(k - new BigInteger(2)),
            _maxComparisonDepth - 1);
    }

    /// <summary>
    /// Expected value at lane index <paramref name="lane"/> on visit
    /// <paramref name="visit"/> for the Euler three-lane shape
    /// <c>[Const(1), Plus(2, +2), Const(1)]</c>.
    /// </summary>
    private static BigInteger EulerLaneAt(int visit, int lane) => lane switch
    {
        0 => BigInteger.One,
        1 => new BigInteger(2 + 2 * visit),
        2 => BigInteger.One,
        _ => throw new ArgumentOutOfRangeException(nameof(lane)),
    };

    /// <summary>
    /// Renders <c>e + n</c> as <c>"e"</c> when <paramref name="n"/> is
    /// zero, <c>"e + |n|"</c> when positive, <c>"e − |n|"</c> when
    /// negative.
    /// </summary>
    private static string FormatEPlus(BigInteger n)
    {
        if (n.IsZero)
        {
            return "e";
        }
        return n.Sign > 0
            ? $"e + {n.ToString(CultureInfo.InvariantCulture)}"
            : $"e − {(-n).ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Renders <c>1/(e + n)</c> as <c>"1/e"</c> when <paramref name="n"/>
    /// is zero, <c>"1/(e + |n|)"</c> when positive,
    /// <c>"1/(e − |n|)"</c> when negative.
    /// </summary>
    private static string FormatReciprocalE(BigInteger n)
    {
        if (n.IsZero)
        {
            return "1/e";
        }
        return n.Sign > 0
            ? $"1/(e + {n.ToString(CultureInfo.InvariantCulture)})"
            : $"1/(e − {(-n).ToString(CultureInfo.InvariantCulture)})";
    }
}
