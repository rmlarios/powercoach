using CoachPlatform.Application.Features.Exercises.Commands.DeleteExercise;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class DeleteExerciseCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeleteExerciseCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();

    public DeleteExerciseCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteExerciseCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidExercise_ShouldDeactivate()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create(_validCoachId, "Bench Press", ExerciseCategory.Bench, MuscleGroup.Chest);
        SetEntityId(exercise, exerciseId);

        exercise.IsActive.Should().BeTrue(); // Precondition

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new DeleteExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = exerciseId
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        exercise.IsActive.Should().BeFalse();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentExercise_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise>());

        var command = new DeleteExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = Guid.NewGuid()
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithExerciseFromDifferentCoach_ShouldThrowNotFoundException()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var otherCoachId = Guid.NewGuid();
        var exercise = Exercise.Create(otherCoachId, "Deadlift", ExerciseCategory.Deadlift, MuscleGroup.Back);
        SetEntityId(exercise, exerciseId);

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new DeleteExerciseCommand
        {
            CoachId = _validCoachId,  // Different coach
            ExerciseId = exerciseId
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyInactiveExercise_ShouldStillSucceed()
    {
        // Arrange - Deactivate() is idempotent, calling it twice should not throw
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create(_validCoachId, "OHP", ExerciseCategory.OverheadPress, MuscleGroup.Shoulders);
        SetEntityId(exercise, exerciseId);
        exercise.Deactivate(); // Already inactive
        exercise.IsActive.Should().BeFalse();

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new DeleteExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = exerciseId
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        exercise.IsActive.Should().BeFalse();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
