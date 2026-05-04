using System.Numerics;
using ContinuedFractions.Generators;
using HalHeinrich.Numerics;

namespace ContinuedFractions.Tests;

public class EulersNumberTests
{
    [Fact]
    public void Indexer_AtZero_ReturnsTwo()
    {
        Assert.Equal(new BigInteger(2), new EulersNumber()[0]);
    }

    [Fact]
    public void Indexer_FollowsExpectedPattern()
    {
        // [2; 1, 2, 1, 1, 4, 1, 1, 6, 1, 1, 8, ...]
        var e = new EulersNumber();
        Assert.Equal(new BigInteger(2), e[0]);
        Assert.Equal(new BigInteger(1), e[1]);
        Assert.Equal(new BigInteger(2), e[2]);
        Assert.Equal(new BigInteger(1), e[3]);
        Assert.Equal(new BigInteger(1), e[4]);
        Assert.Equal(new BigInteger(4), e[5]);
        Assert.Equal(new BigInteger(1), e[6]);
        Assert.Equal(new BigInteger(1), e[7]);
        Assert.Equal(new BigInteger(6), e[8]);
        Assert.Equal(new BigInteger(1), e[9]);
        Assert.Equal(new BigInteger(1), e[10]);
        Assert.Equal(new BigInteger(8), e[11]);
    }

    [Fact]
    public void Indexer_RejectsNegativeIndex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EulersNumber()[-1]);
    }

    [Fact]
    public void Length_IsNullUnbounded()
    {
        Assert.Null(new EulersNumber().Length);
    }

    [Fact]
    public void ToString_ReturnsE()
    {
        Assert.Equal("e", new EulersNumber().ToString());
    }

    [Fact]
    public void Convergents_ApproximateE()
    {
        // Classical convergents of e: 2/1, 3/1, 8/3, 11/4, 19/7, 87/32, 106/39
        var cf = new ContinuedFraction(new EulersNumber());
        Assert.Equal(new BigRational(2, 1), cf[0]);
        Assert.Equal(new BigRational(3, 1), cf[1]);
        Assert.Equal(new BigRational(8, 3), cf[2]);
        Assert.Equal(new BigRational(11, 4), cf[3]);
        Assert.Equal(new BigRational(19, 7), cf[4]);
        Assert.Equal(new BigRational(87, 32), cf[5]);
        Assert.Equal(new BigRational(106, 39), cf[6]);
    }
}
