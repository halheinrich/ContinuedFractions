namespace ContinuedFractions.Generators;

/// <summary>
/// The rule by which a <see cref="Lane"/> evolves its emitted value
/// across visits. Each visit yields one continued-fraction coefficient.
/// </summary>
/// <remarks>
/// <para>
/// A lane has an <c>InitialValue</c> and an <c>Operand</c>; on visit
/// <c>n</c> (zero-indexed: <c>n = 0</c> for the first emission), the lane
/// produces the value determined by its <see cref="Operation"/>.
/// </para>
/// <para>
/// Degenerate operand choices that would make an operation effectively a
/// no-op (e.g. <c>Plus 0</c>) are rejected at <see cref="Lane"/>
/// construction so that <c>Operation == Const</c> is the unique signal
/// of a constant-valued lane.
/// </para>
/// </remarks>
public enum Operation
{
    /// <summary>
    /// Lane emits <c>InitialValue</c> on every visit. <c>Operand</c> is
    /// unused.
    /// </summary>
    Const,

    /// <summary>
    /// Lane emits <c>InitialValue + n · Operand</c> on visit <c>n</c>.
    /// <c>Operand</c> must be strictly positive.
    /// </summary>
    Plus,

    /// <summary>
    /// Lane emits <c>InitialValue · Operandⁿ</c> on visit <c>n</c>.
    /// <c>Operand</c> must be at least 2.
    /// </summary>
    Multiply,
}
