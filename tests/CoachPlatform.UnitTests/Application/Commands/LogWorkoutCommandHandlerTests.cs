using CoachPlatform.Application.Features.WorkoutLogs.Commands.LogWorkout;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class LogWorkoutCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly LogWorkoutCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();
    private readonly Guid _validExerciseId = Guid.NewGuid();

    public LogWorkoutCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new LogWorkoutCommandHandler(_contextMock.Object);
    }

    private Exercise CreateValidExercise()
    {
        var exercise = Exercise.Create(_validCoachId, "Bench Press", ExerciseCategory.Bench, MuscleGroup.Chest);
        SetEntityId(exercise, _validExerciseId);
        return exercise;
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldLogWorkout()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var exercise = CreateValidExercise();

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        _contextMock.Setup(x => x.WorkoutLogs)
            .ReturnsDbSet(new List<WorkoutLog>());

        var command = new LogWorkoutCommand
        {
            AthleteId = _validAthleteId,
            ExerciseId = _validExerciseId,
            Sets = 4,
            Reps = 8,
            Weight = 100.0m,
            RPE = 8,
            Notes = "Felt strong today",
            WorkoutDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.WorkoutLogs.Add(It.IsAny<WorkoutLog>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentAthlete_ShouldThrowNotFoundException()
    {
        // Arrange
        var exercise = CreateValidExercise();

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        var command = new LogWorkoutCommand
        {
            AthleteId = Guid.NewGuid(),
            ExerciseId = _validExerciseId,
            Sets = 5,
            Reps = 5,
            WorkoutDate = DateTime.UtcNow.Date
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonexistentExercise_ShouldThrowNotFoundException()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise>());

        var command = new LogWorkoutCommand
        {
            AthleteId = _validAthleteId,
            ExerciseId = Guid.NewGuid(),
            Sets = 5,
            Reps = 5,
            WorkoutDate = DateTime.UtcNow.Date
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldLogWorkout()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var exercise = CreateValidExercise();

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        _contextMock.Setup(x => x.WorkoutLogs)
            .ReturnsDbSet(new List<WorkoutLog>());

        var command = new LogWorkoutCommand
        {
            AthleteId = _validAthleteId,
            ExerciseId = _validExerciseId,
            Sets = 3,
            Reps = 10,
            WorkoutDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.WorkoutLogs.Add(It.IsAny<WorkoutLog>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetAllFieldsCorrectly()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var exercise = Exercise.Create(_validCoachId, "Deadlift", ExerciseCategory.Deadlift, MuscleGroup.Back);
        SetEntityId(exercise, _validExerciseId);

        WorkoutLog? addedLog = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        _contextMock.Setup(x => x.WorkoutLogs)
            .ReturnsDbSet(new List<WorkoutLog>());

        _contextMock.Setup(x => x.WorkoutLogs.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(w => addedLog = w);

        var workoutDate = new DateTime(2026, 3, 9);
        var command = new LogWorkoutCommand
        {
            AthleteId = _validAthleteId,
            ExerciseId = _validExerciseId,
            Sets = 5,
            Reps = 3,
            Weight = 180.5m,
            RPE = 9,
            Notes = "New PR!",
            WorkoutDate = workoutDate
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedLog.Should().NotBeNull();
        addedLog!.AthleteId.Should().Be(_validAthleteId);
        addedLog.ExerciseId.Should().Be(_validExerciseId);
        addedLog.ExerciseName.Should().Be("Deadlift");
        addedLog.Sets.Should().Be(5);
        addedLog.Reps.Should().Be(3);
        addedLog.Weight.Should().Be(180.5m);
        addedLog.RPE.Should().Be(9);
        addedLog.Notes.Should().Be("New PR!");
        addedLog.WorkoutDate.Should().Be(workoutDate);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeDateToStartOfDay()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var exercise = Exercise.Create(_validCoachId, "Squat", ExerciseCategory.Squat, MuscleGroup.Legs);
        SetEntityId(exercise, _validExerciseId);

        WorkoutLog? addedLog = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        _contextMock.Setup(x => x.WorkoutLogs)
            .ReturnsDbSet(new List<WorkoutLog>());

        _contextMock.Setup(x => x.WorkoutLogs.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(w => addedLog = w);

        var workoutDateWithTime = new DateTime(2026, 3, 9, 15, 45, 30);
        var command = new LogWorkoutCommand
        {
            AthleteId = _validAthleteId,
            ExerciseId = _validExerciseId,
            Sets = 4,
            Reps = 6,
            WorkoutDate = workoutDateWithTime
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedLog.Should().NotBeNull();
        addedLog!.WorkoutDate.Should().Be(workoutDateWithTime.Date);
        addedLog.WorkoutDate.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_WithoutOptionalFields_ShouldLogWorkoutWithNulls()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var exercise = Exercise.Create(_validCoachId, "Push-ups", ExerciseCategory.Accessory, MuscleGroup.Chest);
        SetEntityId(exercise, _validExerciseId);

        WorkoutLog? addedLog = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.Exercises)
            .ReturnsDbSet(new List<Exercise> { exercise });

        _contextMock.Setup(x => x.WorkoutLogs)
            .ReturnsDbSet(new List<WorkoutLog>());

        _contextMock.Setup(x => x.WorkoutLogs.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(w => addedLog = w);

        var command = new LogWorkoutCommand
        {
            AthleteId = _validAthleteId,
            ExerciseId = _validExerciseId,
            Sets = 3,
            Reps = 20,
            WorkoutDate = DateTime.UtcNow.Date
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedLog.Should().NotBeNull();
        addedLog!.Weight.Should().BeNull();
        addedLog.RPE.Should().BeNull();
        addedLog.Notes.Should().BeNull();
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
