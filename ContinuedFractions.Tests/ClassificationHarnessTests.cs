using System.Numerics;
using ContinuedFractions.Generators;

namespace ContinuedFractions.Tests;

/// <summary>
/// Demonstrates the harness pattern: a set of continued fractions
/// dispatched through a list of <see cref="CFIdentifier"/> instances,
/// where the first identifier to claim a match wins.
/// </summary>
public class ClassificationHarnessTests
{
    [Fact]
    public void Harness_ClassifiesEachCfWithFirstMatchingIdentifier()
    {
        // Test set: each CF paired with what it should be identified as
        // (or null if no current identifier should match).
        var cases = new (string Label, ContinuedFraction Cf, string? ExpectedIdentification)[]
        {
            ("φ",          new ContinuedFraction(new Phi()),          null),
            ("√2",         new ContinuedFraction(new Sqrt2()),        "√2"),
            ("e",          new ContinuedFraction(new EulersNumber()), null),
            ("√5",
                new ContinuedFraction(
                    new FuncCFCoefficientGenerator(
                        "√5", i => i == 0 ? new BigInteger(2) : new BigInteger(4))),
                "√5"),
            ("[3; 7]",     new ContinuedFraction(3, 7),               null),
        };

        // Available identifiers, evaluated in order.
        var identifiers = new CFIdentifier[]
        {
            new SquareRootIdentifier(),
            // Future identifiers (rational-multiple-of-e, periodic-CF, ...)
            // would slot in here.
        };

        // Run the harness and collect classifications.
        var classifications = new Dictionary<string, IdentificationResult?>();
        foreach (var (label, cf, _) in cases)
        {
            IdentificationResult? hit = null;
            foreach (var identifier in identifiers)
            {
                var result = identifier.TryIdentify(cf);
                if (result.Match)
                {
                    hit = result;
                    break;
                }
            }
            classifications[label] = hit;
        }

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
