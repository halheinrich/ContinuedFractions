using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions.Tests;

public class EFamilyShapeIdentifierTests
{
    // ---------- construction ----------

    [Fact]
    public void Constructor_RejectsNonPositiveDepth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EFamilyShapeIdentifier(maxComparisonDepth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EFamilyShapeIdentifier(maxComparisonDepth: -1));
    }

    // ---------- catalogue CFs are all matched (with parameterised names) ----------

    [Fact]
    public void Identifies_eCatalogueCf_AsEulerShifted()
    {
        var cf = new ContinuedFraction(Patterns.EulersNumber());
        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("e", result.Identification);
    }

    [Fact]
    public void Identifies_eMinus1_CatalogueCf_AsEulerShifted()
    {
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 1 },
                new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("e − 1", result.Identification);
    }

    [Fact]
    public void Identifies_OneOverE_CatalogueCf_AsReciprocalEulerShifted()
    {
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0, 2 },
                new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("1/e", result.Identification);
    }

    [Fact]
    public void Identifies_TanhHalf_AliasedToAlgebraicForm()
    {
        // tanh(1/2) renames to "(e − 1)/(e + 1)" via the alias table.
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0 },
                new[] { Lane.Plus(2, 4) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("(e − 1)/(e + 1)", result.Identification);
    }

    [Fact]
    public void Identifies_CothHalf_AliasedToAlgebraicForm()
    {
        // coth(1/2) renames to "(e + 1)/(e − 1)" via the alias table.
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 2 },
                new[] { Lane.Plus(6, 4) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("(e + 1)/(e − 1)", result.Identification);
    }

    // ---------- additional family members not in the catalogue ----------

    [Theory]
    [InlineData(1, "tanh(1/1)")]
    [InlineData(3, "tanh(1/3)")]
    [InlineData(5, "tanh(1/5)")]
    [InlineData(7, "tanh(1/7)")]
    public void Identifies_TanhOneOverM_ForVariousM(int m, string expectedName)
    {
        // tanh(1/m): CF [0; m, 3m, 5m, 7m, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0 },
                new[] { Lane.Plus(m, 2 * m) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal(expectedName, result.Identification);
    }

    [Theory]
    [InlineData(1, "coth(1/1)")]
    [InlineData(3, "coth(1/3)")]
    [InlineData(5, "coth(1/5)")]
    public void Identifies_CothOneOverM_ForVariousM(int m, string expectedName)
    {
        // coth(1/m): CF [m; 3m, 5m, 7m, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { m },
                new[] { Lane.Plus(3 * m, 2 * m) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal(expectedName, result.Identification);
    }

    [Theory]
    [InlineData(2, "e")]            // k=2 → n=0 → "e"
    [InlineData(1, "e − 1")]        // k=1 → n=-1 → "e − 1"
    [InlineData(3, "e + 1")]        // k=3 → n=1 → "e + 1"
    [InlineData(4, "e + 2")]
    [InlineData(0, "e − 2")]
    public void Identifies_EulerShifted_ForVariousK(int k, string expectedName)
    {
        // [k; 1, 2, 1, 1, 4, 1, 1, 6, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { k },
                new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal(expectedName, result.Identification);
    }

    [Theory]
    [InlineData(2, "1/e")]
    [InlineData(1, "1/(e − 1)")]
    [InlineData(3, "1/(e + 1)")]
    [InlineData(4, "1/(e + 2)")]
    public void Identifies_ReciprocalEulerShifted_ForVariousK(int k, string expectedName)
    {
        // [0, k; 1, 2, 1, 1, 4, 1, 1, 6, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0, k },
                new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal(expectedName, result.Identification);
    }

    // ---------- negative cases ----------

    [Fact]
    public void Rejects_Phi()
    {
        var result = new EFamilyShapeIdentifier().TryIdentify(
            new ContinuedFraction(Patterns.Phi()));

        Assert.False(result.Match);
    }

    [Fact]
    public void Rejects_Sqrt2()
    {
        var result = new EFamilyShapeIdentifier().TryIdentify(
            new ContinuedFraction(Patterns.Sqrt2()));

        Assert.False(result.Match);
    }

    [Fact]
    public void Rejects_FiniteRational()
    {
        var result = new EFamilyShapeIdentifier().TryIdentify(
            new ContinuedFraction(Patterns.Rational(22, 7)));

        Assert.False(result.Match);
    }

    [Fact]
    public void Rejects_NearMissShape_OneCoefficientOff()
    {
        // tanh(1/2) shape but with a₃ = 11 instead of 14 — should fail
        // verification at position 3.
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0, 2, 6, 10, 11 },
                new[] { Lane.Plus(18, 4) }));

        var result = new EFamilyShapeIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
    }
}
