using CoachPlatform.Domain.ValueObjects;
using FluentAssertions;

namespace CoachPlatform.UnitTests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    public void Create_WithValidEmail_ShouldSucceed(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    public void Create_WithInvalidEmail_ShouldThrow(string invalidEmail)
    {
        // Act
        var act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_ShouldBeNormalizedToLowerCase()
    {
        // Arrange
        var mixed = "Test.User@EXAMPLE.COM";

        // Act
        var email = Email.Create(mixed);

        // Assert
        email.Value.Should().Be("test.user@example.com");
    }

    [Fact]
    public void TwoEmails_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM");

        // Act & Assert
        email1.Should().Be(email2);
    }
}
