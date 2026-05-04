using System.Numerics;
using ContinuedFractions.Generators;
using HalHeinrich.Numerics;

namespace ContinuedFractions.Tests;

public class ContinuedFractionConvergentTests
{
    // ---------- finite CFs (known rationals) ----------

    [Fact]
    public void Indexer_Depth0_OnIntegerCF_ReturnsInteger()
    {
        var cf = new ContinuedFraction(7);
        Assert.Equal(new BigRational(7, 1), cf[0]);
    }

    [Fact]
    public void Indexer_Depth0_OnNegativeIntegerCF_ReturnsNegative()
    {
        var cf = new ContinuedFraction(-3);
        Assert.Equal(new BigRational(-3, 1), cf[0]);
    }

    [Fact]
    public void Indexer_Depth1_OnTwoCoefficientCF_ReturnsExactRational()
    {
        // [3; 7] = 3 + 1/7 = 22/7
        var cf = new ContinuedFraction(3, 7);
        Assert.Equal(new BigRational(22, 7), cf[1]);
    }

    [Fact]
    public void Indexer_FollowsConvergentRecurrence()
    {
        // [1; 2, 3] convergents: 1/1, 3/2, 10/7
        // n=0: a0=1 -> p=1,q=1 -> 1/1
        // n=1: a1=2 -> p=2*1+0=2+1?? wait
        // Recurrence with init p_-1=1,p_-2=0; q_-1=0,q_-2=1
        // n=0: a=1, p=1*1+0=1, q=1*0+1=1, conv=1/1
        // n=1: a=2, p=2*1+1=3, q=2*1+0=2, conv=3/2
        // n=2: a=3, p=3*3+1=10, q=3*2+1=7, conv=10/7
        var cf = new ContinuedFraction(1, 2, 3);
        Assert.Equal(new BigRational(1, 1), cf[0]);
        Assert.Equal(new BigRational(3, 2), cf[1]);
        Assert.Equal(new BigRational(10, 7), cf[2]);
    }

    [Fact]
    public void Indexer_NegativeIntegerPart_RecurrenceCorrect()
    {
        // [-2; 1, 4] = -2 + 1/(1 + 1/4) = -2 + 4/5 = -6/5
        // n=0: a=-2, p=-2, q=1, conv=-2/1
        // n=1: a=1, p=1*-2+1=-1, q=1*1+0=1, conv=-1/1
        // n=2: a=4, p=4*-1+-2=-6, q=4*1+1=5, conv=-6/5
        var cf = new ContinuedFraction(-2, 1, 4);
        Assert.Equal(new BigRational(-6, 5), cf[2]);
    }

    // ---------- φ (golden ratio): [1; 1, 1, 1, ...] ----------

    [Fact]
    public void Phi_ConvergentsAreFibonacciRatios()
    {
        var phi = new ContinuedFraction(
            new FuncCFCoefficientGenerator("φ", _ => BigInteger.One));

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
        var sqrt2 = new ContinuedFraction(
            new FuncCFCoefficientGenerator("√2", i => i == 0 ? BigInteger.One : new BigInteger(2)));

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
        var cf = new ContinuedFraction(1, 2, 3);
        var convergents = cf.ToList();
        Assert.Equal(3, convergents.Count);
        Assert.Equal(new BigRational(1, 1), convergents[0]);
        Assert.Equal(new BigRational(3, 2), convergents[1]);
        Assert.Equal(new BigRational(10, 7), convergents[2]);
    }

    [Fact]
    public void Iteration_UnboundedCF_YieldsIndefinitelyUntilConsumerStops()
    {
        var phi = new ContinuedFraction(
            new FuncCFCoefficientGenerator("φ", _ => BigInteger.One));

        var first6 = phi.Take(6).ToList();
        Assert.Equal(6, first6.Count);
        Assert.Equal(new BigRational(13, 8), first6[5]);
    }

    [Fact]
    public void Iteration_IntegerCF_YieldsExactlyOne()
    {
        var cf = new ContinuedFraction(7);
        var convergents = cf.ToList();
        Assert.Single(convergents);
        Assert.Equal(new BigRational(7, 1), convergents[0]);
    }

