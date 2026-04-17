using CoachPlatform.Application.Features.WorkoutLogs.Commands.StartWorkout;
using CoachPlatform.Application.Features.WorkoutLogs.Commands.SkipWorkout;
using CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteWorkout;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class WorkoutTrackingCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();
    private readonly Guid _validProgramTemplateId = Guid.NewGuid();

    public WorkoutTrackingCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    // ========================
    // Helpers
    // ========================

    private AthleteProgram CreateAthleteProgram()
    {
        var program = AthleteProgram.Create(_validAthleteId, _validProgramTemplateId, DateTime.UtcNow.Date);
        SetEntityId(program, Guid.NewGuid());
        return program;
    }

    private AthleteWorkout CreateWorkout(AthleteProgram program)
    {
        var workout = program.AddWorkout(1, 1, DateTime.UtcNow.Date);
        SetEntityId(workout, Guid.NewGuid());

        // Set navigation property for in-memory LINQ queries (ReturnsDbSet doesn't resolve Include)
        typeof(AthleteWorkout).GetProperty(nameof(AthleteWorkout.AthleteProgram))!
            .SetValue(workout, program);

        return workout;
    }

    private void SetupAthleteWorkouts(params AthleteWorkout[] workouts)
    {
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(workouts.ToList());
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProp = typeof(T).BaseType?.BaseType?.GetProperty("Id")
                     ?? typeof(T).BaseType?.GetProperty("Id");
        idProp?.SetValue(entity, id);
    }

    // ========================
    // StartWorkoutCommandHandler Tests
    // ========================

    [Fact]
    public async Task StartWorkout_WithValidData_ShouldSetInProgress()
    {
        var program = CreateAthleteProgram();
        var workout = CreateWorkout(program);
        SetupAthleteWorkouts(workout);

        var handler = new StartWorkoutCommandHandler(_contextMock.Object);
        var command = new StartWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = workout.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        workout.Status.Should().Be(WorkoutStatus.InProgress);
        workout.StartedAt.Should().NotBeNull();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartWorkout_WithNonexistentWorkout_ShouldThrow()
    {
        SetupAthleteWorkouts();

        var handler = new StartWorkoutCommandHandler(_contextMock.Object);
        var command = new StartWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = Guid.NewGuid()
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task StartWorkout_WhenAlreadyStarted_ShouldThrow()
    {
        var program = CreateAthleteProgram();
        var workout = CreateWorkout(program);
        workout.Start(); // Already in progress
        SetupAthleteWorkouts(workout);

        var handler = new StartWorkoutCommandHandler(_contextMock.Object);
        var command = new StartWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = workout.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================
    // SkipWorkoutCommandHandler Tests
    // ========================

    [Fact]
    public async Task SkipWorkout_WithReason_ShouldSetSkippedStatus()
    {
        var program = CreateAthleteProgram();
        var workout = CreateWorkout(program);
        SetupAthleteWorkouts(workout);

        var handler = new SkipWorkoutCommandHandler(_contextMock.Object);
        var command = new SkipWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = workout.Id,
            Reason = "Feeling sick"
        };

        await handler.Handle(command, CancellationToken.None);

        workout.Status.Should().Be(WorkoutStatus.Skipped);
        workout.SkippedReason.Should().Be("Feeling sick");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SkipWorkout_WithNonexistentWorkout_ShouldThrow()
    {
        SetupAthleteWorkouts();

        var handler = new SkipWorkoutCommandHandler(_contextMock.Object);
        var command = new SkipWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = Guid.NewGuid()
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SkipWorkout_WhenCompleted_ShouldThrow()
    {
        var program = CreateAthleteProgram();
        var workout = CreateWorkout(program);
        workout.Complete();
        SetupAthleteWorkouts(workout);

        var handler = new SkipWorkoutCommandHandler(_contextMock.Object);
        var command = new SkipWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = workout.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========================
    // CompleteWorkoutCommandHandler Tests
    // ========================

    [Fact]
    public async Task CompleteWorkout_WithValidData_ShouldComplete()
    {
        var program = CreateAthleteProgram();
        var workout = CreateWorkout(program);
        SetupAthleteWorkouts(workout);

        var handler = new CompleteWorkoutCommandHandler(_contextMock.Object);
        var command = new CompleteWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = workout.Id,
            DurationMinutes = 75,
            FatigueRating = 8,
            Notes = "Tough session"
        };

        await handler.Handle(command, CancellationToken.None);

        workout.IsCompleted.Should().BeTrue();
        workout.CompletedDate.Should().NotBeNull();
        workout.DurationMinutes.Should().Be(75);
        workout.FatigueRating.Should().Be(8);
        workout.Notes.Should().Be("Tough session");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteWorkout_WhenAlreadyCompleted_ShouldThrow()
    {
        var program = CreateAthleteProgram();
        var workout = CreateWorkout(program);
        workout.Complete();
        SetupAthleteWorkouts(workout);

        var handler = new CompleteWorkoutCommandHandler(_contextMock.Object);
        var command = new CompleteWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = workout.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CompleteWorkout_WithNonexistentWorkout_ShouldThrow()
    {
        SetupAthleteWorkouts();

        var handler = new CompleteWorkoutCommandHandler(_contextMock.Object);
        var command = new CompleteWorkoutCommand
        {
            AthleteId = _validAthleteId,
            WorkoutId = Guid.NewGuid()
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
