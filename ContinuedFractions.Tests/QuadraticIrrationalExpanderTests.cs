using System.Numerics;

namespace ContinuedFractions.Tests;

public class QuadraticIrrationalExpanderTests
{
    // ---------- known irrational expansions ----------

    [Fact]
    public void TryExpand_Sqrt2_PreperiodOneAndPeriodTwo()
    {
        // √2 = [1; 2, 2, 2, …] — preperiod [1], period [2].
        var result = QuadraticIrrationalExpander.TryExpand(2, 0, 1, maxSteps: 16);

        Assert.NotNull(result);
        var (pre, per) = result.Value;
        Assert.Equal(new BigInteger[] { 1 }, pre);
        Assert.Equal(new BigInteger[] { 2 }, per);
    }

    [Fact]
    public void TryExpand_Sqrt7_PreperiodTwoAndPeriodOneOneOneFour()
    {
        // √7 = [2; 1, 1, 1, 4, 1, 1, 1, 4, …] — preperiod [2], period [1,1,1,4].
        var result = QuadraticIrrationalExpander.TryExpand(7, 0, 1, maxSteps: 16);

        Assert.NotNull(result);
        var (pre, per) = result.Value;
        Assert.Equal(new BigInteger[] { 2 }, pre);
        Assert.Equal(new BigInteger[] { 1, 1, 1, 4 }, per);
    }

    [Fact]
    public void TryExpand_Sqrt23_PreperiodFourAndPeriodOneThreeOneEight()
    {
        // √23 = [4; 1, 3, 1, 8, …] — preperiod [4], period [1, 3, 1, 8].
        var result = QuadraticIrrationalExpander.TryExpand(23, 0, 1, maxSteps: 16);

        Assert.NotNull(result);
        var (pre, per) = result.Value;
        Assert.Equal(new BigInteger[] { 4 }, pre);
        Assert.Equal(new BigInteger[] { 1, 3, 1, 8 }, per);
    }

    [Fact]
    public void TryExpand_GeneralForm_Sqrt7Minus2Over3()
    {
        // (√7 − 2)/3 ≈ 0.215. CF: [0; 4, 1, 1, 1, 4, 1, 1, 1, …].
        // Preperiod [0], period [4, 1, 1, 1].
        var result = QuadraticIrrationalExpander.TryExpand(7, 2, 3, maxSteps: 16);

        Assert.NotNull(result);
        var (pre, per) = result.Value;
        Assert.Equal(new BigInteger[] { 0 }, pre);
        Assert.Equal(new BigInteger[] { 4, 1, 1, 1 }, per);
    }

    // ---------- perfect-square (rational) cases ----------

    [Fact]
    public void TryExpand_PerfectSquareD_ProducesRationalWithEmptyPeriod()
    {
        // (√4 − 0)/1 = 2 — rational, single coefficient, empty period.
        var result = QuadraticIrrationalExpander.TryExpand(4, 0, 1, maxSteps: 16);

        Assert.NotNull(result);
        var (pre, per) = result.Value;
        Assert.Equal(new BigInteger[] { 2 }, pre);
        Assert.Empty(per);
    }

    [Fact]
    public void TryExpand_OneOneOne_IsRationalZero()
    {
        // (√1 − 1)/1 = 0 — rational.
        var result = QuadraticIrrationalExpander.TryExpand(1, 1, 1, maxSteps: 16);

        Assert.NotNull(result);
        var (pre, per) = result.Value;
        Assert.Equal(new BigInteger[] { 0 }, pre);
        Assert.Empty(per);
    }

    // ---------- input validation ----------

    [Fact]
    public void TryExpand_ReturnsNull_OnNegativeD()
    {
        Assert.Null(QuadraticIrrationalExpander.TryExpand(-1, 0, 1, maxSteps: 16));
    }

    [Fact]
    public void TryExpand_AcceptsNegativeP_AndExpandsPhi()
    {
        // φ = (1 + √5)/2 = (√5 − (−1))/2 — d=5, p=−1, q=2.
        // Period [1] (the slowest-converging CF: [1; 1, 1, 1, …]).
        var result = QuadraticIrrationalExpander.TryExpand(5, -1, 2, maxSteps: 16);

        Assert.NotNull(result);
        var (pre, per) = result.Value;
        Assert.Empty(pre);
        Assert.Equal(new BigInteger[] { 1 }, per);
    }

    [Fact]
    public void TryExpand_ReturnsNull_OnZeroQ()
    {
        Assert.Null(QuadraticIrrationalExpander.TryExpand(7, 0, 0, maxSteps: 16));
    }

    [Fact]
    public void TryExpand_ReturnsNull_OnNegativeQ()
    {
        Assert.Null(QuadraticIrrationalExpander.TryExpand(7, 0, -1, maxSteps: 16));
    }

    [Fact]
    public void TryExpand_ReturnsNull_OnNonCanonicalDivisibility()
    {
        // d − p² = 2 − 1 = 1; q = 2 does not divide 1.
        Assert.Null(QuadraticIrrationalExpander.TryExpand(2, 1, 2, maxSteps: 16));
    }

    [Fact]
    public void TryExpand_RejectsNonPositiveMaxSteps()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            QuadraticIrrationalExpander.TryExpand(7, 0, 1, maxSteps: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            QuadraticIrrationalExpander.TryExpand(7, 0, 1, maxSteps: -1));
    }

    // ---------- budget exhaustion ----------

    [Fact]
    public void TryExpand_ReturnsNull_WhenBudgetIsTooSmallForCycleDetection()
    {
        // √7 has preperiod + period = 5; cycle detection needs ≥ 6 steps.
        // With maxSteps = 5, the iteration ends just before revisiting (2, 3).
        Assert.Null(QuadraticIrrationalExpander.TryExpand(7, 0, 1, maxSteps: 5));
    }

    [Fact]
    public void TryExpand_DetectsCycleAtMinimalBudget()
    {
        // √7: preperiod + period = 5; cycle detected at step 5, so
        // maxSteps = 6 is the smallest budget that succeeds.
        var result = QuadraticIrrationalExpander.TryExpand(7, 0, 1, maxSteps: 6);

        Assert.NotNull(result);
        var (pre, per) = result.Value;
        Assert.Equal(new BigInteger[] { 2 }, pre);
        Assert.Equal(new BigInteger[] { 1, 1, 1, 4 }, per);
    }
}