    // ---------- memoization ----------

    [Fact]
    public void Memoization_RepeatedAccessReturnsSameValue()
    {
        var cf = new ContinuedFraction(1, 2, 3);
        Assert.Equal(cf[2], cf[2]);
        Assert.Equal(cf[1], cf[1]);
    }

    [Fact]
    public void Memoization_OnlyCallsGeneratorOncePerIndex()
    {
        var callCounts = new int[10];
        var gen = new FuncCFCoefficientGenerator(
            "counted",
            i =>
            {
                callCounts[i]++;
                return i == 0 ? BigInteger.One : new BigInteger(2);
            });
        var cf = new ContinuedFraction(gen);

        // First pass: indices 0..4
        _ = cf[4];
        // Second pass: indexer at depth 4, then at depth 0..3 — should reuse cache
        _ = cf[4];
        _ = cf[0];
        _ = cf[3];

        for (var i = 0; i <= 4; i++)
        {
            Assert.Equal(1, callCounts[i]);
        }
    }

    [Fact]
    public void Memoization_IteratorAndIndexerShareCache()
    {
        var callCounts = new int[10];
        var gen = new FuncCFCoefficientGenerator(
            "counted",
            i =>
            {
                callCounts[i]++;
                return i == 0 ? BigInteger.One : new BigInteger(2);
            });
        var cf = new ContinuedFraction(gen);

        // Walk to depth 4 via iterator
        _ = cf.Take(5).ToList();
        // Now indexer access should not re-invoke generator
        _ = cf[2];

        for (var i = 0; i <= 4; i++)
        {
            Assert.Equal(1, callCounts[i]);
        }
    }

    // ---------- indexer validation ----------

    [Fact]
    public void Indexer_RejectsNegativeDepth()
    {
        var cf = new ContinuedFraction(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => cf[-1]);
    }

    [Fact]
    public void Indexer_RejectsDepthAtFiniteLength()
    {
        var cf = new ContinuedFraction(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => cf[3]);
    }

    [Fact]
    public void Indexer_RejectsDepthBeyondFiniteLength()
    {
        var cf = new ContinuedFraction(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => cf[100]);
    }

    [Fact]
    public void Indexer_AcceptsHighDepthOnUnboundedCF()
    {
        var phi = new ContinuedFraction(
            new FuncCFCoefficientGenerator("φ", _ => BigInteger.One));
        // Just don't throw.
        _ = phi[100];
    }

    // ---------- partial-quotient invariant validation (lazy) ----------

    [Fact]
    public void Iteration_NonPositivePartialQuotientThrows()
    {
        // Generator that returns 0 at index 1 — violates a_i > 0 for i >= 1
        var bad = new FuncCFCoefficientGenerator(
            "bad",
            i => i == 0 ? BigInteger.One : BigInteger.Zero);
        var cf = new ContinuedFraction(bad);

        // Reading depth 0 is fine
        _ = cf[0];
        // Reading depth 1 triggers validation
        Assert.Throws<InvalidOperationException>(() => cf[1]);
    }

    [Fact]
    public void Iteration_NegativePartialQuotientThrows()
    {
        var bad = new FuncCFCoefficientGenerator(
            "bad",
            i => i == 0 ? BigInteger.One : new BigInteger(-3));
        var cf = new ContinuedFraction(bad);

        Assert.Throws<InvalidOperationException>(() => cf[1]);
    }

    [Fact]
    public void Iteration_NegativeIntegerPartIsAllowed()
    {
        // a_0 may be negative — invariant only applies to a_i for i >= 1
        var gen = new FuncCFCoefficientGenerator(
            "[-5; 1, 1, 1, ...]",
            i => i == 0 ? new BigInteger(-5) : BigInteger.One);
        var cf = new ContinuedFraction(gen);
        Assert.Equal(new BigRational(-5, 1), cf[0]);
        Assert.Equal(new BigRational(-4, 1), cf[1]);
    }
}
