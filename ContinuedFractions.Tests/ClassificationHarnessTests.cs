using System.Numerics;
using ContinuedFractions.Generators;
using Xunit.Abstractions;

namespace ContinuedFractions.Tests;

/// <summary>
/// Demonstrates the harness pattern: a set of continued fractions
/// dispatched through a list of <see cref="CFIdentifier"/> instances,
/// where the first identifier to claim a match wins.
/// </summary>
public class ClassificationHarnessTests
{
    private readonly ITestOutputHelper _output;

    public ClassificationHarnessTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Harness_ClassifiesEachCfWithFirstMatchingIdentifier()
    {
        // Test set: each CF paired with what it should be identified as
        // (or null if no current identifier should match).
        var cases = new (string Label, ContinuedFraction Cf, string? ExpectedIdentification)[]
        {
            ("φ",      new ContinuedFraction(Patterns.Phi()),          "(√5 + 1)/2"),
            ("√2",     new ContinuedFraction(Patterns.Sqrt2()),        "√2"),
            ("e",      new ContinuedFraction(Patterns.EulersNumber()), null),
            ("√5",
                new ContinuedFraction(
                    new PatternCFCoefficientGenerator(
                        new BigInteger[] { 2 },
                        new[] { Lane.Const(4) })),
                "√5"),
            ("[3; 7]", new ContinuedFraction(Patterns.Rational(22, 7)), null),
        };

        // Available identifiers, evaluated in order.
        var identifiers = new CFIdentifier[]
        {
            new QuadraticIrrationalIdentifier(),
            // Future identifiers (rational-multiple-of-e, periodic-CF, ...)
            // would slot in here.
        };

        _output.WriteLine(
            $"Classification harness — {cases.Length} CFs against {identifiers.Length} identifier(s):");
        _output.WriteLine(string.Empty);

        // Run the harness and collect classifications.
        var classifications = new Dictionary<string, IdentificationResult?>();
        foreach (var (label, cf, _) in cases)
        {
            IdentificationResult? hit = null;
            IdentificationResult lastResult = default;
            foreach (var identifier in identifiers)
            {
                lastResult = identifier.TryIdentify(cf);
                if (lastResult.Match)
                {
                    hit = lastResult;
                    break;
                }
            }
            classifications[label] = hit;

            if (hit is { } matched)
            {
                _output.WriteLine(
                    $"  {label,-8} → SOLVED as {matched.Identification} (depth {matched.Depth})");
            }
            else
            {
                _output.WriteLine(
                    $"  {label,-8} → unsolved (gave up at depth {lastResult.Depth})");
            }
        }

        _output.WriteLine(string.Empty);

        // Assert each CF was classified (or not) as expected.
        foreach (var (label, _, expected) in cases)
        {
            var actual = classifications[label];
            if (expected is null)
            {
                Assert.Null(actual);
            }
            else
            {
                Assert.NotNull(actual);
                Assert.True(actual.Value.Match);
                Assert.Equal(expected, actual.Value.Identification);
            }
        }
    }
}
