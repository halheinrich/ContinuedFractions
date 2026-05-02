using System.Numerics;

namespace ContinuedFractions.Tests;

public class BigRationalTests
{
    [Fact]
    public void Constructor_ReducesByGcd()
    {
        var r = new BigRational(4, 8);
        Assert.Equal(new BigInteger(1), r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void Constructor_NormalizesNegativeDenominator()
    {
        var r = new BigRational(1, -2);
        Assert.Equal(new BigInteger(-1), r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void Constructor_NormalizesDoubleNegative()
    {
        var r = new BigRational(-3, -6);
        Assert.Equal(new BigInteger(1), r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void Constructor_ZeroNumeratorBecomesZeroOverOne()
    {
        var r = new BigRational(0, 7);
        Assert.Equal(BigRational.Zero, r);
        Assert.Equal(new BigInteger(1), r.Denominator);
    }

    [Fact]
    public void Constructor_ZeroDenominatorThrows()
    {
        Assert.Throws<DivideByZeroException>(() => new BigRational(1, 0));
    }

    [Fact]
    public void Addition_ReducesResult()
    {
        var r = new BigRational(1, 2) + new BigRational(1, 3);
        Assert.Equal(new BigRational(5, 6), r);
    }

    [Fact]
    public void Addition_CancelsToWholeNumber()
    {
        var r = new BigRational(1, 6) + new BigRational(5, 6);
        Assert.Equal(BigRational.One, r);
    }

    [Fact]
    public void Subtraction_HandlesNegativeResult()
    {
        var r = new BigRational(1, 3) - new BigRational(1, 2);
        Assert.Equal(new BigRational(-1, 6), r);
    }

    [Fact]
    public void Multiplication_ReducesResult()
    {
        var r = new BigRational(2, 3) * new BigRational(3, 4);
        Assert.Equal(new BigRational(1, 2), r);
    }

    [Fact]
    public void Division_ByZeroThrows()
    {
        Assert.Throws<DivideByZeroException>(
            () => new BigRational(1, 2) / BigRational.Zero);
    }

    [Fact]
    public void UnaryNegation_FlipsSign()
    {
        var r = -new BigRational(3, 7);
        Assert.Equal(new BigRational(-3, 7), r);
    }

    [Fact]
    public void Equality_HoldsAcrossUnreducedInputs()
    {
        Assert.Equal(new BigRational(1, 2), new BigRational(50, 100));
        Assert.Equal(new BigRational(-1, 2), new BigRational(3, -6));
    }

    [Fact]
    public void ImplicitConversion_FromBigInteger()
    {
        BigRational r = new BigInteger(5);
        Assert.Equal(new BigInteger(5), r.Numerator);
        Assert.Equal(BigInteger.One, r.Denominator);
    }

    [Fact]
    public void ToString_OmitsDenominatorOfOne()
    {
        Assert.Equal("5", new BigRational(5, 1).ToString());
        Assert.Equal("-3/7", new BigRational(-3, 7).ToString());
    }

    [Fact]
    public void Arithmetic_OnLargeValues()
    {
        var big = BigInteger.Pow(10, 50);
        var r = new BigRational(big, 1) + new BigRational(1, big);
        Assert.Equal(new BigRational(big * big + 1, big), r);
    }
}
