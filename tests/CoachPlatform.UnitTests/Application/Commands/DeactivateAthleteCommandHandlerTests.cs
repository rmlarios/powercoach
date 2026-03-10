using CoachPlatform.Application.Features.Athletes.Commands.DeactivateAthlete;
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

public class DeactivateAthleteCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeactivateAthleteCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();

    public DeactivateAthleteCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeactivateAthleteCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithActiveAthlete_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var athleteId = Guid.NewGuid();
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, athleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        var command = new DeactivateAthleteCommand { Id = athleteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        athlete.Status.Should().Be(AthleteStatus.Inactive);
        athlete.EndDate.Should().NotBeNull();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentAthlete_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var command = new DeactivateAthleteCommand { Id = Guid.NewGuid() };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyInactiveAthlete_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var athleteId = Guid.NewGuid();
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, athleteId);
        athlete.Deactivate(); // Already inactive

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        var command = new DeactivateAthleteCommand { Id = athleteId };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already inactive*");
    }

    [Fact]
    public async Task Handle_WithOnHoldAthlete_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var athleteId = Guid.NewGuid();
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, athleteId);
        athlete.PutOnHold();

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        var command = new DeactivateAthleteCommand { Id = athleteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        athlete.Status.Should().Be(AthleteStatus.Inactive);
    }

    [Fact]
    public async Task Handle_WithSuspendedAthlete_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var athleteId = Guid.NewGuid();
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, athleteId);
        athlete.Suspend();

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        var command = new DeactivateAthleteCommand { Id = athleteId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        athlete.Status.Should().Be(AthleteStatus.Inactive);
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
