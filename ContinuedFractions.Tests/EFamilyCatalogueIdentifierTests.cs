using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions.Tests;

public class EFamilyCatalogueIdentifierTests
{
    // ---------- construction ----------

    [Fact]
    public void Constructor_RejectsNonPositiveDepth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EFamilyCatalogueIdentifier(maxComparisonDepth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EFamilyCatalogueIdentifier(maxComparisonDepth: -1));
    }

    private static readonly string[] ExpectedCatalogueOrder =
    {
        "e", "e − 1", "1/e", "(e − 1)/(e + 1)", "(e + 1)/(e − 1)",
    };

    [Fact]
    public void Catalogue_HasFiveEntries_InExpectedOrder()
    {
        var identifier = new EFamilyCatalogueIdentifier();

        var names = identifier.Catalogue.Select(e => e.Name).ToArray();
        Assert.Equal(ExpectedCatalogueOrder, names);
    }

    // ---------- identifies each catalogue entry by its own CF ----------

    [Fact]
    public void Identifies_e_FromItsCf()
    {
        var cf = new ContinuedFraction(Patterns.EulersNumber());
        var result = new EFamilyCatalogueIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("e", result.Identification);
    }

    [Fact]
    public void Identifies_eMinus1_FromItsCf()
    {
        // [1; 1, 2, 1, 1, 4, 1, 1, 6, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 1 },
                new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) }));

        var result = new EFamilyCatalogueIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("e − 1", result.Identification);
    }

    [Fact]
    public void Identifies_OneOverE_FromItsCf()
    {
        // [0; 2, 1, 2, 1, 1, 4, 1, 1, 6, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0, 2 },
                new[] { Lane.Const(1), Lane.Plus(2, 2), Lane.Const(1) }));

        var result = new EFamilyCatalogueIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("1/e", result.Identification);
    }

    [Fact]
    public void Identifies_TanhHalf_FromItsCf()
    {
        // (e − 1)/(e + 1) = [0; 2, 6, 10, 14, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0 },
                new[] { Lane.Plus(2, 4) }));

        var result = new EFamilyCatalogueIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("(e − 1)/(e + 1)", result.Identification);
    }

    [Fact]
    public void Identifies_CothHalf_FromItsCf()
    {
        // (e + 1)/(e − 1) = [2; 6, 10, 14, …]
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 2 },
                new[] { Lane.Plus(6, 4) }));

        var result = new EFamilyCatalogueIdentifier().TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("(e + 1)/(e − 1)", result.Identification);
    }

    // ---------- rejects out-of-family CFs ----------

    [Fact]
    public void Rejects_Phi()
    {
        var result = new EFamilyCatalogueIdentifier().TryIdentify(
            new ContinuedFraction(Patterns.Phi()));

        Assert.False(result.Match);
    }

    [Fact]
    public void Rejects_Sqrt2()
    {
        var result = new EFamilyCatalogueIdentifier().TryIdentify(
            new ContinuedFraction(Patterns.Sqrt2()));

        Assert.False(result.Match);
    }

    [Fact]
    public void Rejects_FiniteRational()
    {
        var result = new EFamilyCatalogueIdentifier().TryIdentify(
            new ContinuedFraction(Patterns.Rational(22, 7)));

        Assert.False(result.Match);
    }

    [Fact]
    public void Rejects_TanhOneThird_NotInCatalogue()
    {
        // tanh(1/3) has CF [0; 3, 9, 15, 21, …] — not catalogued.
        var cf = new ContinuedFraction(
            new PatternCFCoefficientGenerator(
                new BigInteger[] { 0 },
                new[] { Lane.Plus(3, 6) }));

        var result = new EFamilyCatalogueIdentifier().TryIdentify(cf);

        Assert.False(result.Match);
    }
}
