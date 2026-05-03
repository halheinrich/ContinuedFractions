using System.Numerics;

namespace ContinuedFractions.Tests;

public class FuncCFCoefficientGeneratorTests
{
    // ---------- construction ----------

    [Fact]
    public void Constructor_RejectsNullDescription()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FuncCFCoefficientGenerator(null!, _ => BigInteger.One));
    }

    [Fact]
    public void Constructor_RejectsNullGenerator()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FuncCFCoefficientGenerator("φ", null!));
    }

    [Fact]
    public void Constructor_RejectsNegativeLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FuncCFCoefficientGenerator("φ", _ => BigInteger.One, length: -1));
    }

    [Fact]
    public void Constructor_AcceptsZeroLength()
    {
        var gen = new FuncCFCoefficientGenerator("empty", _ => BigInteger.One, length: 0);
        Assert.Equal(0, gen.Length);
    }

    // ---------- Length ----------

    [Fact]
    public void Length_NullByDefault()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        Assert.Null(gen.Length);
    }

    [Fact]
    public void Length_ReturnsConstructorValue()
    {
        var gen = new FuncCFCoefficientGenerator("[1; 2, 3]", i => i + 1, length: 3);
        Assert.Equal(3, gen.Length);
    }

    // ---------- indexer ----------

    [Fact]
    public void Indexer_ReturnsGeneratorValue_Constant()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        Assert.Equal(BigInteger.One, gen[0]);
        Assert.Equal(BigInteger.One, gen[1]);
        Assert.Equal(BigInteger.One, gen[1000]);
    }

    [Fact]
    public void Indexer_PassesIndexToGenerator()
    {
        var gen = new FuncCFCoefficientGenerator("identity", i => new BigInteger(i));
        Assert.Equal(new BigInteger(0), gen[0]);
        Assert.Equal(new BigInteger(7), gen[7]);
    }

    [Fact]
    public void Indexer_RejectsNegativeIndex()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        Assert.Throws<ArgumentOutOfRangeException>(() => gen[-1]);
    }

    [Fact]
    public void Indexer_RejectsIndexAtLength()
    {
        var gen = new FuncCFCoefficientGenerator("[1; 2, 3]", i => i + 1, length: 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => gen[3]);
    }

    [Fact]
    public void Indexer_RejectsIndexBeyondLength()
    {
        var gen = new FuncCFCoefficientGenerator("[1; 2, 3]", i => i + 1, length: 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => gen[100]);
    }

    [Fact]
    public void Indexer_AllowsIndicesBelowLength()
    {
        var gen = new FuncCFCoefficientGenerator("[1; 2, 3]", i => i + 1, length: 3);
        Assert.Equal(new BigInteger(1), gen[0]);
        Assert.Equal(new BigInteger(2), gen[1]);
        Assert.Equal(new BigInteger(3), gen[2]);
    }

    // ---------- iteration ----------

    [Fact]
    public void Iteration_FiniteGeneratorTerminatesAtLength()
    {
        var gen = new FuncCFCoefficientGenerator("[1; 2, 3]", i => i + 1, length: 3);
        var values = gen.ToList();
        Assert.Equal(
            new[] { new BigInteger(1), new BigInteger(2), new BigInteger(3) },
            values);
    }

    [Fact]
    public void Iteration_UnboundedGeneratorYieldsIndefinitely()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        var first10 = gen.Take(10).ToList();
        Assert.Equal(10, first10.Count);
        Assert.All(first10, v => Assert.Equal(BigInteger.One, v));
    }

    [Fact]
    public void Iteration_PassesIndicesInOrder()
    {
        var gen = new FuncCFCoefficientGenerator("identity", i => new BigInteger(i));
        var first5 = gen.Take(5).ToList();
        Assert.Equal(
            new[]
            {
                new BigInteger(0),
                new BigInteger(1),
                new BigInteger(2),
                new BigInteger(3),
                new BigInteger(4),
            },
            first5);
    }

    [Fact]
    public void Iteration_EmptyLengthYieldsNothing()
    {
        var gen = new FuncCFCoefficientGenerator("empty", _ => BigInteger.One, length: 0);
        Assert.Empty(gen);
    }

    // ---------- ToString ----------

    [Fact]
    public void ToString_ReturnsDescription()
    {
        var gen = new FuncCFCoefficientGenerator("φ", _ => BigInteger.One);
        Assert.Equal("φ", gen.ToString());
    }
}
