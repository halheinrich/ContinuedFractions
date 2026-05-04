using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions.Tests;

public class SquareRootIdentifierTests
{
    // ---------- construction ----------

    [Fact]
    public void Constructor_RejectsMaxDepthBelowStabilityRequirement()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SquareRootIdentifier(maxDepth: SquareRootIdentifier.RequiredStableDepth - 1));
    }

    [Fact]
    public void Constructor_AcceptsMaxDepthEqualToStabilityRequirement()
    {
        _ = new SquareRootIdentifier(maxDepth: SquareRootIdentifier.RequiredStableDepth);
    }

    // ---------- positive identifications ----------

    [Fact]
    public void TryIdentify_RecognisesSqrt2()
    {
        var cf = new ContinuedFraction(new Sqrt2());
        var result = new SquareRootIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("√2", result.Identification);
    }

    [Fact]
    public void TryIdentify_RecognisesSqrt3()
    {
        // CF of √3 = [1; 1, 2, 1, 2, 1, 2, ...]
        var sqrt3Generator = new FuncCFCoefficientGenerator(
            "√3",
            i => i == 0
                ? BigInteger.One
                : (i % 2 == 1 ? BigInteger.One : new BigInteger(2)));
        var cf = new ContinuedFraction(sqrt3Generator);

        var result = new SquareRootIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("√3", result.Identification);
    }

    [Fact]
    public void TryIdentify_RecognisesSqrt5()
    {
        // CF of √5 = [2; 4, 4, 4, ...]
        var sqrt5Generator = new FuncCFCoefficientGenerator(
            "√5",
            i => i == 0 ? new BigInteger(2) : new BigInteger(4));
        var cf = new ContinuedFraction(sqrt5Generator);

        var result = new SquareRootIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("√5", result.Identification);
    }

    [Fact]
    public void TryIdentify_RecognisesSqrt7()
    {
        // CF of √7 = [2; 1, 1, 1, 4, 1, 1, 1, 4, ...] (period 4)
        var sqrt7Generator = new FuncCFCoefficientGenerator(
            "√7",
            i =>
            {
                if (i == 0)
                {
                    return new BigInteger(2);
                }
                // Period: 1, 1, 1, 4 starting at i=1
                return ((i - 1) % 4) == 3 ? new BigInteger(4) : BigInteger.One;
            });
        var cf = new ContinuedFraction(sqrt7Generator);

        var result = new SquareRootIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("√7", result.Identification);
    }

    // ---------- negative identifications ----------

    [Fact]
    public void TryIdentify_RejectsPhi()
    {
        var cf = new ContinuedFraction(new Phi());
        var result = new SquareRootIdentifier().TryIdentify(cf);
        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_RejectsEulersNumber()
    {
        var cf = new ContinuedFraction(new EulersNumber());
        var result = new SquareRootIdentifier().TryIdentify(cf);
        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_RejectsFiniteRational()
    {
        // [3; 7] = 22/7 — a rational, not a square root.
        var cf = new ContinuedFraction(3, 7);
        var result = new SquareRootIdentifier().TryIdentify(cf);
        Assert.False(result.Match);
    }

    [Fact]
    public void TryIdentify_RejectsBareInteger()
    {
        // Length-1 CF — single convergent can't establish stability.
        var cf = new ContinuedFraction(5);
        var result = new SquareRootIdentifier().TryIdentify(cf);
        Assert.False(result.Match);
    }

    // ---------- budget exhaustion ----------

    [Fact]
    public void TryIdentify_NotMatchedReturnsBudgetDepth_WhenBudgetExhaustedByUnboundedCf()
    {
        var cf = new ContinuedFraction(new Phi());
        var result = new SquareRootIdentifier(maxDepth: 10).TryIdentify(cf);

        Assert.False(result.Match);
        Assert.Equal(10, result.Depth);
    }

    [Fact]
    public void TryIdentify_NotMatchedReturnsLastDepth_WhenFiniteCfExhausts()
    {
        var cf = new ContinuedFraction(3, 7);
        var result = new SquareRootIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
        Assert.Equal(1, result.Depth);  // last convergent index for length-2 CF
    }
}
