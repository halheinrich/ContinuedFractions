using System.Numerics;

namespace ContinuedFractions.Tests;

public class ContinuedFractionTests
{
    // ---------- construction ----------

    [Fact]
    public void Constructor_StoresCoefficientsInGenerator()
    {
        var cf = new ContinuedFraction(3, 7, 15, 1, 292);
        Assert.Equal(
            new BigInteger[] { 3, 7, 15, 1, 292 },
            cf.Generator.ToList());
    }

    [Fact]
    public void Constructor_AcceptsLargeCoefficients()
    {
        var huge = BigInteger.Pow(10, 40);
        var cf = new ContinuedFraction(new BigInteger[] { 0, huge });
        Assert.Equal(huge, cf.Generator[1]);
    }

    [Fact]
    public void Constructor_RejectsNullEnumerable()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ContinuedFraction((IEnumerable<BigInteger>)null!));
    }

    [Fact]
    public void Constructor_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ContinuedFraction(Array.Empty<BigInteger>()));
    }

    [Fact]
    public void Constructor_RejectsNonPositivePartialQuotient()
    {
        Assert.Throws<ArgumentException>(() => new ContinuedFraction(1, 0, 2));
        Assert.Throws<ArgumentException>(() => new ContinuedFraction(1, -3, 2));
    }

    [Fact]
    public void Constructor_AllowsNegativeIntegerPart()
    {
        var cf = new ContinuedFraction(-2, 1, 4);
        Assert.Equal(new BigInteger(-2), cf.IntegerPart);
    }

    // ---------- properties ----------

    [Fact]
    public void IntegerPart_ReturnsFirstCoefficient()
    {
        Assert.Equal(new BigInteger(3), new ContinuedFraction(3, 7).IntegerPart);
        Assert.Equal(new BigInteger(-5), new ContinuedFraction(-5).IntegerPart);
    }

    // ---------- formatting ----------

    [Fact]
    public void ToString_DelegatesToGenerator()
    {
        var cf = new ContinuedFraction(3, 7, 15, 1, 292);
        Assert.Equal(cf.Generator.ToString(), cf.ToString());
    }

    [Fact]
    public void ToString_ListBackedCfReturnsBracketNotation()
    {
        var cf = new ContinuedFraction(3, 7, 15, 1, 292);
        Assert.Equal("[3; 7, 15, 1, 292]", cf.ToString());
    }
}
