using System.Numerics;

namespace ContinuedFractions.Tests;

public class ContinuedFractionTests
{
    [Fact]
    public void Constructor_StoresCoefficients()
    {
        var cf = new ContinuedFraction(3, 7, 15, 1, 292);
        Assert.Equal(
            new BigInteger[] { 3, 7, 15, 1, 292 },
            cf.Coefficients);
    }

    [Fact]
    public void Constructor_AcceptsLargeCoefficients()
    {
        var huge = BigInteger.Pow(10, 40);
        var cf = new ContinuedFraction(new BigInteger[] { 0, huge });
        Assert.Equal(huge, cf.Coefficients[1]);
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
        Assert.Equal(new BigInteger(-2), cf.Coefficients[0]);
    }
}
