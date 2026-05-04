using System.Numerics;
using HalHeinrich.Numerics;

namespace ContinuedFractions.Tests;

public class PhiTests
{
    [Fact]
    public void Indexer_AlwaysReturnsOne()
    {
        var phi = new Phi();
        Assert.Equal(BigInteger.One, phi[0]);
        Assert.Equal(BigInteger.One, phi[1]);
        Assert.Equal(BigInteger.One, phi[1000]);
    }

    [Fact]
    public void Indexer_RejectsNegativeIndex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Phi()[-1]);
    }

    [Fact]
    public void Length_IsNullUnbounded()
    {
        Assert.Null(new Phi().Length);
    }

    [Fact]
    public void ToString_ReturnsGreekPhi()
    {
        Assert.Equal("φ", new Phi().ToString());
    }

    [Fact]
    public void Convergents_AreFibonacciRatios()
    {
        var cf = new ContinuedFraction(new Phi());
        Assert.Equal(new BigRational(1, 1), cf[0]);
        Assert.Equal(new BigRational(2, 1), cf[1]);
        Assert.Equal(new BigRational(3, 2), cf[2]);
        Assert.Equal(new BigRational(5, 3), cf[3]);
        Assert.Equal(new BigRational(8, 5), cf[4]);
        Assert.Equal(new BigRational(13, 8), cf[5]);
        Assert.Equal(new BigRational(21, 13), cf[6]);
    }
}
