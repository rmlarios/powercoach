using CoachPlatform.Application.Features.Plans.Commands.CreatePlan;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class CreatePlanCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreatePlanCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();

    public CreatePlanCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreatePlanCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreatePlan()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan>());

        var command = new CreatePlanCommand
        {
            CoachId = _validCoachId,
            Name = "Monthly Plan",
            Description = "A monthly coaching plan",
            Price = 99.99m,
            Currency = "USD",
            DurationInMonths = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.Plans.Add(It.IsAny<Plan>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentCoach_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach>());

        var command = new CreatePlanCommand
        {
            CoachId = Guid.NewGuid(),
            Name = "Monthly Plan",
            Price = 99.99m,
            Currency = "USD",
            DurationInMonths = 1
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithMonthlyDuration_ShouldSetMonthlyPlanType()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        Plan? addedPlan = null;
        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan>());

        _contextMock.Setup(x => x.Plans.Add(It.IsAny<Plan>()))
            .Callback<Plan>(p => addedPlan = p);

        var command = new CreatePlanCommand
        {
            CoachId = _validCoachId,
            Name = "Monthly Plan",
            Price = 99.99m,
            Currency = "USD",
            DurationInMonths = 1
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedPlan.Should().NotBeNull();
        addedPlan!.PlanType.Should().Be(PlanType.Monthly);
        addedPlan.DurationDays.Should().Be(30);
    }

    [Fact]
    public async Task Handle_WithQuarterlyDuration_ShouldSetQuarterlyPlanType()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        Plan? addedPlan = null;
        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan>());

        _contextMock.Setup(x => x.Plans.Add(It.IsAny<Plan>()))
            .Callback<Plan>(p => addedPlan = p);

        var command = new CreatePlanCommand
        {
            CoachId = _validCoachId,
            Name = "Quarterly Plan",
            Price = 249.99m,
            Currency = "USD",
            DurationInMonths = 3
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedPlan.Should().NotBeNull();
        addedPlan!.PlanType.Should().Be(PlanType.Quarterly);
        addedPlan.DurationDays.Should().Be(90);
    }

    [Fact]
    public async Task Handle_WithAnnualDuration_ShouldSetAnnualPlanType()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        Plan? addedPlan = null;
        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan>());

        _contextMock.Setup(x => x.Plans.Add(It.IsAny<Plan>()))
            .Callback<Plan>(p => addedPlan = p);

        var command = new CreatePlanCommand
        {
            CoachId = _validCoachId,
            Name = "Annual Plan",
            Price = 899.99m,
            Currency = "USD",
            DurationInMonths = 12
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedPlan.Should().NotBeNull();
        addedPlan!.PlanType.Should().Be(PlanType.Annual);
        addedPlan.DurationDays.Should().Be(360);
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldCreatePlan()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan>());

        var command = new CreatePlanCommand
        {
            CoachId = _validCoachId,
            Name = "Basic Plan",
            Price = 50m,
            Currency = "USD",
            DurationInMonths = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentCurrency_ShouldCreatePlanWithCurrency()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        Plan? addedPlan = null;
        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan>());

        _contextMock.Setup(x => x.Plans.Add(It.IsAny<Plan>()))
            .Callback<Plan>(p => addedPlan = p);

        var command = new CreatePlanCommand
        {
            CoachId = _validCoachId,
            Name = "Euro Plan",
            Price = 79.99m,
            Currency = "EUR",
            DurationInMonths = 1
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedPlan.Should().NotBeNull();
        addedPlan!.Price.Currency.Should().Be("EUR");
        addedPlan.Price.Amount.Should().Be(79.99m);
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
