namespace HalHeinrich.Numerics.ContinuedFractions.Tests;

public class IdentificationResultTests
{
    // ---------- Matched ----------

    [Fact]
    public void Matched_SetsMatchTrue()
    {
        var r = IdentificationResult.Matched("√2", 5);
        Assert.True(r.Match);
        Assert.Equal("√2", r.Identification);
        Assert.Equal(5, r.Depth);
    }

    [Fact]
    public void Matched_RejectsNullIdentification()
    {
        Assert.Throws<ArgumentNullException>(() =>
            IdentificationResult.Matched(null!, 0));
    }

    [Fact]
    public void Matched_RejectsNegativeDepth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IdentificationResult.Matched("x", -1));
    }

    [Fact]
    public void Matched_AcceptsZeroDepth()
    {
        var r = IdentificationResult.Matched("x", 0);
        Assert.Equal(0, r.Depth);
    }

    // ---------- NotMatched ----------

    [Fact]
    public void NotMatched_SetsMatchFalseAndIdentificationNull()
    {
        var r = IdentificationResult.NotMatched(100);
        Assert.False(r.Match);
        Assert.Null(r.Identification);
        Assert.Equal(100, r.Depth);
    }

    [Fact]
    public void NotMatched_RejectsNegativeDepth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IdentificationResult.NotMatched(-1));
    }

    // ---------- equality ----------

    [Fact]
    public void Equality_SameMatchedValuesAreEqual()
    {
        var a = IdentificationResult.Matched("φ", 7);
        var b = IdentificationResult.Matched("φ", 7);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentDepthsAreNotEqual()
    {
        var a = IdentificationResult.Matched("φ", 7);
        var b = IdentificationResult.Matched("φ", 8);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equality_DifferentIdentificationsAreNotEqual()
    {
        var a = IdentificationResult.Matched("φ", 7);
        var b = IdentificationResult.Matched("√2", 7);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equality_MatchedAndNotMatchedAreNotEqual()
    {
        var a = IdentificationResult.Matched("φ", 7);
        var b = IdentificationResult.NotMatched(7);
        Assert.NotEqual(a, b);
    }
}
