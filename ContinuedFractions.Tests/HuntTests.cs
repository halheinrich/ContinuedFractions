using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using HalHeinrich.Numerics.ContinuedFractions.Generators;
using Xunit.Abstractions;

namespace HalHeinrich.Numerics.ContinuedFractions.Tests;

/// <summary>
/// Discovery-style tests: enumerate <see cref="PatternCFCoefficientGenerator"/>
/// constructor inputs in level-organised sets, dispatch each through
/// the identifier chain (gated by the structural classifiers), and log
/// every result.
/// </summary>
public class HuntTests
{
    private readonly ITestOutputHelper _output;

    public HuntTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Hunt_SingleLanePatterns_LevelsOneToThree_ShowEachAndSolution()
    {
        var quadratic = new QuadraticIrrationalIdentifier();
        var eFamily = new EFamilyShapeIdentifier();

        var rationalCount = 0;
        var quadraticSolved = 0;
        var eFamilySolved = 0;
        var candidateCount = 0;
        var rows = new List<(int Level, string Label, string CfPrefix, string Verdict)>();

        foreach (var (level, pattern) in EnumerateLevels(maxLevel: 3))
        {
            var label = DescribePattern(pattern);
            var cfPrefix = FormatCfPrefix(pattern, length: 8);

            string verdict;
            if (pattern.IsRational)
            {
                verdict = "rational";
                rationalCount++;
            }
            else if (pattern.IsQuadraticIrrational)
            {
                var cf = new ContinuedFraction(pattern);
                var r = quadratic.TryIdentify(cf);
                if (r.Match)
                {
                    verdict = $"Quadratic → {r.Identification}";
                    quadraticSolved++;
                }
                else
                {
                    verdict = "Quadratic over budget";
                    candidateCount++;
                }
            }
            else
            {
                var cf = new ContinuedFraction(pattern);
                var r = eFamily.TryIdentify(cf);
                if (r.Match)
                {
                    verdict = $"EFamily → {r.Identification}";
                    eFamilySolved++;
                }
                else
                {
                    verdict = "candidate";
                    candidateCount++;
                }
            }

            rows.Add((level, label, cfPrefix, verdict));
        }

        var report = new StringBuilder();
        var inv = CultureInfo.InvariantCulture;
        report.AppendLine(inv, $"Single-lane pattern hunt — {rows.Count} patterns at levels 1–3:");
        report.AppendLine();

        foreach (var (level, label, cfPrefix, verdict) in rows)
        {
            report.AppendLine(inv, $"  L{level}  {label,-32}  CF [{cfPrefix}, …]   {verdict}");
        }

        report.AppendLine();
        report.AppendLine(
            inv,
            $"Totals: rational={rationalCount}, " +
            $"Quadratic-solved={quadraticSolved}, " +
            $"EFamily-solved={eFamilySolved}, " +
            $"candidates={candidateCount}");

        var reportText = report.ToString();
        _output.WriteLine(reportText);

        // Also drop the report alongside the test source so it's easy
        // to open from the IDE without digging into Test Explorer panes.
        // BaseDirectory is bin/Debug/net10.0/ — three levels up is the
        // test project root.
        var testProjectDir = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var reportPath = Path.Combine(testProjectDir, "hunt-output.txt");
        File.WriteAllText(reportPath, reportText);
        _output.WriteLine($"Report also written to: {reportPath}");

        // Anchor assertions on a few hand-checked cases.
        Assert.Contains(rows, r =>
            r.Label == "Pattern([], Const(1))" &&
            r.Verdict == "Quadratic → (√5 + 1)/2");                  // φ
        Assert.Contains(rows, r =>
            r.Label == "Pattern([], Const(2))" &&
            r.Verdict == "Quadratic → √2 + 1");                       // 1 + √2
        Assert.Contains(rows, r =>
            r.Label == "Pattern([0], Const(2))" &&
            r.Verdict == "Quadratic → √2 − 1");                       // √2 − 1
        Assert.Contains(rows, r =>
            r.Label == "Pattern([1], Const(2))" &&
            r.Verdict == "Quadratic → √2");                           // √2 itself
        Assert.True(candidateCount > 0,
            "Hunt must produce some unsolved candidates — those are the discovery surface.");
    }

