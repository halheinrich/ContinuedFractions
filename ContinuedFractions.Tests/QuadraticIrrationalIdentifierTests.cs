using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions.Tests;

public class QuadraticIrrationalIdentifierTests
{
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
        var cf = new ContinuedFraction(new Sqrt2());
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("√2", result.Identification);
    }

    [Fact]
    public void TryIdentifyQuadratic_RecognisesSqrt2AsTriple()
    {
        var cf = new ContinuedFraction(new Sqrt2());
        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.NotNull(result.Value);
        Assert.Equal(new QuadraticIrrational(2, 0, 1), result.Value);
    }

    [Fact]
    public void TryIdentifyQuadratic_RecognisesSqrt5()
    {
        // CF of √5 = [2; 4, 4, 4, …]
        var sqrt5 = new FuncCFCoefficientGenerator(
            "√5", i => i == 0 ? new BigInteger(2) : new BigInteger(4));
        var cf = new ContinuedFraction(sqrt5);

        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(5, 0, 1), result.Value);
    }

    [Fact]
    public void TryIdentifyQuadratic_RecognisesSqrt7()
    {
        // CF of √7 = [2; 1, 1, 1, 4, …] (period 4)
        var sqrt7 = new FuncCFCoefficientGenerator(
            "√7",
            i => i == 0
                ? new BigInteger(2)
                : ((i - 1) % 4) == 3 ? new BigInteger(4) : BigInteger.One);
        var cf = new ContinuedFraction(sqrt7);

        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(7, 0, 1), result.Value);
    }

    // ---------- positive identifications: general quadratic irrationals ----------

    [Fact]
    public void TryIdentifyQuadratic_RecognisesSqrt7Minus2Over3()
    {
        // (√7 − 2)/3 = [0; 4, 1, 1, 1, 4, 1, 1, 1, …] (period 4 starting at index 1)
        var generator = new FuncCFCoefficientGenerator(
            "(√7 − 2)/3",
            i =>
            {
                if (i == 0)
                {
                    return BigInteger.Zero;
                }
                // Period [4, 1, 1, 1] starting at i = 1.
                return ((i - 1) % 4) == 0 ? new BigInteger(4) : BigInteger.One;
            });
        var cf = new ContinuedFraction(generator);

        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.NotNull(result.Value);
        Assert.Equal(new QuadraticIrrational(7, 2, 3), result.Value);
    }

    [Fact]
    public void TryIdentify_GeneralForm_StringContainsCanonicalRendering()
    {
        var generator = new FuncCFCoefficientGenerator(
            "(√7 − 2)/3",
            i =>
            {
                if (i == 0)
                {
                    return BigInteger.Zero;
                }
                return ((i - 1) % 4) == 0 ? new BigInteger(4) : BigInteger.One;
            });
        var cf = new ContinuedFraction(generator);

        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("(√7 − 2)/3", result.Identification);
    }

    // ---------- negative identifications ----------

    [Fact]
    public void TryIdentifyQuadratic_RecognisesPhi()
    {
        // φ = (1 + √5)/2 = (√5 − (−1))/2 — canonical form (d=5, p=−1, q=2).
        var cf = new ContinuedFraction(new Phi());
        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(5, -1, 2), result.Value);
    }

    [Fact]
    public void TryIdentify_RecognisesPhiAsString()
    {
        var cf = new ContinuedFraction(new Phi());
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("(√5 + 1)/2", result.Identification);
    }

    [Fact]
    public void TryIdentify_RejectsEulersNumber()
    {
        // e is transcendental, not quadratic.
        var cf = new ContinuedFraction(new EulersNumber());
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_RejectsFiniteRational()
    {
        // [3; 7] = 22/7 — rational, not a quadratic irrational.
        var cf = new ContinuedFraction(3, 7);
        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_RejectsBareInteger()
    {
        // Length-1 CF — the candidate's preperiod+period exceeds 1, so
        // any quadratic-irrational candidate fails the input-length check.
        var cf = new ContinuedFraction(5);
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

    [Fact]
    public void TryIdentify_TightTripleBudgetMissesLargerD()
    {
        // √11 needs the (11, 0, 1) triple, which sits at the start of
        // level 11's square-root sweep — position ~1111 in the
        // enumeration. With maxTriples = 5 we cover only level 1 and
        // most of level 2; far short of level 11.
        var sqrt11 = new FuncCFCoefficientGenerator(
            "√11",
            i => i == 0
                ? new BigInteger(3)
                : ((i - 1) % 2) == 0 ? new BigInteger(3) : new BigInteger(6));
        // CF of √11 = [3; 3, 6, 3, 6, …]
        var cf = new ContinuedFraction(sqrt11);

        var result = new QuadraticIrrationalIdentifier(maxTriples: 5).TryIdentify(cf);

        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_AdequateBudgetFindsLargerD()
    {
        var sqrt11 = new FuncCFCoefficientGenerator(
            "√11",
            i => i == 0
                ? new BigInteger(3)
                : ((i - 1) % 2) == 0 ? new BigInteger(3) : new BigInteger(6));
        var cf = new ContinuedFraction(sqrt11);

        // Default budget of 10000 covers up through level ~21, so √11
        // (at position ~1111) is well within reach.
        var result = new QuadraticIrrationalIdentifier()
            .TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(11, 0, 1), result.Value);
    }
}
