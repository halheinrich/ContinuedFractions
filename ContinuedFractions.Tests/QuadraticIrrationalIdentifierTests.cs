using System.Numerics;
using HalHeinrich.Numerics.ContinuedFractions.Generators;
using Xunit.Abstractions;

namespace HalHeinrich.Numerics.ContinuedFractions.Tests;

public class QuadraticIrrationalIdentifierTests
{
    private readonly ITestOutputHelper _output;

    public QuadraticIrrationalIdentifierTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ---------- construction ----------

    [Fact]
    public void Constructor_RejectsNonPositiveMaxTriples()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new QuadraticIrrationalIdentifier(maxTriples: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new QuadraticIrrationalIdentifier(maxTriples: -1));
    }

    [Fact]
    public void Constructor_RejectsNonPositiveComparisonDepth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new QuadraticIrrationalIdentifier(maxComparisonDepth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new QuadraticIrrationalIdentifier(maxComparisonDepth: -1));
    }

    // ---------- positive identifications: square roots ----------

    [Fact]
    public void TryIdentify_RecognisesSqrt2AsString()
    {
        var cf = new ContinuedFraction(Patterns.Sqrt2());
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("√2", result.Identification);
    }

    [Fact]
    public void TryIdentifyQuadratic_RecognisesSqrt2AsTriple()
    {
        var cf = new ContinuedFraction(Patterns.Sqrt2());
        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.NotNull(result.Value);
        Assert.Equal(new QuadraticIrrational(2, 0, 1), result.Value);
    }

    [Fact]
    public void TryIdentifyQuadratic_RecognisesSqrt5()
    {
        // CF of √5 = [2; 4, 4, 4, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 2 },
                new[] { Lane.Const(4) }));

        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(5, 0, 1), result.Value);
    }

    [Fact]
    public void TryIdentifyQuadratic_RecognisesSqrt7()
    {
        // CF of √7 = [2; 1, 1, 1, 4, …] (period 4)
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 2 },
                new[] { Lane.Const(1), Lane.Const(1), Lane.Const(1), Lane.Const(4) }));

        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(7, 0, 1), result.Value);
    }

    // ---------- positive identifications: general quadratic irrationals ----------

    [Fact]
    public void TryIdentifyQuadratic_RecognisesSqrt7Minus2Over3()
    {
        // (√7 − 2)/3 = [0; 4, 1, 1, 1, 4, 1, 1, 1, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0 },
                new[] { Lane.Const(4), Lane.Const(1), Lane.Const(1), Lane.Const(1) }));

        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.NotNull(result.Value);
        Assert.Equal(new QuadraticIrrational(7, 2, 3), result.Value);
    }

    [Fact]
    public void TryIdentify_GeneralForm_StringContainsCanonicalRendering()
    {
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0 },
                new[] { Lane.Const(4), Lane.Const(1), Lane.Const(1), Lane.Const(1) }));

        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("(√7 − 2)/3", result.Identification);
    }

    // ---------- negative identifications ----------

    [Fact]
    public void TryIdentifyQuadratic_RecognisesPhi()
    {
        // φ = (1 + √5)/2 = (√5 − (−1))/2 — canonical form (d=5, p=−1, q=2).
        var cf = new ContinuedFraction(Patterns.Phi());
        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(5, -1, 2), result.Value);
    }

    [Fact]
    public void TryIdentify_RecognisesPhiAsString()
    {
        var cf = new ContinuedFraction(Patterns.Phi());
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("(√5 + 1)/2", result.Identification);
    }

    [Fact]
    public void TryIdentify_RejectsEulersNumber()
    {
        // e is transcendental, not quadratic.
        var cf = new ContinuedFraction(Patterns.EulersNumber());
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_RejectsFiniteRational()
    {
        // 22/7 — rational, not a quadratic irrational.
        var cf = new ContinuedFraction(Patterns.Rational(22, 7));
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_RejectsBareInteger()
    {
        // Length-1 CF — the candidate's preperiod+period exceeds 1, so
        // any quadratic-irrational candidate fails the input-length check.
        var cf = new ContinuedFraction(Patterns.Rational(5, 1));
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
    }

    // ---------- null arg ----------

    [Fact]
    public void TryIdentifyQuadratic_RejectsNullCf()
    {
        var identifier = new QuadraticIrrationalIdentifier();
        Assert.Throws<ArgumentNullException>(() =>
            identifier.TryIdentifyQuadratic(null!));
    }

    // ---------- budget behaviour ----------

    private static ContinuedFraction Sqrt11Cf() =>
        // CF of √11 = [3; 3, 6, 3, 6, …]
        new(new PatternCFCoefficientGenerator(
            new BigInteger[] { 3 },
            new[] { Lane.Const(3), Lane.Const(6) }));

    [Fact]
    public void TryIdentify_TightTripleBudgetMissesLargerD()
    {
        // √11 needs the (11, 0, 1) triple, which sits at the start of
        // level 11's square-root sweep — position ~1111 in the
        // enumeration. With maxTriples = 5 we cover only level 1 and
        // most of level 2; far short of level 11.
        var result = new QuadraticIrrationalIdentifier(maxTriples: 5).TryIdentify(Sqrt11Cf());

        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_AdequateBudgetFindsLargerD()
    {
        // Default budget covers up through level ~21, so √11
        // (at position ~1111) is well within reach.
        var result = new QuadraticIrrationalIdentifier()
            .TryIdentifyQuadratic(Sqrt11Cf());

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(11, 0, 1), result.Value);
    }

    // ---------- sweep across constant CFs ----------

    [Fact]
    public void TryIdentify_ConstantCfs_AcrossN_1_to_100()
    {
        // [n; n, n, …] satisfies α = n + 1/α, so α = (n + √(n²+4))/2.
        // Even n = 2k simplifies to k + √(k²+1)  — canonical (k²+1, −k, 1).
        // Odd n stays at (n²+4, −n, 2). The identifier finds the smallest
        // canonical form first; values whose canonical level exceeds the
        // default 20 000-triple budget remain unsolved.
        var identifier = new QuadraticIrrationalIdentifier();
        var solved = new List<(int N, string Identification)>();
        var unsolved = new List<int>();

        for (var n = 1; n <= 100; n++)
        {
            var cf = new ContinuedFraction(
                new PatternCFCoefficientGenerator(
                    Array.Empty<BigInteger>(),
                    new[] { Lane.Const(n) }));
            var result = identifier.TryIdentify(cf);
            if (result.Match)
            {
                solved.Add((n, result.Identification!));
            }
            else
            {
                unsolved.Add(n);
            }
        }

        _output.WriteLine("CF [n; n, n, …] identified at default budget:");
        _output.WriteLine(string.Empty);
        foreach (var (n, ident) in solved)
        {
            _output.WriteLine($"  n = {n,3}  →  {ident}");
        }
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"Solved {solved.Count}/100. Unsolved n = [{string.Join(", ", unsolved)}].");

        Assert.Contains((1, "(√5 + 1)/2"), solved);   // φ
        Assert.Contains((2, "√2 + 1"), solved);       // 1 + √2
        Assert.Contains((3, "(√13 + 3)/2"), solved);
        Assert.Contains((4, "√5 + 2"), solved);
        Assert.Contains((6, "√10 + 3"), solved);
        Assert.Contains((8, "√17 + 4"), solved);
    }
}
