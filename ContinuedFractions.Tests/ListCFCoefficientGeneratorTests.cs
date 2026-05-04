using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions.Tests;

public class ListCFCoefficientGeneratorTests
{
    // ---------- construction ----------

    [Fact]
    public void Constructor_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ListCFCoefficientGenerator(null!));
    }

    [Fact]
    public void Constructor_AcceptsEmptyList()
    {
        var gen = new ListCFCoefficientGenerator(Array.Empty<BigInteger>());
        Assert.Equal(0, gen.Length);
    }

    // ---------- Length ----------

    [Fact]
    public void Length_MatchesListCount()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 1, 2, 3 });
        Assert.Equal(3, gen.Length);
    }

    // ---------- Items ----------

    [Fact]
    public void Items_ReturnsConstructorList()
    {
        var list = new BigInteger[] { 1, 2, 3 };
        var gen = new ListCFCoefficientGenerator(list);
        Assert.Equal(list, gen.Items);
    }

    // ---------- indexer ----------

    [Fact]
    public void Indexer_ReturnsValuesInOrder()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 5, 7, 11 });
        Assert.Equal(new BigInteger(5), gen[0]);
        Assert.Equal(new BigInteger(7), gen[1]);
        Assert.Equal(new BigInteger(11), gen[2]);
    }

    [Fact]
    public void Indexer_RejectsNegativeIndex()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 1, 2 });
        Assert.Throws<ArgumentOutOfRangeException>(() => gen[-1]);
    }

    [Fact]
    public void Indexer_RejectsIndexAtCount()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 1, 2 });
        Assert.Throws<ArgumentOutOfRangeException>(() => gen[2]);
    }

    [Fact]
    public void Indexer_RejectsIndexBeyondCount()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 1, 2 });
        Assert.Throws<ArgumentOutOfRangeException>(() => gen[100]);
    }

    [Fact]
    public void Indexer_OnEmptyListAlwaysThrows()
    {
        var gen = new ListCFCoefficientGenerator(Array.Empty<BigInteger>());
        Assert.Throws<ArgumentOutOfRangeException>(() => gen[0]);
    }

    // ---------- iteration ----------

    [Fact]
    public void Iteration_YieldsAllInOrder()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 1, 2, 3 });
        Assert.Equal(
            new[] { new BigInteger(1), new BigInteger(2), new BigInteger(3) },
            gen.ToList());
    }

    [Fact]
    public void Iteration_EmptyListYieldsNothing()
    {
        var gen = new ListCFCoefficientGenerator(Array.Empty<BigInteger>());
        Assert.Empty(gen);
    }

    [Fact]
    public void Iteration_TerminatesAtListEnd()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 1, 2, 3 });
        Assert.Equal(3, gen.Count());
    }

    // ---------- ToString ----------

    [Fact]
    public void ToString_EmptyListReturnsEmptyBrackets()
    {
        var gen = new ListCFCoefficientGenerator(Array.Empty<BigInteger>());
        Assert.Equal("[]", gen.ToString());
    }

    [Fact]
    public void ToString_SingleElementOmitsSemicolon()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 7 });
        Assert.Equal("[7]", gen.ToString());
    }

    [Fact]
    public void ToString_TwoElementsUsesSemicolon()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 1, 2 });
        Assert.Equal("[1; 2]", gen.ToString());
    }

    [Fact]
    public void ToString_MultipleElementsUsesSemicolonAndCommas()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { 1, 2, 3, 4 });
        Assert.Equal("[1; 2, 3, 4]", gen.ToString());
    }

    [Fact]
    public void ToString_HandlesNegativeIntegerPart()
    {
        var gen = new ListCFCoefficientGenerator(new BigInteger[] { -5, 1, 2 });
        Assert.Equal("[-5; 1, 2]", gen.ToString());
    }
}
