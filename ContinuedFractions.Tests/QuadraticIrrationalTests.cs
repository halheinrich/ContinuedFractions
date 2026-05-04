using System.Numerics;

namespace ContinuedFractions.Tests;

public class QuadraticIrrationalTests
{
    // ---------- construction: validation ----------

    [Fact]
    public void Constructor_AcceptsValidCanonicalForm()
    {
        var qi = new QuadraticIrrational(7, 2, 3);  // d − p² = 3, q | 3 ✓

        Assert.Equal(new BigInteger(7), qi.D);
        Assert.Equal(new BigInteger(2), qi.P);
        Assert.Equal(new BigInteger(3), qi.Q);
    }

    [Fact]
    public void Constructor_AcceptsPlainSquareRoot()
    {
        var qi = new QuadraticIrrational(7, 0, 1);

        Assert.Equal(new BigInteger(7), qi.D);
        Assert.Equal(BigInteger.Zero, qi.P);
        Assert.Equal(BigInteger.One, qi.Q);
    }

    [Fact]
    public void Constructor_RejectsNegativeD()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new QuadraticIrrational(-1, 0, 1));
    }

    [Fact]
    public void Constructor_AcceptsNegativeP()
    {
        // d − p² = 5 − 1 = 4; q = 2 divides 4 ✓. φ = (1 + √5)/2.
        var qi = new QuadraticIrrational(5, -1, 2);

        Assert.Equal(new BigInteger(5), qi.D);
        Assert.Equal(new BigInteger(-1), qi.P);
        Assert.Equal(new BigInteger(2), qi.Q);
    }

    [Fact]
    public void Constructor_RejectsZeroQ()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new QuadraticIrrational(7, 0, 0));
    }

    [Fact]
    public void Constructor_RejectsNegativeQ()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new QuadraticIrrational(7, 0, -1));
    }

    [Fact]
    public void Constructor_RejectsNonDivisibleQ()
    {
        // d − p² = 2 − 1 = 1; q = 2 does not divide 1.
        Assert.Throws<ArgumentException>(() =>
            new QuadraticIrrational(2, 1, 2));
    }

    [Fact]
    public void Constructor_AcceptsNegativeDMinusPSquaredWhenQDivides()
    {
        // d − p² = 2 − 9 = −7; q = 1 divides −7 ✓.
        var qi = new QuadraticIrrational(2, 3, 1);

        Assert.Equal(new BigInteger(2), qi.D);
        Assert.Equal(new BigInteger(3), qi.P);
        Assert.Equal(BigInteger.One, qi.Q);
    }

    // ---------- ToString ----------

    [Theory]
    [InlineData(7, 0, 1, "√7")]
    [InlineData(63, 0, 9, "√63/9")]       // = √7/3 in canonical form
    [InlineData(7, 2, 1, "√7 − 2")]
    [InlineData(7, 2, 3, "(√7 − 2)/3")]   // d − p² = 3, q = 3 ✓
    [InlineData(2, 0, 1, "√2")]
    [InlineData(23, 0, 1, "√23")]
    [InlineData(5, -1, 2, "(√5 + 1)/2")]  // φ — negative p renders as "+ |p|"
    [InlineData(5, -3, 1, "√5 + 3")]      // negative p, q = 1
    public void ToString_RendersCanonicalForm(long d, long p, long q, string expected)
    {
        var qi = new QuadraticIrrational(d, p, q);
        Assert.Equal(expected, qi.ToString());
    }

    // ---------- value equality ----------

    [Fact]
    public void Equals_BasedOnComponentValues()
    {
        var a = new QuadraticIrrational(7, 2, 3);
        var b = new QuadraticIrrational(7, 2, 3);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DistinguishesDifferentTriples()
    {
        var a = new QuadraticIrrational(7, 2, 3);
        var b = new QuadraticIrrational(7, 0, 1);

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }
}
