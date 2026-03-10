using CoachPlatform.Application.Features.Subscriptions.Commands.CreateSubscription;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class CreateSubscriptionCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateSubscriptionCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();
    private readonly Guid _validPlanId = Guid.NewGuid();

    public CreateSubscriptionCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateSubscriptionCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateSubscription()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription>());

        var command = new CreateSubscriptionCommand
        {
            AthleteId = _validAthleteId,
            PlanId = _validPlanId,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.Subscriptions.Add(It.IsAny<Subscription>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentAthlete_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var command = new CreateSubscriptionCommand
        {
            AthleteId = Guid.NewGuid(),
            PlanId = _validPlanId,
            StartDate = DateTime.UtcNow
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonexistentPlan_ShouldThrowNotFoundException()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan>());

        var command = new CreateSubscriptionCommand
        {
            AthleteId = _validAthleteId,
            PlanId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithInactivePlan_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);
        plan.Deactivate(); // Make plan inactive

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        var command = new CreateSubscriptionCommand
        {
            AthleteId = _validAthleteId,
            PlanId = _validPlanId,
            StartDate = DateTime.UtcNow
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldSetActiveStatus()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var plan = Plan.Create(_validCoachId, "Monthly Plan", 99.99m, "USD", 30, PlanType.Monthly);
        SetEntityId(plan, _validPlanId);

        Subscription? addedSubscription = null;
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription>());

        _contextMock.Setup(x => x.Subscriptions.Add(It.IsAny<Subscription>()))
            .Callback<Subscription>(s => addedSubscription = s);

        var startDate = DateTime.UtcNow;
        var command = new CreateSubscriptionCommand
        {
            AthleteId = _validAthleteId,
            PlanId = _validPlanId,
            StartDate = startDate
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedSubscription.Should().NotBeNull();
        addedSubscription!.Status.Should().Be(SubscriptionStatus.Active);
        addedSubscription.StartDate.Should().Be(startDate);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCalculateEndDateFromPlanDuration()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var plan = Plan.Create(_validCoachId, "Quarterly Plan", 249.99m, "USD", 90, PlanType.Quarterly);
        SetEntityId(plan, _validPlanId);

        Subscription? addedSubscription = null;
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        _contextMock.Setup(x => x.Subscriptions)
            .ReturnsDbSet(new List<Subscription>());

        _contextMock.Setup(x => x.Subscriptions.Add(It.IsAny<Subscription>()))
            .Callback<Subscription>(s => addedSubscription = s);

        var startDate = DateTime.UtcNow;
        var command = new CreateSubscriptionCommand
        {
            AthleteId = _validAthleteId,
            PlanId = _validPlanId,
            StartDate = startDate
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedSubscription.Should().NotBeNull();
        addedSubscription!.EndDate.Should().BeCloseTo(startDate.AddDays(90), TimeSpan.FromSeconds(1));
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
