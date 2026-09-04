using System.Numerics;
using HalHeinrich.Numerics.ContinuedFractions.Generators;

namespace HalHeinrich.Numerics.ContinuedFractions.Tests;

public class PatternsTests
{
    // ---------- named constants ----------

    [Fact]
    public void Phi_HasSingleConstLane_AndIsQuadraticIrrational()
    {
        var phi = Patterns.Phi();

        Assert.True(phi.IsQuadraticIrrational);
        Assert.Empty(phi.Preperiod);
        Assert.Single(phi.Lanes);
        Assert.Equal(Operation.Const, phi.Lanes[0].Operation);
        Assert.Equal(BigInteger.One, phi.Lanes[0].InitialValue);
    }

    [Fact]
    public void Sqrt2_PreperiodOne_PeriodTwo()
    {
        var sqrt2 = Patterns.Sqrt2();

        Assert.True(sqrt2.IsQuadraticIrrational);
        Assert.Equal(new BigInteger[] { 1 }, sqrt2.Preperiod);
        Assert.Single(sqrt2.Lanes);
        Assert.Equal(new BigInteger(2), sqrt2.Lanes[0].InitialValue);
    }

    [Fact]
    public void EulersNumber_NotQuadratic_AndPlusLaneInTheMiddle()
    {
        var e = Patterns.EulersNumber();

        Assert.False(e.IsQuadraticIrrational);
        Assert.False(e.IsRational);
        Assert.Equal(new BigInteger[] { 2 }, e.Preperiod);
        Assert.Equal(3, e.Lanes.Count);
        Assert.Equal(Operation.Const, e.Lanes[0].Operation);
        Assert.Equal(Operation.Plus, e.Lanes[1].Operation);
        Assert.Equal(Operation.Const, e.Lanes[2].Operation);
    }

    // ---------- Rational ----------

    [Fact]
    public void Rational_RejectsZeroDenominator()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Patterns.Rational(1, 0));
    }

    [Theory]
    [InlineData(22, 7, new long[] { 3, 7 })]                  // 22/7 = [3; 7]
    [InlineData(7, 1, new long[] { 7 })]                      // integer 7 = [7]
    [InlineData(0, 1, new long[] { 0 })]                      // 0 = [0]
    [InlineData(1, 1, new long[] { 1 })]                      // 1 = [1]
    [InlineData(3, 2, new long[] { 1, 2 })]                   // 3/2 = [1; 2]
    [InlineData(5, 3, new long[] { 1, 1, 2 })]                // 5/3 = [1; 1, 2]
    [InlineData(355, 113, new long[] { 3, 7, 16 })]           // π's third convergent
    [InlineData(-22, 7, new long[] { -4, 1, 6 })]             // negative integer part
    [InlineData(22, -7, new long[] { -4, 1, 6 })]             // sign normalisation
    public void Rational_ProducesEuclideanCfExpansion(long num, long den, long[] expected)
    {
        var pattern = Patterns.Rational(num, den);

        var actual = pattern.Preperiod.Select(b => (long)b).ToArray();
        Assert.Equal(expected, actual);
        Assert.True(pattern.IsRational);
        Assert.Empty(pattern.Lanes);
    }

    [Fact]
    public void Rational_LargeNumeratorAndDenominator()
    {
        var huge = BigInteger.Pow(10, 30);
        var pattern = Patterns.Rational(huge + BigInteger.One, huge);

        // (10^30 + 1) / 10^30 = 1 + 1/10^30 = [1; 10^30]
        Assert.True(pattern.IsRational);
        Assert.Equal(2, pattern.Preperiod.Count);
        Assert.Equal(BigInteger.One, pattern.Preperiod[0]);
        Assert.Equal(huge, pattern.Preperiod[1]);
    }
}
