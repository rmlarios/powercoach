using CoachPlatform.Application.Features.Exercises.Commands.UpdateExercise;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class UpdateExerciseCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateExerciseCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();

    public UpdateExerciseCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateExerciseCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateExercise()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create(_validCoachId, "Low Bar Squat", ExerciseCategory.Squat, MuscleGroup.Quadriceps);
        SetEntityId(exercise, exerciseId);

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new UpdateExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = exerciseId,
            Name = "High Bar Squat",
            Description = "Updated description",
            Category = ExerciseCategory.Squat,
            PrimaryMuscleGroup = MuscleGroup.Quadriceps,
            IsCompound = true,
            Equipment = "Barbell, Squat Rack",
            VideoUrl = "https://youtube.com/watch?v=test"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        exercise.Name.Should().Be("High Bar Squat");
        exercise.Description.Should().Be("Updated description");
        exercise.Equipment.Should().Be("Barbell, Squat Rack");
        exercise.VideoUrl.Should().Be("https://youtube.com/watch?v=test");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentExercise_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise>());

        var command = new UpdateExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = Guid.NewGuid(),
            Name = "Test",
            Category = ExerciseCategory.Squat,
            PrimaryMuscleGroup = MuscleGroup.Quadriceps
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
        var exercise = Exercise.Create(otherCoachId, "Bench Press", ExerciseCategory.Bench, MuscleGroup.Chest);
        SetEntityId(exercise, exerciseId);

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new UpdateExerciseCommand
        {
            CoachId = _validCoachId,  // Different coach
            ExerciseId = exerciseId,
            Name = "Updated Bench Press",
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create(_validCoachId, "Low Bar Squat", ExerciseCategory.Squat, MuscleGroup.Quadriceps);
        SetEntityId(exercise, exerciseId);

        var otherExercise = Exercise.Create(_validCoachId, "High Bar Squat", ExerciseCategory.Squat, MuscleGroup.Quadriceps);
        SetEntityId(otherExercise, Guid.NewGuid());

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise, otherExercise });

        var command = new UpdateExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = exerciseId,
            Name = "High Bar Squat",  // Duplicate name with otherExercise
            Category = ExerciseCategory.Squat,
            PrimaryMuscleGroup = MuscleGroup.Quadriceps
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_SameNameSameExercise_ShouldNotThrowConflict()
    {
        // Arrange — keeping the same name on the same exercise should be fine
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create(_validCoachId, "Bench Press", ExerciseCategory.Bench, MuscleGroup.Chest);
        SetEntityId(exercise, exerciseId);

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new UpdateExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = exerciseId,
            Name = "Bench Press",  // Same name, same exercise
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        exercise.Name.Should().Be("Bench Press");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSecondaryMuscleGroups_ShouldSetThem()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create(_validCoachId, "Squat", ExerciseCategory.Squat, MuscleGroup.Quadriceps);
        SetEntityId(exercise, exerciseId);

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new UpdateExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = exerciseId,
            Name = "Squat",
            Category = ExerciseCategory.Squat,
            PrimaryMuscleGroup = MuscleGroup.Quadriceps,
            SecondaryMuscleGroups = new List<MuscleGroup> { MuscleGroup.Glutes, MuscleGroup.Hamstrings },
            IsCompound = true
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        exercise.SecondaryMuscleGroups.Should().HaveCount(2);
        exercise.SecondaryMuscleGroups.Should().Contain(MuscleGroup.Glutes);
        exercise.SecondaryMuscleGroups.Should().Contain(MuscleGroup.Hamstrings);
    }

    [Fact]
    public async Task Handle_WithCategoryChange_ShouldUpdateCategory()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = Exercise.Create(_validCoachId, "Rowing", ExerciseCategory.Row, MuscleGroup.Back);
        SetEntityId(exercise, exerciseId);

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new UpdateExerciseCommand
        {
            CoachId = _validCoachId,
            ExerciseId = exerciseId,
            Name = "Rowing",
            Category = ExerciseCategory.Accessory,  // Changed
            PrimaryMuscleGroup = MuscleGroup.Back,
            IsCompound = false
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        exercise.Category.Should().Be(ExerciseCategory.Accessory);
        exercise.IsCompound.Should().BeFalse();
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
