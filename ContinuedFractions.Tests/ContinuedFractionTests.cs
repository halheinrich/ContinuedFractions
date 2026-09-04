using System.Numerics;
using HalHeinrich.Numerics.ContinuedFractions.Generators;

namespace HalHeinrich.Numerics.ContinuedFractions.Tests;

public class ContinuedFractionTests
{
    // ---------- construction ----------

    [Fact]
    public void Constructor_RejectsNullPattern()
    {
        Assert.Throws<ArgumentNullException>(() => new ContinuedFraction(null!));
    }

    [Fact]
    public void Constructor_StoresPatternAsGenerator()
    {
        var pattern = Patterns.Rational(22, 7);
        var cf = new ContinuedFraction(pattern);

        Assert.Same(pattern, cf.Generator);
    }

    [Fact]
    public void Constructor_AcceptsRationalPattern()
    {
        // 22/7 = [3; 7].
        var cf = new ContinuedFraction(Patterns.Rational(22, 7));

        Assert.Equal(
            new BigInteger[] { 3, 7 },
            cf.Generator.ToList());
    }

    [Fact]
    public void Constructor_AcceptsLargeRationalCoefficient()
    {
        var huge = BigInteger.Pow(10, 40);
        var pattern = new PatternCFCoefficientGenerator(
            new BigInteger[] { 0, huge },
            Array.Empty<Lane>());
        var cf = new ContinuedFraction(pattern);

        Assert.Equal(huge, cf.Generator[1]);
    }

    [Fact]
    public void Constructor_AllowsNegativeIntegerPart()
    {
        var pattern = new PatternCFCoefficientGenerator(
            new BigInteger[] { -2, 1, 4 },
            Array.Empty<Lane>());
        var cf = new ContinuedFraction(pattern);

        Assert.Equal(new BigInteger(-2), cf.IntegerPart);
    }

    // ---------- properties ----------

    [Fact]
    public void IntegerPart_ReturnsFirstCoefficient()
    {
        Assert.Equal(
            new BigInteger(3),
            new ContinuedFraction(Patterns.Rational(22, 7)).IntegerPart);

        Assert.Equal(
            new BigInteger(-5),
            new ContinuedFraction(Patterns.Rational(-5, 1)).IntegerPart);
    }

    // ---------- formatting ----------

    [Fact]
    public void ToString_DelegatesToGenerator()
    {
        var cf = new ContinuedFraction(Patterns.Rational(22, 7));
        Assert.Equal(cf.Generator.ToString(), cf.ToString());
    }

    [Fact]
    public void ToString_FiniteRationalCfReturnsBracketNotation()
    {
        var cf = new ContinuedFraction(Patterns.Rational(22, 7));
        Assert.Equal("[3, 7]", cf.ToString());
    }
}
