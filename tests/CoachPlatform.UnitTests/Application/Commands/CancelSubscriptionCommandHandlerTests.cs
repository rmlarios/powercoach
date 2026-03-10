using CoachPlatform.Application.Features.Subscriptions.Commands.CancelSubscription;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class CancelSubscriptionCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CancelSubscriptionCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();
    private readonly Guid _validPlanId = Guid.NewGuid();

    public CancelSubscriptionCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CancelSubscriptionCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithActiveSubscription_ShouldCancelSuccessfully()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, subscriptionId);
        subscription.Activate(); // Make it active first

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        var command = new CancelSubscriptionCommand 
        { 
            Id = subscriptionId,
            Reason = "No longer needed"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.CancellationReason.Should().Be("No longer needed");
        subscription.CancelledAt.Should().NotBeNull();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentSubscription_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription>());

        var command = new CancelSubscriptionCommand { Id = Guid.NewGuid() };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyCancelledSubscription_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, subscriptionId);
        subscription.Activate();
        subscription.Cancel("Previous cancellation"); // Already cancelled

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        var command = new CancelSubscriptionCommand { Id = subscriptionId };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public async Task Handle_WithExpiredSubscription_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, subscriptionId);
        subscription.Activate();
        subscription.Expire(); // Expired

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        var command = new CancelSubscriptionCommand { Id = subscriptionId };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_WithPausedSubscription_ShouldCancelSuccessfully()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, subscriptionId);
        subscription.Activate();
        subscription.Pause(); // Paused

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        var command = new CancelSubscriptionCommand 
        { 
            Id = subscriptionId,
            Reason = "Switching plans"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WithNoReason_ShouldCancelSuccessfullyWithNullReason()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        var subscription = Subscription.Create(_validAthleteId, plan);
        SetEntityId(subscription, subscriptionId);
        subscription.Activate();

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription> { subscription });

        var command = new CancelSubscriptionCommand 
        { 
            Id = subscriptionId,
            Reason = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.CancellationReason.Should().BeNull();
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
