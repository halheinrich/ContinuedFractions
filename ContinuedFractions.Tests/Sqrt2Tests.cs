using System.Numerics;
using HalHeinrich.Numerics;

namespace ContinuedFractions.Tests;

public class Sqrt2Tests
{
    [Fact]
    public void Indexer_AtZero_ReturnsOne()
    {
        Assert.Equal(BigInteger.One, new Sqrt2()[0]);
    }

    [Fact]
    public void Indexer_FromIndexOneOnward_ReturnsTwo()
    {
        var sqrt2 = new Sqrt2();
        Assert.Equal(new BigInteger(2), sqrt2[1]);
        Assert.Equal(new BigInteger(2), sqrt2[2]);
        Assert.Equal(new BigInteger(2), sqrt2[100]);
    }

    [Fact]
    public void Indexer_RejectsNegativeIndex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Sqrt2()[-1]);
    }

    [Fact]
    public void Length_IsNullUnbounded()
    {
        Assert.Null(new Sqrt2().Length);
    }

    [Fact]
    public void ToString_ReturnsRoot2()
    {
        Assert.Equal("√2", new Sqrt2().ToString());
    }

    [Fact]
    public void Convergents_ApproximateRoot2()
    {
        // Classical convergents of √2: 1/1, 3/2, 7/5, 17/12, 41/29, 99/70
        var cf = new ContinuedFraction(new Sqrt2());
        Assert.Equal(new BigRational(1, 1), cf[0]);
        Assert.Equal(new BigRational(3, 2), cf[1]);
        Assert.Equal(new BigRational(7, 5), cf[2]);
        Assert.Equal(new BigRational(17, 12), cf[3]);
        Assert.Equal(new BigRational(41, 29), cf[4]);
        Assert.Equal(new BigRational(99, 70), cf[5]);
    }
}
