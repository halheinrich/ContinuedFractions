using System.Globalization;
using System.Numerics;

namespace ContinuedFractions.Tests;

public class ContinuedFractionTests
{
    // ---------- construction ----------

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

    // ---------- encapsulation ----------

    [Fact]
    public void Coefficients_CannotBeMutatedByCallers()
    {
        // The backing store must not be reachable as a mutable array even
        // through casting, otherwise the invariants of the type can be
        // violated post-construction.
        var cf = new ContinuedFraction(3, 7, 15);
        Assert.False(cf.Coefficients is BigInteger[],
            "Coefficients should not expose its mutable backing array.");
    }

    // ---------- properties ----------

    [Fact]
    public void IntegerPart_ReturnsFirstCoefficient()
    {
        Assert.Equal(new BigInteger(3), new ContinuedFraction(3, 7).IntegerPart);
        Assert.Equal(new BigInteger(-5), new ContinuedFraction(-5).IntegerPart);
    }

    [Fact]
    public void IsInteger_TrueOnlyWhenNoPartialQuotients()
    {
        Assert.True(new ContinuedFraction(7).IsInteger);
        Assert.False(new ContinuedFraction(7, 1).IsInteger);
    }

    // ---------- equality ----------

    [Fact]
    public void Equals_TrueForMatchingCoefficients()
    {
        var a = new ContinuedFraction(3, 7, 15, 1, 292);
        var b = new ContinuedFraction(3, 7, 15, 1, 292);
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equals_FalseForDifferentLengths()
    {
        var a = new ContinuedFraction(3, 7);
        var b = new ContinuedFraction(3, 7, 15);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equals_FalseForDifferingCoefficient()
    {
        var a = new ContinuedFraction(3, 7, 15);
        var b = new ContinuedFraction(3, 7, 16);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void EqualityOperator_HandlesNull()
    {
        ContinuedFraction? a = null;
        ContinuedFraction? b = null;
        var c = new ContinuedFraction(1);
        Assert.True(a == b);
        Assert.False(a == c);
        Assert.False(c == a);
        Assert.True(a != c);
    }

    [Fact]
    public void HashCode_ConsistentWithEquality()
    {
        var a = new ContinuedFraction(3, 7, 15, 1, 292);
        var b = new ContinuedFraction(3, 7, 15, 1, 292);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    // ---------- formatting ----------

    [Fact]
    public void ToString_RendersCanonicalForm()
    {
        var cf = new ContinuedFraction(3, 7, 15, 1, 292);
        Assert.Equal("[3; 7, 15, 1, 292]", cf.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ToString_OmitsSemicolonForBareInteger()
    {
        var cf = new ContinuedFraction(7);
        Assert.Equal("[7]", cf.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ToString_HandlesNegativeIntegerPart()
    {
        var cf = new ContinuedFraction(-2, 1, 4);
        Assert.Equal("[-2; 1, 4]", cf.ToString(CultureInfo.InvariantCulture));
    }
}
