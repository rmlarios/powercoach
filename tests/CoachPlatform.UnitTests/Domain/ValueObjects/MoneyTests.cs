using CoachPlatform.Domain.ValueObjects;
using FluentAssertions;

namespace CoachPlatform.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Act
        var money = Money.Create(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        // Act
        var act = () => Money.Create(-10m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Create_WithInvalidCurrency_ShouldThrow(string currency)
    {
        // Act
        var act = () => Money.Create(100m, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldSucceed()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrow()
    {
        // Arrange
        var usd = Money.Create(100m, "USD");
        var eur = Money.Create(50m, "EUR");

        // Act
        var act = () => usd.Add(eur);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_WithSufficientAmount_ShouldSucceed()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(30m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_WithInsufficientAmount_ShouldThrow()
    {
        // Arrange
        var money1 = Money.Create(50m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*negative amount*");
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        // Act
        var zero = Money.Zero("USD");

        // Assert
        zero.Amount.Should().Be(0m);
        zero.Currency.Should().Be("USD");
    }

    [Fact]
    public void TwoMoney_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
    }
}
