using System.Globalization;
using System.Numerics;

namespace HalHeinrich.Numerics.ContinuedFractions.Generators;

/// <summary>
/// A single lane in a <see cref="PatternCFCoefficientGenerator"/>'s
/// periodic part. A lane carries an initial value, an operation, and an
/// operand, and emits one continued-fraction coefficient each time it is
/// visited — the value being determined by the operation rule.
/// </summary>
/// <remarks>
/// <para>
/// Validation in the constructor rejects every degenerate combination
/// that would either make the lane silently constant (when the user
/// asked for a non-constant operation) or produce an invalid CF
/// coefficient at some visit. After construction:
/// </para>
/// <list type="bullet">
/// <item><c>InitialValue ≥ 1</c></item>
/// <item>For <see cref="Operation.Plus"/>, <c>Operand ≥ 1</c></item>
/// <item>For <see cref="Operation.Multiply"/>, <c>Operand ≥ 2</c></item>
/// <item>For <see cref="Operation.Const"/>, <c>Operand</c> is unused</item>
/// </list>
/// <para>
/// These rules guarantee every emitted value is a valid partial quotient
/// (<c>≥ 1</c>) regardless of visit count.
/// </para>
/// <para>
/// The <see langword="default"/> value (<c>InitialValue = 0</c>,
/// <c>Operation = Const</c>, <c>Operand = 0</c>) violates the
/// <c>InitialValue ≥ 1</c> invariant and should not be used.
/// </para>
/// </remarks>
public readonly record struct Lane
{
    /// <summary>The starting value of the lane (its visit-0 emission).</summary>
    public BigInteger InitialValue { get; }

    /// <summary>The rule for evolving the emission across visits.</summary>
    public Operation Operation { get; }

    /// <summary>The parameter to the operation. Unused for <see cref="Operation.Const"/>.</summary>
    public BigInteger Operand { get; }

    /// <summary>
    /// Constructs a lane with the given operation rule, validating that
    /// every visit will produce a valid partial quotient.
    /// </summary>
    /// <param name="initialValue">
    /// The lane's value on its first visit (visit 0). Must be at least 1.
    /// </param>
    /// <param name="operation">The evolution rule.</param>
    /// <param name="operand">
    /// Operation parameter. Must be at least 1 for <see cref="Operation.Plus"/>;
    /// at least 2 for <see cref="Operation.Multiply"/>; unused for
    /// <see cref="Operation.Const"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="initialValue"/> is less than 1, or
    /// <paramref name="operation"/> is not a defined enum value.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="operand"/> violates the operation's constraint
    /// (e.g. <c>Plus 0</c> or <c>Multiply 1</c>).
    /// </exception>
    public Lane(BigInteger initialValue, Operation operation, BigInteger operand)
    {
        if (initialValue.Sign <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialValue),
                initialValue,
                "Initial value must be at least 1 — every emitted value must be a valid partial quotient.");
        }

        switch (operation)
        {
            case Operation.Const:
                // Operand is unused.
                break;

            case Operation.Plus:
                if (operand.Sign <= 0)
                {
                    throw new ArgumentException(
                        $"Plus operand must be at least 1 (got {operand.ToString(CultureInfo.InvariantCulture)}). " +
                        "Plus 0 is a no-op — use Const if a constant lane is intended; " +
                        "negative operands eventually produce non-positive values.",
                        nameof(operand));
                }
                break;

            case Operation.Multiply:
                if (operand < new BigInteger(2))
                {
                    throw new ArgumentException(
                        $"Multiply operand must be at least 2 (got {operand.ToString(CultureInfo.InvariantCulture)}). " +
                        "Multiply 1 is a no-op (use Const), Multiply 0 produces zero (invalid partial quotient), " +
                        "and negative operands produce sign-alternating sequences.",
                        nameof(operand));
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation), operation, "Unknown operation.");
        }

        InitialValue = initialValue;
        Operation = operation;
        Operand = operand;
    }

    /// <summary>
    /// Constructs a constant-valued lane that emits <paramref name="value"/>
    /// on every visit.
    /// </summary>
    public static Lane Const(BigInteger value) =>
        new(value, Operation.Const, BigInteger.Zero);

    /// <summary>
    /// Constructs an arithmetic-progression lane that emits
    /// <c><paramref name="initialValue"/> + n · <paramref name="increment"/></c>
    /// on visit <c>n</c>.
    /// </summary>
    public static Lane Plus(BigInteger initialValue, BigInteger increment) =>
        new(initialValue, Operation.Plus, increment);

    /// <summary>
    /// Constructs a geometric-progression lane that emits
    /// <c><paramref name="initialValue"/> · <paramref name="factor"/>ⁿ</c>
    /// on visit <c>n</c>.
    /// </summary>
    public static Lane Multiply(BigInteger initialValue, BigInteger factor) =>
        new(initialValue, Operation.Multiply, factor);

    /// <summary>
    /// Returns this lane's emission on visit <paramref name="visit"/>,
    /// computed exactly per the lane's operation rule.
    /// </summary>
    /// <param name="visit">Zero-based visit index.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="visit"/> is negative.
    /// </exception>
    public BigInteger ValueAt(int visit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(visit);
        return Operation switch
        {
            Operation.Const => InitialValue,
            Operation.Plus => InitialValue + visit * Operand,
            Operation.Multiply => InitialValue * BigInteger.Pow(Operand, visit),
            _ => throw new InvalidOperationException(
                $"Lane has unsupported operation {Operation}."),
        };
    }

    /// <summary>
    /// A short human-readable description, e.g. <c>"Const(1)"</c>,
    /// <c>"Plus(2, +2)"</c>, <c>"Multiply(1, ×2)"</c>.
    /// </summary>
    public override string ToString()
    {
        var iv = InitialValue.ToString(CultureInfo.InvariantCulture);
        var op = Operand.ToString(CultureInfo.InvariantCulture);
        return Operation switch
        {
            Operation.Const => $"Const({iv})",
            Operation.Plus => $"Plus({iv}, +{op})",
            Operation.Multiply => $"Multiply({iv}, ×{op})",
            _ => $"{Operation}({iv}, {op})",
        };
    }
}
