using CoachPlatform.Application.Features.Payments.Commands.RegisterPayment;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class RegisterPaymentCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly RegisterPaymentCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();
    private readonly Guid _validPlanId = Guid.NewGuid();
    private readonly Guid _validSubscriptionId = Guid.NewGuid();

    public RegisterPaymentCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new RegisterPaymentCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldRegisterPayment()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, _validSubscriptionId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        _contextMock.Setup(x => x.Payments)
            .ReturnsDbSet(new List<Payment>());

        var command = new RegisterPaymentCommand
        {
            AthleteId = _validAthleteId,
            SubscriptionId = _validSubscriptionId,
            Amount = 99.99m,
            Currency = "USD",
            PaymentMethod = "credit_card",
            PaymentDate = DateTime.UtcNow,
            Notes = "Monthly payment"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.Payments.Add(It.IsAny<Payment>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentAthlete_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var command = new RegisterPaymentCommand
        {
            AthleteId = Guid.NewGuid(),
            SubscriptionId = _validSubscriptionId,
            Amount = 99.99m,
            Currency = "USD",
            PaymentDate = DateTime.UtcNow
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonexistentSubscription_ShouldThrowNotFoundException()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription>());

        var command = new RegisterPaymentCommand
        {
            AthleteId = _validAthleteId,
            SubscriptionId = Guid.NewGuid(),
            Amount = 99.99m,
            Currency = "USD",
            PaymentDate = DateTime.UtcNow
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithSubscriptionNotBelongingToAthlete_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var otherAthleteId = Guid.NewGuid();
        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(otherAthleteId, plan); // Different athlete
        SetEntityId(subscription, _validSubscriptionId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        var command = new RegisterPaymentCommand
        {
            AthleteId = _validAthleteId,
            SubscriptionId = _validSubscriptionId,
            Amount = 99.99m,
            Currency = "USD",
            PaymentDate = DateTime.UtcNow
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not belong*");
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSetPaymentAsCompleted()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, _validSubscriptionId);

        Payment? addedPayment = null;
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        _contextMock.Setup(x => x.Payments)
            .ReturnsDbSet(new List<Payment>());

        _contextMock.Setup(x => x.Payments.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => addedPayment = p);

        var command = new RegisterPaymentCommand
        {
            AthleteId = _validAthleteId,
            SubscriptionId = _validSubscriptionId,
            Amount = 99.99m,
            Currency = "USD",
            PaymentMethod = "bank_transfer",
            PaymentDate = DateTime.UtcNow
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedPayment.Should().NotBeNull();
        addedPayment!.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task Handle_WithNotes_ShouldSetNotesOnPayment()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, _validSubscriptionId);

        Payment? addedPayment = null;
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        _contextMock.Setup(x => x.Payments)
            .ReturnsDbSet(new List<Payment>());

        _contextMock.Setup(x => x.Payments.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => addedPayment = p);

        var command = new RegisterPaymentCommand
        {
            AthleteId = _validAthleteId,
            SubscriptionId = _validSubscriptionId,
            Amount = 99.99m,
            Currency = "USD",
            PaymentDate = DateTime.UtcNow,
            Notes = "Payment received via cash"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedPayment.Should().NotBeNull();
        addedPayment!.Notes.Should().Be("Payment received via cash");
    }

    [Fact]
    public async Task Handle_WithDifferentAmount_ShouldSetCorrectAmount()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, _validSubscriptionId);

        Payment? addedPayment = null;
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        _contextMock.Setup(x => x.Payments)
            .ReturnsDbSet(new List<Payment>());

        _contextMock.Setup(x => x.Payments.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => addedPayment = p);

        var command = new RegisterPaymentCommand
        {
            AthleteId = _validAthleteId,
            SubscriptionId = _validSubscriptionId,
            Amount = 150.00m,
            Currency = "EUR",
            PaymentDate = DateTime.UtcNow
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedPayment.Should().NotBeNull();
        addedPayment!.Amount.Amount.Should().Be(150.00m);
        addedPayment.Amount.Currency.Should().Be("EUR");
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
