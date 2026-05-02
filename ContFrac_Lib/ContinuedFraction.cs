namespace ContFrac_Lib;

/// <summary>
/// A simple continued fraction [a0; a1, a2, ...] where a0 is the integer part
/// and a1, a2, ... are positive partial quotients.
/// </summary>
public sealed class ContinuedFraction
{
    public IReadOnlyList<long> Coefficients { get; }

    public ContinuedFraction(IEnumerable<long> coefficients)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        var list = coefficients.ToArray();
        if (list.Length == 0)
            throw new ArgumentException("At least one coefficient is required.", nameof(coefficients));
        for (int i = 1; i < list.Length; i++)
            if (list[i] <= 0)
                throw new ArgumentException($"Partial quotient a{i} must be positive.", nameof(coefficients));
        Coefficients = list;
    }

    public ContinuedFraction(params long[] coefficients) : this((IEnumerable<long>)coefficients) { }
}
