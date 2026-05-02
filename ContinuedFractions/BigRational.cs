using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// An exact rational number p/q with arbitrary-precision numerator and
/// denominator. Always stored in canonical form: gcd(|p|,q) = 1, q &gt; 0,
/// and 0 is represented as 0/1.
/// </summary>
public readonly struct BigRational : IEquatable<BigRational>
{
    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }

    public static BigRational Zero { get; } = new(BigInteger.Zero, BigInteger.One, skipNormalize: true);
    public static BigRational One { get; } = new(BigInteger.One, BigInteger.One, skipNormalize: true);

    public BigRational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero)
            throw new DivideByZeroException("Denominator cannot be zero.");

        if (denominator.Sign < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        if (numerator.IsZero)
        {
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
            return;
        }

        var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    private BigRational(BigInteger numerator, BigInteger denominator, bool skipNormalize)
    {
        Numerator = numerator;
        Denominator = denominator;
    }

    public static implicit operator BigRational(BigInteger n)
        => new(n, BigInteger.One, skipNormalize: true);
    public static implicit operator BigRational(long n)
        => new(n, BigInteger.One, skipNormalize: true);
    public static implicit operator BigRational(int n)
        => new(n, BigInteger.One, skipNormalize: true);

    public static BigRational operator +(BigRational a, BigRational b)
        => new(a.Numerator * b.Denominator + b.Numerator * a.Denominator,
               a.Denominator * b.Denominator);

    public static BigRational operator -(BigRational a, BigRational b)
        => new(a.Numerator * b.Denominator - b.Numerator * a.Denominator,
               a.Denominator * b.Denominator);

    public static BigRational operator -(BigRational a)
        => new(-a.Numerator, a.Denominator, skipNormalize: true);

    public static BigRational operator *(BigRational a, BigRational b)
        => new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

    public static BigRational operator /(BigRational a, BigRational b)
    {
        if (b.Numerator.IsZero)
            throw new DivideByZeroException();
        return new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }

    public static bool operator ==(BigRational a, BigRational b) => a.Equals(b);
    public static bool operator !=(BigRational a, BigRational b) => !a.Equals(b);

    public bool Equals(BigRational other)
        => Numerator == other.Numerator && Denominator == other.Denominator;

    public override bool Equals(object? obj) => obj is BigRational r && Equals(r);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    public override string ToString()
        => Denominator.IsOne ? Numerator.ToString() : $"{Numerator}/{Denominator}";
}
