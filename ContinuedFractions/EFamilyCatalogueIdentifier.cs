using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions;

/// <summary>
/// Identifies a continued fraction as one of a small fixed catalogue of
/// well-known transcendental values in the e-family — <c>e</c>,
/// <c>e − 1</c>, <c>1/e</c>, <c>(e − 1)/(e + 1)</c>, <c>(e + 1)/(e − 1)</c>
/// — by coefficient-by-coefficient comparison against the catalogue
/// entry's CF.
/// </summary>
/// <remarks>
/// <para>
/// The catalogue gives canonical names. <c>tanh(½)</c> and
/// <c>(e − 1)/(e + 1)</c> are the same value, but the catalogue
/// identifies it with the more recognisable algebraic form. For broader
/// family coverage (e.g. <c>tanh(1/3)</c>) use
/// <see cref="EFamilyShapeIdentifier"/> alongside this one — typically
/// catalogue first in the harness, shape second.
/// </para>
/// <para>
/// The comparison budget is shared between candidate iteration and
/// input iteration: the identifier reads up to
/// <see cref="DefaultMaxComparisonDepth"/> coefficients of the input and
/// the catalogue entry, declaring a match only when every position
/// agrees exactly.
/// </para>
/// </remarks>
public sealed class EFamilyCatalogueIdentifier : CFIdentifier
{
    /// <summary>
    /// Default cap on coefficients compared per catalogue entry.
    /// </summary>
    public const int DefaultMaxComparisonDepth = 64;

    private readonly int _maxComparisonDepth;
    private readonly (string Name, PatternCFCoefficientGenerator Pattern)[] _catalogue;

    /// <summary>
    /// Constructs the identifier with the given comparison budget.
    /// </summary>
    /// <param name="maxComparisonDepth">
    /// Maximum coefficient-comparison depth per catalogue entry. Must
    /// be strictly positive.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxComparisonDepth"/> is non-positive.
    /// </exception>
    public EFamilyCatalogueIdentifier(int maxComparisonDepth = DefaultMaxComparisonDepth)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxComparisonDepth);

        _maxComparisonDepth = maxComparisonDepth;
        _catalogue = BuildCatalogue();
    }

    /// <summary>
    /// The catalogue entries this identifier compares against, in the
    /// order they are tried.
    /// </summary>
    public IReadOnlyList<(string Name, PatternCFCoefficientGenerator Pattern)> Catalogue => _catalogue;

    /// <inheritdoc/>
    protected override IdentificationResult TryIdentifyCore(ContinuedFraction cf)
    {
        var maxIndexExamined = -1;

        foreach (var (name, pattern) in _catalogue)
        {
            var (matched, indexExamined) = CompareCoefficients(
                cf.Generator, pattern, _maxComparisonDepth);

            if (indexExamined > maxIndexExamined)
            {
                maxIndexExamined = indexExamined;
            }

            if (matched)
            {
                return IdentificationResult.Matched(name, indexExamined);
            }
        }

        return IdentificationResult.NotMatched(Math.Max(maxIndexExamined, 0));
    }

    /// <summary>
    /// Compares <paramref name="input"/> and <paramref name="candidate"/>
    /// coefficient-by-coefficient up to <paramref name="depth"/> positions.
    /// Stops early on the first mismatch or when either generator's
    /// finite length runs out.
    /// </summary>
    private static (bool Matched, int IndexExamined) CompareCoefficients(
        CFCoefficientGenerator input,
        CFCoefficientGenerator candidate,
        int depth)
    {
        var inputLength = input.Length;
        var candidateLength = candidate.Length;
        var lastIndex = -1;

        for (var i = 0; i < depth; i++)
        {
            if (inputLength is int inLen && i >= inLen)
            {
                return (false, lastIndex);
            }
            if (candidateLength is int candLen && i >= candLen)
            {
                return (false, lastIndex);
            }

            if (input[i] != candidate[i])
            {
                return (false, i);
            }
            lastIndex = i;
        }

        return (true, lastIndex);
    }

    private static (string Name, PatternCFCoefficientGenerator Pattern)[] BuildCatalogue()
    {
        // The Euler three-lane shape that drives e and its simple shifts
        // and reciprocals.
        Lane[] eulerLanes() =>
            new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) };

        return new (string, PatternCFCoefficientGenerator)[]
        {
            ("e",
                Patterns.EulersNumber()),
            ("e − 1",
                new PatternCFCoefficientGenerator(
                    new BigInteger[] { 1 },
                    eulerLanes())),
            ("1/e",
                new PatternCFCoefficientGenerator(
                    new BigInteger[] { 0, 2 },
                    eulerLanes())),
            ("(e − 1)/(e + 1)",
                new PatternCFCoefficientGenerator(
                    new BigInteger[] { 0 },
                    new[] { Lane.Plus(2, 4) })),
            ("(e + 1)/(e − 1)",
                new PatternCFCoefficientGenerator(
                    new BigInteger[] { 2 },
                    new[] { Lane.Plus(6, 4) })),
        };
    }
}
