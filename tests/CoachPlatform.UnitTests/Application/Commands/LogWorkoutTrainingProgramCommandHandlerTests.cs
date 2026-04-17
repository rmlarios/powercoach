using CoachPlatform.Application.Features.TrainingPrograms.Commands.LogWorkout;
using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class LogTrainingProgramWorkoutCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly LogWorkoutCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();
    private readonly Guid _validProgramId = Guid.NewGuid();
    private readonly Guid _validWorkoutId = Guid.NewGuid();
    private readonly Guid _validExerciseId = Guid.NewGuid();

    public LogTrainingProgramWorkoutCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new LogWorkoutCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldLogWorkout()
    {
        // Arrange
        var (workout, exercises) = CreateValidWorkoutWithExercises();

        _contextMock.Setup(x => x.AthleteWorkouts)
            .ReturnsDbSet(new List<AthleteWorkout> { workout });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(exercises);

        var command = new LogWorkoutCommand
        {
            WorkoutId = _validWorkoutId,
            DurationMinutes = 60,
            FatigueRating = 7,
            Notes = "Felt strong today",
            ExerciseSets = new List<LogExerciseSetDto>
            {
                new() { ExerciseId = _validExerciseId, SetNumber = 1, Reps = 5, Weight = 100, Rpe = 7.5m },
                new() { ExerciseId = _validExerciseId, SetNumber = 2, Reps = 5, Weight = 100, Rpe = 8 },
                new() { ExerciseId = _validExerciseId, SetNumber = 3, Reps = 5, Weight = 100, Rpe = 8.5m }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(_validWorkoutId);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentWorkout_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.AthleteWorkouts)
            .ReturnsDbSet(new List<AthleteWorkout>());

        var command = new LogWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
            ExerciseSets = new List<LogExerciseSetDto>
            {
                new() { ExerciseId = _validExerciseId, SetNumber = 1, Reps = 5, Weight = 100 }
            }
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*AthleteWorkout*");
    }

    [Fact]
    public async Task Handle_WithNonexistentExercise_ShouldThrowNotFoundException()
    {
        // Arrange
        var (workout, _) = CreateValidWorkoutWithExercises();

        _contextMock.Setup(x => x.AthleteWorkouts)
            .ReturnsDbSet(new List<AthleteWorkout> { workout });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise>()); // No exercises

        var command = new LogWorkoutCommand
        {
            WorkoutId = _validWorkoutId,
            ExerciseSets = new List<LogExerciseSetDto>
            {
                new() { ExerciseId = Guid.NewGuid(), SetNumber = 1, Reps = 5, Weight = 100 }
            }
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Exercise*");
    }

    [Fact]
    public async Task Handle_WithCompletedProgram_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var (workout, exercises) = CreateValidWorkoutWithExercises(ProgramStatus.Completed);

        _contextMock.Setup(x => x.AthleteWorkouts)
            .ReturnsDbSet(new List<AthleteWorkout> { workout });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(exercises);

        var command = new LogWorkoutCommand
        {
            WorkoutId = _validWorkoutId,
            ExerciseSets = new List<LogExerciseSetDto>
            {
                new() { ExerciseId = _validExerciseId, SetNumber = 1, Reps = 5, Weight = 100 }
            }
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task Handle_WithNotStartedProgram_ShouldStartProgramAndLogWorkout()
    {
        // Arrange
        var (workout, exercises) = CreateValidWorkoutWithExercises(ProgramStatus.NotStarted);

        _contextMock.Setup(x => x.AthleteWorkouts)
            .ReturnsDbSet(new List<AthleteWorkout> { workout });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(exercises);

        var command = new LogWorkoutCommand
        {
            WorkoutId = _validWorkoutId,
            ExerciseSets = new List<LogExerciseSetDto>
            {
                new() { ExerciseId = _validExerciseId, SetNumber = 1, Reps = 5, Weight = 100 }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(_validWorkoutId);
        workout.AthleteProgram.Status.Should().Be(ProgramStatus.Active);
    }

    [Fact]
    public async Task Handle_WithMultipleSets_ShouldLogAllSets()
    {
        // Arrange
        var (workout, exercises) = CreateValidWorkoutWithExercises();

        _contextMock.Setup(x => x.AthleteWorkouts)
            .ReturnsDbSet(new List<AthleteWorkout> { workout });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(exercises);

        var command = new LogWorkoutCommand
        {
            WorkoutId = _validWorkoutId,
            DurationMinutes = 90,
            FatigueRating = 8,
            ExerciseSets = new List<LogExerciseSetDto>
            {
                new() { ExerciseId = _validExerciseId, SetNumber = 1, Reps = 5, Weight = 140, Rpe = 6 },
                new() { ExerciseId = _validExerciseId, SetNumber = 2, Reps = 5, Weight = 150, Rpe = 7 },
                new() { ExerciseId = _validExerciseId, SetNumber = 3, Reps = 5, Weight = 160, Rpe = 8 },
                new() { ExerciseId = _validExerciseId, SetNumber = 4, Reps = 3, Weight = 170, Rpe = 9 },
                new() { ExerciseId = _validExerciseId, SetNumber = 5, Reps = 1, Weight = 180, Rpe = 10 }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(_validWorkoutId);
        workout.ExerciseLogs.Should().HaveCount(5);
        workout.IsCompleted.Should().BeTrue();
    }

    private (AthleteWorkout workout, List<Exercise> exercises) CreateValidWorkoutWithExercises(
        ProgramStatus status = ProgramStatus.Active)
    {
        // Create an exercise
        var exercise = Exercise.Create(
            _validCoachId,
            "Back Squat",
            ExerciseCategory.Squat,
            MuscleGroup.Quadriceps,
            "Main lower body compound",
            true);
        SetEntityId(exercise, _validExerciseId);

        // Create athlete program
        var athleteProgram = AthleteProgram.Create(
            _validAthleteId,
            _validProgramId,
            DateTime.UtcNow);
        SetEntityId(athleteProgram, Guid.NewGuid());

        if (status == ProgramStatus.Active)
        {
            athleteProgram.Start();
        }
        else if (status == ProgramStatus.Completed)
        {
            athleteProgram.Start();
            athleteProgram.Complete();
        }

        // Create workout
        var workout = athleteProgram.AddWorkout(1, 1, DateTime.UtcNow);
        SetEntityId(workout, _validWorkoutId);
        
        // Set the navigation property manually (EF Core does this automatically with Include)
        SetNavigationProperty(workout, "AthleteProgram", athleteProgram);

        return (workout, new List<Exercise> { exercise });
    }

    private static void SetNavigationProperty<T>(T entity, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName);
        property?.SetValue(entity, value);
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var baseType = typeof(T).BaseType;
        while (baseType != null && baseType != typeof(object))
        {
            var idProperty = baseType.GetProperty("Id");
            if (idProperty != null)
            {
                idProperty.SetValue(entity, id);
                return;
            }
            baseType = baseType.BaseType;
        }
    }
}
