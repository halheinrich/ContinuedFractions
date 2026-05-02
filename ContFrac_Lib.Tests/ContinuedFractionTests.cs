namespace ContFrac_Lib.Tests;

public class ContinuedFractionTests
{
    [Fact]
    public void Constructor_StoresCoefficients()
    {
        var cf = new ContinuedFraction(3, 7, 15, 1, 292);
        Assert.Equal(new long[] { 3, 7, 15, 1, 292 }, cf.Coefficients);
    }

    [Fact]
    public void Constructor_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ContinuedFraction(Array.Empty<long>()));
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
        Assert.Equal(-2, cf.Coefficients[0]);
    }
}
