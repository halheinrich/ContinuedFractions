using System.Numerics;

namespace HalHeinrich.Numerics.ContinuedFractions.Tests;

public class IntegerMathTests
{
    // ---------- Sqrt: Floor (default) ----------

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(5, 2)]
    [InlineData(8, 2)]
    [InlineData(9, 3)]
    [InlineData(10, 3)]
    [InlineData(99, 9)]
    [InlineData(100, 10)]
    [InlineData(101, 10)]
    [InlineData(120, 10)]
    [InlineData(121, 11)]
    public void Sqrt_DefaultsToFloor(long input, long expected)
    {
        Assert.Equal(new BigInteger(expected), IntegerMath.Sqrt(new BigInteger(input)));
    }

    [Fact]
    public void Sqrt_Floor_OnLargeInput()
    {
        // 10^60 has integer sqrt 10^30 exactly.
        var input = BigInteger.Pow(10, 60);
        var expected = BigInteger.Pow(10, 30);

        Assert.Equal(expected, IntegerMath.Sqrt(input));
    }

    [Fact]
    public void Sqrt_Floor_OnLargeNonSquare()
    {
        // 10^60 - 1: floor sqrt is 10^30 - 1.
        var input = BigInteger.Pow(10, 60) - BigInteger.One;
        var expected = BigInteger.Pow(10, 30) - BigInteger.One;

        Assert.Equal(expected, IntegerMath.Sqrt(input));
    }

    // ---------- Sqrt: Ceiling ----------

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 3)]
    [InlineData(10, 4)]
    [InlineData(99, 10)]
    [InlineData(100, 10)]
    [InlineData(101, 11)]
    public void Sqrt_Ceiling(long input, long expected)
    {
        Assert.Equal(
            new BigInteger(expected),
            IntegerMath.Sqrt(new BigInteger(input), IntegerSqrtRounding.Ceiling));
    }

    // ---------- Sqrt: Nearest ----------

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]    // √2 ≈ 1.414, closer to 1
    [InlineData(3, 2)]    // √3 ≈ 1.732, closer to 2
    [InlineData(4, 2)]
    [InlineData(5, 2)]    // √5 ≈ 2.236, closer to 2
    [InlineData(6, 2)]    // √6 ≈ 2.449, closer to 2
    [InlineData(7, 3)]    // √7 ≈ 2.646, closer to 3
    [InlineData(8, 3)]    // √8 ≈ 2.828, closer to 3
    [InlineData(9, 3)]
    [InlineData(20, 4)]   // √20 ≈ 4.472, closer to 4
    [InlineData(30, 5)]   // √30 ≈ 5.477, closer to 5
    public void Sqrt_Nearest(long input, long expected)
    {
        Assert.Equal(
            new BigInteger(expected),
            IntegerMath.Sqrt(new BigInteger(input), IntegerSqrtRounding.Nearest));
    }

    // ---------- Validation ----------

    [Fact]
    public void Sqrt_RejectsNegativeInput()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IntegerMath.Sqrt(new BigInteger(-1)));
    }

    [Fact]
    public void Sqrt_RejectsUndefinedRoundingMode()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IntegerMath.Sqrt(new BigInteger(4), (IntegerSqrtRounding)42));
    }

    // ---------- Defining property checks ----------

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(100)]
    [InlineData(1_000_000)]
    [InlineData(1_000_001)]
    public void Sqrt_Floor_SatisfiesDefiningProperty(long input)
    {
        // ⌊√n⌋ = k satisfies k² ≤ n < (k+1)².
        var n = new BigInteger(input);
        var k = IntegerMath.Sqrt(n);

        Assert.True(k * k <= n);
        Assert.True(n < (k + BigInteger.One) * (k + BigInteger.One));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(100)]
    public void Sqrt_Ceiling_SatisfiesDefiningProperty(long input)
    {
        // ⌈√n⌉ = k satisfies (k-1)² < n ≤ k² (or k = 0 when n = 0).
        var n = new BigInteger(input);
        var k = IntegerMath.Sqrt(n, IntegerSqrtRounding.Ceiling);

        Assert.True(k * k >= n);
        if (!k.IsZero)
        {
            Assert.True((k - BigInteger.One) * (k - BigInteger.One) < n);
        }
    }
}
