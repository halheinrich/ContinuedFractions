using System.Globalization;
using System.Numerics;

namespace ContinuedFractions.Tests;

public class BigRationalTests
{
    // ---------- construction & normalization ----------

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

    // ---------- properties ----------

    [Fact]
    public void Sign_ReturnsExpectedValue()
    {
        Assert.Equal(0, BigRational.Zero.Sign);
        Assert.Equal(1, new BigRational(3, 4).Sign);
        Assert.Equal(-1, new BigRational(-3, 4).Sign);
    }

    [Fact]
    public void IsInteger_TrueWhenDenominatorIsOne()
    {
        Assert.True(new BigRational(5, 1).IsInteger);
        Assert.True(new BigRational(6, 2).IsInteger);
        Assert.False(new BigRational(1, 2).IsInteger);
    }

    [Fact]
    public void Abs_ReturnsAbsoluteValue()
    {
        Assert.Equal(new BigRational(3, 7), BigRational.Abs(new BigRational(-3, 7)));
        Assert.Equal(new BigRational(3, 7), BigRational.Abs(new BigRational(3, 7)));
        Assert.Equal(BigRational.Zero, BigRational.Abs(BigRational.Zero));
    }

    [Fact]
    public void Reciprocal_FlipsAndPreservesSign()
    {
        Assert.Equal(new BigRational(7, 3), BigRational.Reciprocal(new BigRational(3, 7)));
        Assert.Equal(new BigRational(-7, 3), BigRational.Reciprocal(new BigRational(-3, 7)));
    }

    [Fact]
    public void Reciprocal_OfZeroThrows()
    {
        Assert.Throws<DivideByZeroException>(() => BigRational.Reciprocal(BigRational.Zero));
    }

    // ---------- arithmetic ----------

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
    public void NamedAlternates_MatchOperators()
    {
        var a = new BigRational(2, 3);
        var b = new BigRational(3, 4);
        Assert.Equal(a + b, BigRational.Add(a, b));
        Assert.Equal(a - b, BigRational.Subtract(a, b));
        Assert.Equal(-a, BigRational.Negate(a));
        Assert.Equal(a * b, BigRational.Multiply(a, b));
        Assert.Equal(a / b, BigRational.Divide(a, b));
    }

    [Fact]
    public void Arithmetic_OnLargeValues()
    {
        var big = BigInteger.Pow(10, 50);
        var r = new BigRational(big, 1) + new BigRational(1, big);
        Assert.Equal(new BigRational((big * big) + 1, big), r);
    }

    // ---------- equality & hashing ----------

    [Fact]
    public void Equality_HoldsAcrossUnreducedInputs()
    {
        Assert.Equal(new BigRational(1, 2), new BigRational(50, 100));
        Assert.Equal(new BigRational(-1, 2), new BigRational(3, -6));
    }

    [Fact]
    public void HashCode_ConsistentWithEquality()
    {
        var a = new BigRational(50, 100);
        var b = new BigRational(1, 2);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    // ---------- comparison ----------

    [Fact]
    public void Comparison_OrdersCorrectly()
    {
        var third = new BigRational(1, 3);
        var half = new BigRational(1, 2);
        var thirdAgain = new BigRational(2, 6);
        Assert.True(third < half);
        Assert.True(half > third);
        Assert.True(third <= thirdAgain);
        Assert.True(third >= thirdAgain);
        Assert.Equal(0, third.CompareTo(thirdAgain));
    }

    [Fact]
    public void Comparison_HandlesNegativesAcrossZero()
    {
        var negative = new BigRational(-1, 100);
        var positive = new BigRational(1, 1_000_000);
        Assert.True(negative < positive);
        Assert.True(negative < BigRational.Zero);
        Assert.True(BigRational.Zero < positive);
    }

    [Fact]
    public void CompareTo_NonGenericRejectsForeignType()
    {
        Assert.Throws<ArgumentException>(() => BigRational.One.CompareTo("not a rational"));
        Assert.Equal(1, BigRational.One.CompareTo(null));
    }

    // ---------- conversions ----------

    [Fact]
    public void FromInteger_FactoriesAgree()
    {
        var fromBig = BigRational.FromBigInteger(new BigInteger(5));
        var fromLong = BigRational.FromInt64(5L);
        var fromInt = BigRational.FromInt32(5);
        Assert.Equal(fromBig, fromLong);
        Assert.Equal(fromLong, fromInt);
        Assert.Equal(new BigInteger(5), fromBig.Numerator);
        Assert.Equal(BigInteger.One, fromBig.Denominator);
    }

    [Fact]
    public void ExplicitConversion_FromIntegerTypes()
    {
        var fromBig = (BigRational)new BigInteger(7);
        var fromLong = (BigRational)7L;
        var fromInt = (BigRational)7;
        Assert.Equal(fromBig, fromLong);
        Assert.Equal(fromLong, fromInt);
    }

    // ---------- formatting ----------

    [Fact]
    public void ToString_OmitsDenominatorOfOne()
    {
        Assert.Equal("5", new BigRational(5, 1).ToString(CultureInfo.InvariantCulture));
        Assert.Equal("-3/7", new BigRational(-3, 7).ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ToString_ForwardsFormatToBothComponents()
    {
        // 16/3 in hex is 10/3. (Avoid values whose high hex bit is set; BigInteger
        // pads those with a leading 0 to disambiguate sign — e.g. 255 -> "0FF".)
        var r = new BigRational(16, 3);
        Assert.Equal("10/3", r.ToString("X", CultureInfo.InvariantCulture));
    }

    // ---------- parsing ----------

    [Fact]
    public void TryParse_AcceptsCanonicalFraction()
    {
        Assert.True(BigRational.TryParse("3/7", CultureInfo.InvariantCulture, out var r));
        Assert.Equal(new BigRational(3, 7), r);
    }

    [Fact]
    public void TryParse_AcceptsBareInteger()
    {
        Assert.True(BigRational.TryParse("-42", CultureInfo.InvariantCulture, out var r));
        Assert.Equal(new BigRational(-42, 1), r);
    }

    [Fact]
    public void TryParse_AcceptsWhitespaceAroundComponents()
    {
        Assert.True(BigRational.TryParse("  3 / 7  ", CultureInfo.InvariantCulture, out var r));
        Assert.Equal(new BigRational(3, 7), r);
    }

    [Fact]
    public void TryParse_RejectsZeroDenominator()
    {
        Assert.False(BigRational.TryParse("5/0", CultureInfo.InvariantCulture, out _));
    }

    [Fact]
    public void TryParse_RejectsMalformed()
    {
        Assert.False(BigRational.TryParse("not a number", CultureInfo.InvariantCulture, out _));
        Assert.False(BigRational.TryParse("1/2/3", CultureInfo.InvariantCulture, out _));
        Assert.False(BigRational.TryParse(null, CultureInfo.InvariantCulture, out _));
    }

    [Fact]
    public void Parse_ThrowsOnMalformed()
    {
        Assert.Throws<FormatException>(() => BigRational.Parse("garbage", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Parse_RoundtripsToString()
    {
        var original = new BigRational(355, 113);
        var roundtripped = BigRational.Parse(
            original.ToString(CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture);
        Assert.Equal(original, roundtripped);
    }
}
