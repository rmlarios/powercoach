using CoachPlatform.Application.Features.Plans.Commands.DeactivatePlan;
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

public class DeactivatePlanCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeactivatePlanCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();

    public DeactivatePlanCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeactivatePlanCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithActivePlan_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = Plan.Create(
            _validCoachId, 
            "Test Plan", 
            99.99m, 
            "USD", 
            30, 
            PlanType.Monthly);
        SetEntityId(plan, planId);

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        var command = new DeactivatePlanCommand { Id = planId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        plan.IsActive.Should().BeFalse();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentPlan_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan>());

        var command = new DeactivatePlanCommand { Id = Guid.NewGuid() };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyInactivePlan_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = Plan.Create(
            _validCoachId, 
            "Test Plan", 
            99.99m, 
            "USD", 
            30, 
            PlanType.Monthly);
        SetEntityId(plan, planId);
        plan.Deactivate(); // Already inactive

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        var command = new DeactivatePlanCommand { Id = planId };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already inactive*");
    }

    [Fact]
    public async Task Handle_WithValidPlan_ShouldCallSaveChanges()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = Plan.Create(
            _validCoachId, 
            "Test Plan", 
            99.99m, 
            "USD", 
            30, 
            PlanType.Monthly);
        SetEntityId(plan, planId);

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        var command = new DeactivatePlanCommand { Id = planId };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithQuarterlyPlan_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = Plan.Create(
            _validCoachId, 
            "Quarterly Plan", 
            249.99m, 
            "USD", 
            90, 
            PlanType.Quarterly);
        SetEntityId(plan, planId);

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        var command = new DeactivatePlanCommand { Id = planId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        plan.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithAnnualPlan_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = Plan.Create(
            _validCoachId, 
            "Annual Plan", 
            899.99m, 
            "USD", 
            365, 
            PlanType.Annual);
        SetEntityId(plan, planId);

        _contextMock.Setup(x => x.Plans)
            .ReturnsDbSet(new List<Plan> { plan });

        var command = new DeactivatePlanCommand { Id = planId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        plan.IsActive.Should().BeFalse();
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