    [Fact]
    public void Hunt_SingleLanePatterns_LevelOne_ShowEachAndSolution()
    {
        var quadratic = new QuadraticIrrationalIdentifier();
        var eFamily = new EFamilyShapeIdentifier();

        var rationalCount = 0;
        var quadraticSolved = 0;
        var eFamilySolved = 0;
        var candidateCount = 0;
        var rows = new List<(int Level, string Label, string Cf, string Verdict)>();

        foreach (var (level, pattern) in EnumerateLevels(maxLevel: 1))
        {
            var label = DescribePattern(pattern);
            var cf = pattern.ToString();

            string verdict;
            if (pattern.IsRational)
            {
                verdict = "rational";
                rationalCount++;
            }
            else if (pattern.IsQuadraticIrrational)
            {
                var r = quadratic.TryIdentify(new ContinuedFraction(pattern));
                if (r.Match)
                {
                    verdict = $"Quadratic → {r.Identification}";
                    quadraticSolved++;
                }
                else
                {
                    verdict = "Quadratic over budget";
                    candidateCount++;
                }
            }
            else
            {
                var r = eFamily.TryIdentify(new ContinuedFraction(pattern));
                if (r.Match)
                {
                    verdict = $"EFamily → {r.Identification}";
                    eFamilySolved++;
                }
                else
                {
                    verdict = "candidate";
                    candidateCount++;
                }
            }

            rows.Add((level, label, cf, verdict));
        }

        var report = new StringBuilder();
        var inv = CultureInfo.InvariantCulture;
        report.AppendLine(inv, $"Single-lane pattern hunt — {rows.Count} patterns at level 1:");
        report.AppendLine();

        foreach (var (level, label, cf, verdict) in rows)
        {
            report.AppendLine(inv, $"  L{level}  {label,-32}  CF {cf}   {verdict}");
        }

        report.AppendLine();
        report.AppendLine(
            inv,
            $"Totals: rational={rationalCount}, " +
            $"Quadratic-solved={quadraticSolved}, " +
            $"EFamily-solved={eFamilySolved}, " +
            $"candidates={candidateCount}");

        var reportText = report.ToString();
        _output.WriteLine(reportText);

        var testProjectDir = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var reportPath = Path.Combine(testProjectDir, "hunt-output-level1.txt");
        File.WriteAllText(reportPath, reportText);
        _output.WriteLine($"Report also written to: {reportPath}");

        Assert.Contains(rows, r =>
            r.Label == "Pattern([], Const(1))" &&
            r.Verdict == "Quadratic → (√5 + 1)/2");                  // φ
    }

    // ---------- enumeration ----------

    private static IEnumerable<(int Level, PatternCFCoefficientGenerator Pattern)> EnumerateLevels(int maxLevel)
    {
        for (var level = 1; level <= maxLevel; level++)
        {
            foreach (BigInteger[] preperiod in EnumeratePreperiods(level))
            {
                foreach (Lane lane in EnumerateLanes(level))
                {
                    if (MaxFieldValue(preperiod, lane) != level)
                    {
                        continue;
                    }
                    yield return (level, new PatternCFCoefficientGenerator(preperiod, [lane]));
                }
            }
        }
    }

    /// <summary>
    /// Pre-periods of length 0 or 1 with a non-negative entry up to
    /// <paramref name="upToValue"/>.
    /// </summary>
    private static IEnumerable<BigInteger[]> EnumeratePreperiods(int upToValue)
    {
        yield return Array.Empty<BigInteger>();
        for (var a = 0; a <= upToValue; a++)
        {
            yield return new BigInteger[] { a };
        }
    }

    /// <summary>
    /// All single lanes at level <paramref name="upToValue"/>: <c>Const(c)</c>
    /// for <c>c ∈ 1..upToValue</c>, <c>Plus(init, op)</c> with both in
    /// <c>1..upToValue</c>, and <c>Multiply(init, op)</c> with
    /// <c>init ∈ 1..upToValue</c> and <c>op ∈ 2..upToValue + 1</c>.
    /// The Multiply range is shifted by one so its minimum operand
    /// (<c>2</c>) appears at level 1 alongside <see cref="Operation.Const"/>
    /// and <see cref="Operation.Plus"/>.
    /// </summary>
    private static IEnumerable<Lane> EnumerateLanes(int upToValue)
    {
        for (var init = 1; init <= upToValue; init++)
        {
            yield return Lane.Const(init);
            for (var op = 1; op <= upToValue; op++)
            {
                yield return Lane.Plus(init, op);
            }
            for (var op = 2; op <= upToValue + 1; op++)
            {
                yield return Lane.Multiply(init, op);
            }
        }
    }

    /// <summary>
    /// The largest absolute value among the pattern's pre-period entries
    /// and the lane's <c>InitialValue</c> / <c>Operand</c> (the latter
    /// only when the operation actually uses it). For
    /// <see cref="Operation.Multiply"/> the operand contributes
    /// <c>|Operand| − 1</c>, so the minimum legal Multiply
    /// (<c>×2</c>) registers as level 1 — matching the way <c>+1</c> is
    /// the level-1 minimum for <see cref="Operation.Plus"/>.
    /// </summary>
    private static int MaxFieldValue(BigInteger[] preperiod, Lane lane)
    {
        var max = 0;
        foreach (var p in preperiod)
        {
            var abs = (int)BigInteger.Abs(p);
            if (abs > max)
            {
                max = abs;
            }
        }

        var initAbs = (int)BigInteger.Abs(lane.InitialValue);
        if (initAbs > max)
        {
            max = initAbs;
        }

        if (lane.Operation != Operation.Const)
        {
            var opAbs = (int)BigInteger.Abs(lane.Operand);
            if (lane.Operation == Operation.Multiply)
            {
                opAbs--;
            }
            if (opAbs > max)
            {
                max = opAbs;
            }
        }

        return max;
    }

    // ---------- formatting ----------

    private static string DescribePattern(PatternCFCoefficientGenerator pattern)
    {
        var pre = pattern.Preperiod.Count == 0
            ? "[]"
            : "[" + string.Join(
                ", ",
                pattern.Preperiod.Select(b => b.ToString(CultureInfo.InvariantCulture))) + "]";
        var lanes = string.Join(", ", pattern.Lanes.Select(l => l.ToString()));
        return $"Pattern({pre}, {lanes})";
    }

    private static string FormatCfPrefix(PatternCFCoefficientGenerator pattern, int length)
    {
        var values = new List<string>();
        for (var i = 0; i < length; i++)
        {
            try
            {
                values.Add(pattern[i].ToString(CultureInfo.InvariantCulture));
            }
            catch (ArgumentOutOfRangeException)
            {
                break;
            }
        }
        return string.Join(", ", values);
    }
}
