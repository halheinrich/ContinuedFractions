using System.Numerics;
using ContinuedFractions.Generators;
using HalHeinrich.Numerics;

namespace ContinuedFractions.Tests;

public class ContinuedFractionConvergentTests
{
    // ---------- helpers ----------

    private static ContinuedFraction Finite(params BigInteger[] coeffs) =>
        new(new PatternCFCoefficientGenerator(coeffs, Array.Empty<Lane>()));

    private static ContinuedFraction Periodic(BigInteger[] preperiod, params Lane[] lanes) =>
        new(new PatternCFCoefficientGenerator(preperiod, lanes));

    // ---------- finite CFs (known rationals) ----------

    [Fact]
    public void Indexer_Depth0_OnIntegerCF_ReturnsInteger()
    {
        var cf = Finite(7);
        Assert.Equal(new BigRational(7, 1), cf[0]);
    }

    [Fact]
    public void Indexer_Depth0_OnNegativeIntegerCF_ReturnsNegative()
    {
        var cf = Finite(-3);
        Assert.Equal(new BigRational(-3, 1), cf[0]);
    }

    [Fact]
    public void Indexer_Depth1_OnTwoCoefficientCF_ReturnsExactRational()
    {
        // [3; 7] = 22/7
        var cf = Finite(3, 7);
        Assert.Equal(new BigRational(22, 7), cf[1]);
    }

    [Fact]
    public void Indexer_FollowsConvergentRecurrence()
    {
        // [1; 2, 3] convergents: 1/1, 3/2, 10/7
        var cf = Finite(1, 2, 3);
        Assert.Equal(new BigRational(1, 1), cf[0]);
        Assert.Equal(new BigRational(3, 2), cf[1]);
        Assert.Equal(new BigRational(10, 7), cf[2]);
    }

    [Fact]
    public void Indexer_NegativeIntegerPart_RecurrenceCorrect()
    {
        // [-2; 1, 4] = -6/5
        var cf = Finite(-2, 1, 4);
        Assert.Equal(new BigRational(-6, 5), cf[2]);
    }

    // ---------- φ (golden ratio): [1; 1, 1, 1, ...] ----------

    [Fact]
    public void Phi_ConvergentsAreFibonacciRatios()
    {
        var phi = new ContinuedFraction(Patterns.Phi());

        Assert.Equal(new BigRational(1, 1), phi[0]);
        Assert.Equal(new BigRational(2, 1), phi[1]);
        Assert.Equal(new BigRational(3, 2), phi[2]);
        Assert.Equal(new BigRational(5, 3), phi[3]);
        Assert.Equal(new BigRational(8, 5), phi[4]);
        Assert.Equal(new BigRational(13, 8), phi[5]);
        Assert.Equal(new BigRational(21, 13), phi[6]);
    }

    // ---------- √2: [1; 2, 2, 2, ...] ----------

    [Fact]
    public void Sqrt2_ConvergentsApproximateRoot2()
    {
        var sqrt2 = new ContinuedFraction(Patterns.Sqrt2());

        Assert.Equal(new BigRational(1, 1), sqrt2[0]);
        Assert.Equal(new BigRational(3, 2), sqrt2[1]);
        Assert.Equal(new BigRational(7, 5), sqrt2[2]);
        Assert.Equal(new BigRational(17, 12), sqrt2[3]);
        Assert.Equal(new BigRational(41, 29), sqrt2[4]);
        Assert.Equal(new BigRational(99, 70), sqrt2[5]);
    }

    // ---------- iteration ----------

    [Fact]
    public void Iteration_FiniteCF_YieldsAllConvergentsThenTerminates()
    {
        var cf = Finite(1, 2, 3);
        var convergents = cf.ToList();

        Assert.Equal(3, convergents.Count);
        Assert.Equal(new BigRational(1, 1), convergents[0]);
        Assert.Equal(new BigRational(3, 2), convergents[1]);
        Assert.Equal(new BigRational(10, 7), convergents[2]);
    }

    [Fact]
    public void Iteration_UnboundedCF_YieldsIndefinitelyUntilConsumerStops()
    {
        var phi = new ContinuedFraction(Patterns.Phi());

        var first6 = phi.Take(6).ToList();
        Assert.Equal(6, first6.Count);
        Assert.Equal(new BigRational(13, 8), first6[5]);
    }

    [Fact]
    public void Iteration_IntegerCF_YieldsExactlyOne()
    {
        var cf = Finite(7);
        var convergents = cf.ToList();

        Assert.Single(convergents);
        Assert.Equal(new BigRational(7, 1), convergents[0]);
    }

    // ---------- memoization ----------

    [Fact]
    public void Memoization_RepeatedAccessReturnsSameValue()
    {
        var cf = Finite(1, 2, 3);
        Assert.Equal(cf[2], cf[2]);
        Assert.Equal(cf[1], cf[1]);
    }

    [Fact]
    public void Memoization_OnlyComputesEachConvergentOnce()
    {
        // The convergent cache is internal, so we observe it indirectly
        // by checking that repeated access at the same depth yields the
        // same value (same instance is not contractually guaranteed, but
        // the value must agree).
        var cf = Periodic(Array.Empty<BigInteger>(), Lane.Const(2));

        var firstAt5 = cf[5];
        var againAt5 = cf[5];
        Assert.Equal(firstAt5, againAt5);

        // Accessing a smaller depth after a larger one stays consistent.
        var atZero = cf[0];
        Assert.Equal(new BigRational(2, 1), atZero);
    }

    // ---------- indexer validation ----------

    [Fact]
    public void Indexer_RejectsNegativeDepth()
    {
        var cf = Finite(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => cf[-1]);
    }

    [Fact]
    public void Indexer_RejectsDepthAtFiniteLength()
    {
        var cf = Finite(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => cf[3]);
    }

    [Fact]
    public void Indexer_RejectsDepthBeyondFiniteLength()
    {
        var cf = Finite(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => cf[100]);
    }

    [Fact]
    public void Indexer_AcceptsHighDepthOnUnboundedCF()
    {
        var phi = new ContinuedFraction(Patterns.Phi());
        _ = phi[100];   // just don't throw
    }

    [Fact]
    public void Iteration_NegativeIntegerPartIsAllowed()
    {
        // a_0 may be negative — the partial-quotient invariant only
        // applies to a_i for i ≥ 1.
        var cf = Periodic(new BigInteger[] { -5 }, Lane.Const(1));
        Assert.Equal(new BigRational(-5, 1), cf[0]);
        Assert.Equal(new BigRational(-4, 1), cf[1]);
    }
}
