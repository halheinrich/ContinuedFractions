using System.Numerics;

namespace ContinuedFractions.Tests;

public class ContinuedFractionGeneratorCtorTests
{
    // ---------- construction ----------

    [Fact]
    public void Constructor_RejectsNullGenerator()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ContinuedFraction((CFCoefficientGenerator)null!));
    }

    [Fact]
    public void Constructor_RejectsZeroLengthGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("empty", _ => BigInteger.One, length: 0);
        Assert.Throws<ArgumentException>(() => new ContinuedFraction(gen));
    }

    [Fact]
    public void Constructor_AcceptsUnboundedGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        _ = new ContinuedFraction(gen);
    }

    [Fact]
    public void Constructor_AcceptsFiniteGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("[1; 2]", i => i + 1, length: 2);
        _ = new ContinuedFraction(gen);
    }

    [Fact]
    public void Constructor_AcceptsLengthOneGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("[7]", _ => new BigInteger(7), length: 1);
        _ = new ContinuedFraction(gen);
    }

    // ---------- Generator property ----------

    [Fact]
    public void Generator_ReturnsConstructorGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        var cf = new ContinuedFraction(gen);
        Assert.Same(gen, cf.Generator);
    }

    [Fact]
    public void Generator_ForListBuiltCfReturnsListCFCoefficientGenerator()
    {
        var cf = new ContinuedFraction(1, 2, 3);
        Assert.IsType<ListCFCoefficientGenerator>(cf.Generator);
    }

    // ---------- IntegerPart on generator-backed CFs ----------

    [Fact]
    public void IntegerPart_PullsIndexZeroFromGenerator()
    {
        var gen = new FuncCFCoefficientGenerator(
            "[7; 1, 1, ...]",
            i => i == 0 ? new BigInteger(7) : BigInteger.One);
        var cf = new ContinuedFraction(gen);
        Assert.Equal(new BigInteger(7), cf.IntegerPart);
    }

    // ---------- IsInteger ----------

    [Fact]
    public void IsInteger_FalseForUnboundedGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        var cf = new ContinuedFraction(gen);
        Assert.False(cf.IsInteger);
    }

    [Fact]
    public void IsInteger_TrueForLengthOneGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("[7]", _ => new BigInteger(7), length: 1);
        var cf = new ContinuedFraction(gen);
        Assert.True(cf.IsInteger);
    }

    [Fact]
    public void IsInteger_FalseForLengthGreaterThanOneGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("[1; 2]", i => i + 1, length: 2);
        var cf = new ContinuedFraction(gen);
        Assert.False(cf.IsInteger);
    }

    // ---------- Coefficients property ----------

    [Fact]
    public void Coefficients_ThrowsForGeneratorBackedCf()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        var cf = new ContinuedFraction(gen);
        Assert.Throws<InvalidOperationException>(() => cf.Coefficients);
    }

    [Fact]
    public void Coefficients_StillWorksForListBackedCf()
    {
        var cf = new ContinuedFraction(1, 2, 3);
        Assert.Equal(3, cf.Coefficients.Count);
    }
}
