using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency.
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>
    /// The monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// The currency code (e.g., USD, EUR, MXN).
    /// </summary>
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Creates a new Money value object.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The 3-letter currency code.</param>
    /// <returns>A valid Money value object.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        currency = currency.Trim().ToUpperInvariant();

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));

        return new Money(amount, currency);
    }

    /// <summary>
    /// Creates a Money value object with USD currency.
    /// </summary>
    public static Money Usd(decimal amount) => Create(amount, "USD");

    /// <summary>
    /// Creates a Money value object with EUR currency.
    /// </summary>
    public static Money Eur(decimal amount) => Create(amount, "EUR");

    /// <summary>
    /// Creates a Money value object with MXN currency.
    /// </summary>
    public static Money Mxn(decimal amount) => Create(amount, "MXN");

    /// <summary>
    /// Creates a zero amount Money with the specified currency.
    /// </summary>
    public static Money Zero(string currency) => Create(0, currency);

    /// <summary>
    /// Adds two Money values. They must have the same currency.
    /// </summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts a Money value from this one. They must have the same currency.
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        
        if (result < 0)
            throw new InvalidOperationException("Subtraction would result in negative amount.");
        
        return Create(result, Currency);
    }

    /// <summary>
    /// Multiplies the amount by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative.", nameof(factor));
        
        return Create(Amount * factor, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot perform operation on different currencies: {Currency} and {other.Currency}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
}
