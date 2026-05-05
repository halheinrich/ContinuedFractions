using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions.Tests;

public class LaneTests
{
    // ---------- construction: validation ----------

    [Fact]
    public void Constructor_AcceptsValidConst()
    {
        var lane = new Lane(3, Operation.Const, 0);

        Assert.Equal(new BigInteger(3), lane.InitialValue);
        Assert.Equal(Operation.Const, lane.Operation);
    }

    [Fact]
    public void Constructor_AcceptsValidPlus()
    {
        var lane = new Lane(2, Operation.Plus, 2);

        Assert.Equal(Operation.Plus, lane.Operation);
        Assert.Equal(new BigInteger(2), lane.Operand);
    }

    [Fact]
    public void Constructor_AcceptsValidMultiply()
    {
        var lane = new Lane(1, Operation.Multiply, 2);

        Assert.Equal(Operation.Multiply, lane.Operation);
        Assert.Equal(new BigInteger(2), lane.Operand);
    }

    [Fact]
    public void Constructor_RejectsZeroInitialValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Lane(0, Operation.Const, 0));
    }

    [Fact]
    public void Constructor_RejectsNegativeInitialValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Lane(-1, Operation.Const, 0));
    }

    [Fact]
    public void Constructor_RejectsPlusZero_AsNoOp()
    {
        Assert.Throws<ArgumentException>(() =>
            new Lane(2, Operation.Plus, 0));
    }

    [Fact]
    public void Constructor_RejectsPlusNegative_WouldDecayBelowOne()
    {
        Assert.Throws<ArgumentException>(() =>
            new Lane(2, Operation.Plus, -1));
    }

    [Fact]
    public void Constructor_RejectsMultiplyOne_AsNoOp()
    {
        Assert.Throws<ArgumentException>(() =>
            new Lane(2, Operation.Multiply, 1));
    }

    [Fact]
    public void Constructor_RejectsMultiplyZero_WouldZeroOutAfterFirstVisit()
    {
        Assert.Throws<ArgumentException>(() =>
            new Lane(2, Operation.Multiply, 0));
    }

    [Fact]
    public void Constructor_RejectsMultiplyNegative_WouldAlternateSign()
    {
        Assert.Throws<ArgumentException>(() =>
            new Lane(2, Operation.Multiply, -2));
    }

    [Fact]
    public void Constructor_RejectsUndefinedOperation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Lane(1, (Operation)42, 0));
    }

    // ---------- factory methods ----------

    [Fact]
    public void Const_FactoryMatchesConstructor()
    {
        var a = Lane.Const(5);
        var b = new Lane(5, Operation.Const, 0);

        Assert.Equal(b, a);
    }

    [Fact]
    public void Plus_FactoryMatchesConstructor()
    {
        var a = Lane.Plus(2, 3);
        var b = new Lane(2, Operation.Plus, 3);

        Assert.Equal(b, a);
    }

    [Fact]
    public void Multiply_FactoryMatchesConstructor()
    {
        var a = Lane.Multiply(1, 2);
        var b = new Lane(1, Operation.Multiply, 2);

        Assert.Equal(b, a);
    }

    // ---------- ValueAt ----------

    [Theory]
    [InlineData(0, 7)]
    [InlineData(1, 7)]
    [InlineData(50, 7)]
    public void Const_EmitsInitialValueOnEveryVisit(int visit, long expected)
    {
        var lane = Lane.Const(7);
        Assert.Equal(new BigInteger(expected), lane.ValueAt(visit));
    }

    [Theory]
    [InlineData(0, 2)]   // 2 + 0·2
    [InlineData(1, 4)]   // 2 + 1·2
    [InlineData(2, 6)]   // 2 + 2·2
    [InlineData(5, 12)]  // 2 + 5·2
    public void Plus_EmitsArithmeticProgression(int visit, long expected)
    {
        var lane = Lane.Plus(2, 2);
        Assert.Equal(new BigInteger(expected), lane.ValueAt(visit));
    }

    [Theory]
    [InlineData(0, 1)]   // 1 · 2⁰
    [InlineData(1, 2)]   // 1 · 2¹
    [InlineData(2, 4)]   // 1 · 2²
    [InlineData(10, 1024)]
    public void Multiply_EmitsGeometricProgression(int visit, long expected)
    {
        var lane = Lane.Multiply(1, 2);
        Assert.Equal(new BigInteger(expected), lane.ValueAt(visit));
    }

    [Fact]
    public void ValueAt_RejectsNegativeVisit()
    {
        var lane = Lane.Const(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => lane.ValueAt(-1));
    }

    // ---------- value equality ----------

    [Fact]
    public void Equals_BasedOnAllFields()
    {
        var a = new Lane(2, Operation.Plus, 3);
        var b = new Lane(2, Operation.Plus, 3);
        var c = new Lane(2, Operation.Plus, 5);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    // ---------- ToString ----------

    [Fact]
    public void ToString_RendersConst()
    {
        Assert.Equal("Const(3)", Lane.Const(3).ToString());
    }

    [Fact]
    public void ToString_RendersPlus()
    {
        Assert.Equal("Plus(2, +2)", Lane.Plus(2, 2).ToString());
    }

    [Fact]
    public void ToString_RendersMultiply()
    {
        Assert.Equal("Multiply(1, ×2)", Lane.Multiply(1, 2).ToString());
    }
}
