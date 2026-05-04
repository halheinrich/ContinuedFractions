namespace ContinuedFractions.Tests;

public class CFIdentifierTests
{
    /// <summary>
    /// Test-only stub that delegates the identification decision to a
    /// caller-supplied function. Used to verify the abstract base's
    /// contract without committing the library to any concrete
    /// identifier yet.
    /// </summary>
    private sealed class StubIdentifier : CFIdentifier
    {
        private readonly Func<ContinuedFraction, IdentificationResult> _impl;

        public StubIdentifier(Func<ContinuedFraction, IdentificationResult> impl)
        {
            _impl = impl;
        }

        protected override IdentificationResult TryIdentifyCore(ContinuedFraction cf)
            => _impl(cf);
    }

    [Fact]
    public void TryIdentify_DispatchesToCore_WhenMatched()
    {
        var id = new StubIdentifier(_ => IdentificationResult.Matched("test", 3));
        var cf = new ContinuedFraction(1);

        var result = id.TryIdentify(cf);

        Assert.True(result.Match);
        Assert.Equal("test", result.Identification);
        Assert.Equal(3, result.Depth);
    }

    [Fact]
    public void TryIdentify_DispatchesToCore_WhenNotMatched()
    {
        var id = new StubIdentifier(_ => IdentificationResult.NotMatched(50));
        var cf = new ContinuedFraction(1);

        var result = id.TryIdentify(cf);

        Assert.False(result.Match);
        Assert.Null(result.Identification);
        Assert.Equal(50, result.Depth);
    }

    [Fact]
    public void TryIdentify_RejectsNullCf_BeforeDispatchingToCore()
    {
        var coreCalled = false;
        var id = new StubIdentifier(_ =>
        {
            coreCalled = true;
            return IdentificationResult.Matched("should-not-reach", 0);
        });

        Assert.Throws<ArgumentNullException>(() => id.TryIdentify(null!));
        Assert.False(coreCalled);
    }

    [Fact]
    public void TryIdentify_PassesCfThrough()
    {
        ContinuedFraction? captured = null;
        var id = new StubIdentifier(cf =>
        {
            captured = cf;
            return IdentificationResult.NotMatched(0);
        });
        var input = new ContinuedFraction(1, 2, 3);

        id.TryIdentify(input);

        Assert.Same(input, captured);
    }
}
