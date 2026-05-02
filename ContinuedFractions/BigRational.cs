using System.Globalization;
using System.Numerics;

namespace ContinuedFractions;

/// <summary>
/// An exact rational number p/q with arbitrary-precision numerator and
/// denominator. Always stored in canonical form: gcd(|p|, q) = 1, q &gt; 0,
/// and zero is represented as 0/1.
/// </summary>
/// <remarks>
/// The default-initialized value (<see cref="Denominator"/> = 0) is
/// considered invalid. Use <see cref="Zero"/> for a zero-valued instance.
/// </remarks>
public readonly struct BigRational
    : IEquatable<BigRational>,
      IComparable<BigRational>,
      IComparable,
      IFormattable
{
    /// <summary>The numerator p. Sign of the rational equals the sign of p.</summary>
    public BigInteger Numerator { get; }

    /// <summary>The denominator q. Always strictly positive after construction.</summary>
    public BigInteger Denominator { get; }

    /// <summary>The rational 0/1.</summary>
    public static BigRational Zero { get; } = new(BigInteger.Zero, BigInteger.One);

    /// <summary>The rational 1/1.</summary>
    public static BigRational One { get; } = new(BigInteger.One, BigInteger.One);

    /// <summary>The rational -1/1.</summary>
    public static BigRational MinusOne { get; } = new(BigInteger.MinusOne, BigInteger.One);

    /// <summary>
    /// Constructs the canonical form of <paramref name="numerator"/> /
    /// <paramref name="denominator"/>.
    /// </summary>
    /// <exception cref="DivideByZeroException">
    /// <paramref name="denominator"/> is zero.
    /// </exception>
    public BigRational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero)
        {
            throw new DivideByZeroException("Denominator cannot be zero.");
        }

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

    /// <summary>Sign of the rational: -1, 0, or +1.</summary>
    public int Sign => Numerator.Sign;

    /// <summary>True when the rational equals an integer (denominator is 1).</summary>
    public bool IsInteger => Denominator.IsOne;

    /// <summary>True when the rational equals zero.</summary>
    public bool IsZero => Numerator.IsZero;

    /// <summary>Returns the absolute value of <paramref name="value"/>.</summary>
    public static BigRational Abs(BigRational value)
        => value.Sign < 0 ? new BigRational(-value.Numerator, value.Denominator) : value;

    /// <summary>
    /// Returns the multiplicative inverse q/p of <paramref name="value"/>.
    /// </summary>
    /// <exception cref="DivideByZeroException">
    /// <paramref name="value"/> is zero.
    /// </exception>
    public static BigRational Reciprocal(BigRational value)
    {
        if (value.IsZero)
        {
            throw new DivideByZeroException("Cannot take the reciprocal of zero.");
        }
        return new BigRational(value.Denominator, value.Numerator);
    }

    // ---------- conversions (explicit only) ----------

    /// <summary>Returns the rational n/1.</summary>
    public static BigRational FromBigInteger(BigInteger value)
        => new(value, BigInteger.One);

    /// <summary>Returns the rational n/1.</summary>
    public static BigRational FromInt64(long value)
        => new(value, BigInteger.One);

    /// <summary>Returns the rational n/1.</summary>
    public static BigRational FromInt32(int value)
        => new(value, BigInteger.One);

    public static explicit operator BigRational(BigInteger value) => FromBigInteger(value);
    public static explicit operator BigRational(long value) => FromInt64(value);
    public static explicit operator BigRational(int value) => FromInt32(value);

    // ---------- arithmetic ----------

    public static BigRational operator +(BigRational left, BigRational right)
        => new(left.Numerator * right.Denominator + right.Numerator * left.Denominator,
               left.Denominator * right.Denominator);

    public static BigRational operator -(BigRational left, BigRational right)
        => new(left.Numerator * right.Denominator - right.Numerator * left.Denominator,
               left.Denominator * right.Denominator);

    public static BigRational operator -(BigRational value)
        => new(-value.Numerator, value.Denominator);

    public static BigRational operator *(BigRational left, BigRational right)
        => new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

    public static BigRational operator /(BigRational left, BigRational right)
    {
        if (right.Numerator.IsZero)
        {
            throw new DivideByZeroException();
        }
        return new BigRational(left.Numerator * right.Denominator, left.Denominator * right.Numerator);
    }

    /// <summary>Friendly alternate for <c>operator +</c>.</summary>
    public static BigRational Add(BigRational left, BigRational right) => left + right;

    /// <summary>Friendly alternate for <c>operator -</c> (binary).</summary>
    public static BigRational Subtract(BigRational left, BigRational right) => left - right;

    /// <summary>Friendly alternate for unary <c>operator -</c>.</summary>
    public static BigRational Negate(BigRational value) => -value;

    /// <summary>Friendly alternate for <c>operator *</c>.</summary>
    public static BigRational Multiply(BigRational left, BigRational right) => left * right;

    /// <summary>Friendly alternate for <c>operator /</c>.</summary>
    public static BigRational Divide(BigRational left, BigRational right) => left / right;

    // ---------- equality ----------

    public bool Equals(BigRational other)
        => Numerator == other.Numerator && Denominator == other.Denominator;

    public override bool Equals(object? obj) => obj is BigRational r && Equals(r);

    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    public static bool operator ==(BigRational left, BigRational right) => left.Equals(right);

    public static bool operator !=(BigRational left, BigRational right) => !left.Equals(right);

    // ---------- comparison ----------

    /// <summary>
    /// Compares this rational with <paramref name="other"/>. Both denominators
    /// are positive, so cross-multiplication preserves order.
    /// </summary>
    public int CompareTo(BigRational other)
        => (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);

    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }
        if (obj is BigRational other)
        {
            return CompareTo(other);
        }
        throw new ArgumentException("Object is not a BigRational.", nameof(obj));
    }

    public static bool operator <(BigRational left, BigRational right) => left.CompareTo(right) < 0;
    public static bool operator <=(BigRational left, BigRational right) => left.CompareTo(right) <= 0;
    public static bool operator >(BigRational left, BigRational right) => left.CompareTo(right) > 0;
    public static bool operator >=(BigRational left, BigRational right) => left.CompareTo(right) >= 0;

    // ---------- formatting ----------

    /// <summary>
    /// Returns a string of the form "p/q", or just "p" when the denominator
    /// is 1, using the current culture.
    /// </summary>
    public override string ToString() => ToString(null, CultureInfo.CurrentCulture);

    /// <inheritdoc cref="ToString(string?, IFormatProvider?)"/>
    public string ToString(IFormatProvider? formatProvider)
        => ToString(null, formatProvider);

    /// <summary>
    /// Returns a string of the form "p/q", or just "p" when the denominator
    /// is 1. <paramref name="format"/> is forwarded to
    /// <see cref="BigInteger.ToString(string?, IFormatProvider?)"/> for both
    /// numerator and denominator.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        var numerator = Numerator.ToString(format, formatProvider);
        if (Denominator.IsOne)
        {
            return numerator;
        }
        var denominator = Denominator.ToString(format, formatProvider);
        return $"{numerator}/{denominator}";
    }

    // ---------- parsing ----------

    /// <summary>
    /// Parses a string of the form "p/q" or "p" into a <see cref="BigRational"/>.
    /// Whitespace around the components is allowed.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="FormatException">The input is not a valid rational.</exception>
    public static BigRational Parse(string s, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (TryParse(s, provider, out var result))
        {
            return result;
        }
        throw new FormatException($"Could not parse '{s}' as a {nameof(BigRational)}.");
    }

    /// <summary>
    /// Tries to parse a string of the form "p/q" or "p" into a
    /// <see cref="BigRational"/>. Returns false on null, malformed input,
    /// or a zero denominator.
    /// </summary>
    public static bool TryParse(string? s, IFormatProvider? provider, out BigRational result)
    {
        result = default;
        if (s is null)
        {
            return false;
        }

        var trimmed = s.AsSpan().Trim();
        var slash = trimmed.IndexOf('/');

        if (slash < 0)
        {
            if (!BigInteger.TryParse(trimmed, NumberStyles.Integer, provider, out var integer))
            {
                return false;
            }
            result = FromBigInteger(integer);
            return true;
        }

        var numeratorText = trimmed[..slash].Trim();
        var denominatorText = trimmed[(slash + 1)..].Trim();

        if (!BigInteger.TryParse(numeratorText, NumberStyles.Integer, provider, out var numerator))
        {
            return false;
        }
        if (!BigInteger.TryParse(denominatorText, NumberStyles.Integer, provider, out var denominator))
        {
            return false;
        }
        if (denominator.IsZero)
        {
            return false;
        }

        result = new BigRational(numerator, denominator);
        return true;
    }
}
