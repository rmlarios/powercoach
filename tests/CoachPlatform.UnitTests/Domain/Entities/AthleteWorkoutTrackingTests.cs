using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;

namespace CoachPlatform.UnitTests.Domain.Entities;

public class AthleteWorkoutTrackingTests
{
    private readonly Guid _validProgramId = Guid.NewGuid();

    private AthleteWorkout CreateWorkout(int week = 1, int day = 1)
    {
        // Use reflection to invoke internal Create method
        var method = typeof(AthleteWorkout).GetMethod("Create",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        return (AthleteWorkout)method!.Invoke(null, [_validProgramId, week, day, DateTime.UtcNow.Date])!;
    }

    // ========================
    // Start() Tests
    // ========================

    [Fact]
    public void Start_FromNotStarted_ShouldSetStatusAndStartedAt()
    {
        var workout = CreateWorkout();

        workout.Start();

        workout.Status.Should().Be(WorkoutStatus.InProgress);
        workout.StartedAt.Should().NotBeNull();
        workout.StartedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Start_WhenAlreadyInProgress_ShouldThrow()
    {
        var workout = CreateWorkout();
        workout.Start();

        var act = () => workout.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already in progress*");
    }

    [Fact]
    public void Start_WhenCompleted_ShouldThrow()
    {
        var workout = CreateWorkout();
        workout.Complete();

        var act = () => workout.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void Start_WhenSkipped_ShouldThrow()
    {
        var workout = CreateWorkout();
        workout.Skip();

        var act = () => workout.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*skipped*");
    }

    // ========================
    // Skip() Tests
    // ========================

    [Fact]
    public void Skip_FromNotStarted_ShouldSetStatusAndReason()
    {
        var workout = CreateWorkout();

        workout.Skip("Traveling");

        workout.Status.Should().Be(WorkoutStatus.Skipped);
        workout.SkippedReason.Should().Be("Traveling");
    }

    [Fact]
    public void Skip_WithoutReason_ShouldWork()
    {
        var workout = CreateWorkout();

        workout.Skip();

        workout.Status.Should().Be(WorkoutStatus.Skipped);
        workout.SkippedReason.Should().BeNull();
    }

    [Fact]
    public void Skip_WhenCompleted_ShouldThrow()
    {
        var workout = CreateWorkout();
        workout.Complete();

        var act = () => workout.Skip();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    [Fact]
    public void Skip_WhenInProgress_ShouldThrow()
    {
        var workout = CreateWorkout();
        workout.Start();

        var act = () => workout.Skip();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*in progress*");
    }

    // ========================
    // Complete() Tests
    // ========================

    [Fact]
    public void Complete_ShouldSetIsCompletedAndCompletedDate()
    {
        var workout = CreateWorkout();

        workout.Complete(durationMinutes: 60, fatigueRating: 7, notes: "Good session");

        workout.IsCompleted.Should().BeTrue();
        workout.CompletedDate.Should().NotBeNull();
        workout.DurationMinutes.Should().Be(60);
        workout.FatigueRating.Should().Be(7);
        workout.Notes.Should().Be("Good session");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var workout = CreateWorkout();
        workout.Complete();

        var act = () => workout.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already completed*");
    }

    [Fact]
    public void Complete_WithInvalidFatigueRating_ShouldThrow()
    {
        var workout = CreateWorkout();

        var act = () => workout.Complete(fatigueRating: 11);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Fatigue rating*");
    }

    [Fact]
    public void Complete_WithNoSetsCompleted_ShouldBePartiallyCompleted()
    {
        var workout = CreateWorkout();
        // No exercise logs → PartiallyCompleted (0 completed logs)

        workout.Complete();

        // With 0 exercise logs, allSetsCompleted is false
        workout.Status.Should().Be(WorkoutStatus.PartiallyCompleted);
        workout.IsCompleted.Should().BeTrue();
    }

    // ========================
    // Create() Tests
    // ========================

    [Fact]
    public void Create_ShouldSetDefaultStatus()
    {
        var workout = CreateWorkout();

        workout.Status.Should().Be(WorkoutStatus.NotStarted);
        workout.IsCompleted.Should().BeFalse();
        workout.StartedAt.Should().BeNull();
    }

    [Fact]
    public void Reschedule_WhenNotCompleted_ShouldUpdateDate()
    {
        var workout = CreateWorkout();
        var newDate = DateTime.UtcNow.AddDays(3);

        workout.Reschedule(newDate);

        workout.ScheduledDate.Should().Be(newDate);
    }

    [Fact]
    public void Reschedule_WhenCompleted_ShouldThrow()
    {
        var workout = CreateWorkout();
        workout.Complete();

        var act = () => workout.Reschedule(DateTime.UtcNow.AddDays(1));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }
}
