namespace HalHeinrich.Numerics.ContinuedFractions.Tests;

public class QuadraticIdentificationResultTests
{
    // ---------- Matched ----------

    [Fact]
    public void Matched_PopulatesFields()
    {
        var qi = new QuadraticIrrational(7, 2, 3);
        var result = QuadraticIdentificationResult.Matched(qi, 5);

        Assert.True(result.Match);
        Assert.Equal(qi, result.Value);
        Assert.Equal(5, result.Depth);
    }

    [Fact]
    public void Matched_RejectsNegativeDepth()
    {
        var qi = new QuadraticIrrational(7, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            QuadraticIdentificationResult.Matched(qi, -1));
    }

    [Fact]
    public void Matched_AcceptsZeroDepth()
    {
        var qi = new QuadraticIrrational(2, 0, 1);
        var result = QuadraticIdentificationResult.Matched(qi, 0);

        Assert.True(result.Match);
        Assert.Equal(0, result.Depth);
    }

    // ---------- NotMatched ----------

    [Fact]
    public void NotMatched_PopulatesFields()
    {
        var result = QuadraticIdentificationResult.NotMatched(50);

        Assert.False(result.Match);
        Assert.Null(result.Value);
        Assert.Equal(50, result.Depth);
    }

    [Fact]
    public void NotMatched_RejectsNegativeDepth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            QuadraticIdentificationResult.NotMatched(-1));
    }

    [Fact]
    public void NotMatched_AcceptsZeroDepth()
    {
        var result = QuadraticIdentificationResult.NotMatched(0);

        Assert.False(result.Match);
        Assert.Equal(0, result.Depth);
    }
}
