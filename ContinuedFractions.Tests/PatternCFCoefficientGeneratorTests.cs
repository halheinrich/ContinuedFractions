using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions.Tests;

public class PatternCFCoefficientGeneratorTests
{
    // ---------- construction: validation ----------

    [Fact]
    public void Constructor_RejectsNullPreperiod()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PatternCFCoefficientGenerator(
                (IReadOnlyList<BigInteger>)null!,
                new[] { Lane.Const(1) }));
    }

    [Fact]
    public void Constructor_RejectsNullLanes()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PatternCFCoefficientGenerator(
                Array.Empty<BigInteger>(),
                (IReadOnlyList<Lane>)null!));
    }

    [Fact]
    public void Constructor_RejectsEmptyLanes()
    {
        Assert.Throws<ArgumentException>(() =>
            new PatternCFCoefficientGenerator(
                Array.Empty<BigInteger>(),
                Array.Empty<Lane>()));
    }

    [Fact]
    public void Constructor_AcceptsEmptyPreperiod()
    {
        // No pre-period — first lane visit produces a₀.
        var gen = new PatternCFCoefficientGenerator(
            Array.Empty<BigInteger>(),
            new[] { Lane.Const(1) });

        Assert.Empty(gen.Preperiod);
        Assert.Single(gen.Lanes);
    }

    [Fact]
    public void Constructor_AcceptsAnyA0InPreperiod()
    {
        // Pre-period[0] is a₀, may be any sign (e.g. negative integer parts).
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { -2 },
            new[] { Lane.Const(2) });

        Assert.Equal(new BigInteger(-2), gen.Preperiod[0]);
    }

    [Fact]
    public void Constructor_RejectsNonPositivePreperiodEntryAfterA0()
    {
        // preperiod[1] = 0 is not a valid partial quotient.
        Assert.Throws<ArgumentException>(() =>
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 1, 0 },
                new[] { Lane.Const(1) }));
    }

    [Fact]
    public void Constructor_RejectsNegativePreperiodEntryAfterA0()
    {
        Assert.Throws<ArgumentException>(() =>
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 1, -1 },
                new[] { Lane.Const(1) }));
    }

    // ---------- IsQuadraticIrrational ----------

    [Fact]
    public void IsQuadraticIrrational_TrueWhenAllLanesAreConst()
    {
        // φ = [1; 1, 1, 1, …] — single Const lane.
        var gen = new PatternCFCoefficientGenerator(
            Array.Empty<BigInteger>(),
            new[] { Lane.Const(1) });

        Assert.True(gen.IsQuadraticIrrational);
    }

    [Fact]
    public void IsQuadraticIrrational_TrueForMultiConstLanes()
    {
        // √7 = [2; 1, 1, 1, 4, 1, 1, 1, 4, …] — preperiod [2], period [1, 1, 1, 4].
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { 2 },
            new[] { Lane.Const(1), Lane.Const(1), Lane.Const(1), Lane.Const(4) });

        Assert.True(gen.IsQuadraticIrrational);
    }

    [Fact]
    public void IsQuadraticIrrational_FalseWhenAnyLaneIsPlus()
    {
        // e = [2; 1, 2, 1, 1, 4, 1, 1, 6, …] — middle lane is Plus.
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { 2 },
            new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) });

        Assert.False(gen.IsQuadraticIrrational);
    }

    [Fact]
    public void IsQuadraticIrrational_FalseWhenAnyLaneIsMultiply()
    {
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { 2 },
            new[] { Lane.Multiply(1, 2) });

        Assert.False(gen.IsQuadraticIrrational);
    }

    // ---------- indexing ----------

    [Fact]
    public void Indexer_ReturnsPreperiodValuesFirst()
    {
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { 7, 3, 5 },
            new[] { Lane.Const(2) });

        Assert.Equal(new BigInteger(7), gen[0]);
        Assert.Equal(new BigInteger(3), gen[1]);
        Assert.Equal(new BigInteger(5), gen[2]);
    }

    [Fact]
    public void Indexer_CyclesThroughLanesAfterPreperiod()
    {
        // Pre-period [2], lanes [Const(1), Const(4)] — produces [2; 1, 4, 1, 4, 1, 4, …].
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { 2 },
            new[] { Lane.Const(1), Lane.Const(4) });

        Assert.Equal(new BigInteger(2), gen[0]);
        Assert.Equal(new BigInteger(1), gen[1]);
        Assert.Equal(new BigInteger(4), gen[2]);
        Assert.Equal(new BigInteger(1), gen[3]);
        Assert.Equal(new BigInteger(4), gen[4]);
        Assert.Equal(new BigInteger(1), gen[5]);
    }

    [Fact]
    public void Indexer_EvolvesLanesAcrossVisits_ForEulersNumber()
    {
        // e = [2; 1, 2, 1, 1, 4, 1, 1, 6, 1, 1, 8, 1, 1, 10, …].
        // Preperiod [2], lanes [Const(1), Plus(2, +2), Const(1)].
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { 2 },
            new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) });

        var expected = new BigInteger[]
        {
            2,                         // preperiod
            1, 2, 1,                   // lanes visit 0
            1, 4, 1,                   // lanes visit 1
            1, 6, 1,                   // lanes visit 2
            1, 8, 1,                   // lanes visit 3
            1, 10, 1,                  // lanes visit 4
        };

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], gen[i]);
        }
    }

    [Fact]
    public void Indexer_HandlesMultiplyLane()
    {
        // [2; 1, 2, 4, 8, 16, 32, …] — single Multiply lane after one
        // pre-period entry.
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { 2 },
            new[] { Lane.Multiply(1, 2) });

        var expected = new BigInteger[] { 2, 1, 2, 4, 8, 16, 32, 64 };
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], gen[i]);
        }
    }

    [Fact]
    public void Indexer_RejectsNegativeIndex()
    {
        var gen = new PatternCFCoefficientGenerator(
            Array.Empty<BigInteger>(),
            new[] { Lane.Const(1) });

        Assert.Throws<ArgumentOutOfRangeException>(() => gen[-1]);
    }

    [Fact]
    public void Length_IsNullForUnboundedGenerator()
    {
        var gen = new PatternCFCoefficientGenerator(
            Array.Empty<BigInteger>(),
            new[] { Lane.Const(1) });

        Assert.Null(gen.Length);
    }

    // ---------- integration with ContinuedFraction ----------

    [Fact]
    public void Integration_ConstantPattern_GivesQuadraticIrrationalCf()
    {
        // [3; 3, 3, …] = (3 + √13)/2.
        var gen = new PatternCFCoefficientGenerator(
            Array.Empty<BigInteger>(),
            new[] { Lane.Const(3) });
        var cf = new ContinuedFraction(gen);

        var result = new QuadraticIrrationalIdentifier().TryIdentifyQuadratic(cf);

        Assert.True(result.Match);
        Assert.Equal(new QuadraticIrrational(13, -3, 2), result.Value);
    }

    [Fact]
    public void Integration_EulersNumberPattern_NotIdentifiedAsQuadratic()
    {
        // e is transcendental — the quadratic identifier should give up.
        var gen = new PatternCFCoefficientGenerator(
            new BigInteger[] { 2 },
            new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) });
        var cf = new ContinuedFraction(gen);

        var result = new QuadraticIrrationalIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
    }

    [Fact]
    public void Integration_EulersNumberPattern_MatchesEulersNumberGenerator()
    {
        // The pattern reproduces the same coefficients as the dedicated
        // EulersNumber generator for at least the first 24 terms.
        var pattern = new PatternCFCoefficientGenerator(
            new BigInteger[] { 2 },
            new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) });
        var canonical = new EulersNumber();

        for (var i = 0; i < 24; i++)
        {
            Assert.Equal(canonical[i], pattern[i]);
        }
    }
}
