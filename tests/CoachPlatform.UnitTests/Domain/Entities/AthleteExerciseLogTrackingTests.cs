using CoachPlatform.Domain.Entities;
using FluentAssertions;

namespace CoachPlatform.UnitTests.Domain.Entities;

public class AthleteExerciseLogTrackingTests
{
    private readonly Guid _validWorkoutId = Guid.NewGuid();
    private readonly Guid _validExerciseId = Guid.NewGuid();

    private AthleteExerciseLog CreateLog(int setNumber = 1, int reps = 5, decimal weight = 100m,
        decimal? rpe = null, int? targetReps = null, decimal? targetWeight = null)
    {
        var method = typeof(AthleteExerciseLog).GetMethod("Create",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        return (AthleteExerciseLog)method!.Invoke(null,
            [_validWorkoutId, _validExerciseId, setNumber, reps, weight, rpe, (string?)null, targetReps, targetWeight])!;
    }

    // ========================
    // Create() Tests
    // ========================

    [Fact]
    public void Create_WithValidData_ShouldCreateLog()
    {
        var log = CreateLog(setNumber: 2, reps: 8, weight: 80m, rpe: 7.5m);

        log.WorkoutId.Should().Be(_validWorkoutId);
        log.ExerciseId.Should().Be(_validExerciseId);
        log.SetNumber.Should().Be(2);
        log.Reps.Should().Be(8);
        log.Weight.Should().Be(80m);
        log.Rpe.Should().Be(7.5m);
        log.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithTargets_ShouldSetTargetValues()
    {
        var log = CreateLog(reps: 0, weight: 0, targetReps: 5, targetWeight: 100m);

        log.TargetReps.Should().Be(5);
        log.TargetWeight.Should().Be(100m);
    }

    [Fact]
    public void Create_WithEmptyWorkoutId_ShouldThrow()
    {
        var method = typeof(AthleteExerciseLog).GetMethod("Create",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        var act = () => method!.Invoke(null,
            [Guid.Empty, _validExerciseId, 1, 5, 100m, (decimal?)null, (string?)null, (int?)null, (decimal?)null]);

        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<ArgumentException>()
            .WithMessage("*Workout ID*");
    }

    [Fact]
    public void Create_WithInvalidSetNumber_ShouldThrow()
    {
        var method = typeof(AthleteExerciseLog).GetMethod("Create",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        var act = () => method!.Invoke(null,
            [_validWorkoutId, _validExerciseId, 0, 5, 100m, (decimal?)null, (string?)null, (int?)null, (decimal?)null]);

        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<ArgumentException>()
            .WithMessage("*Set number*");
    }

    [Fact]
    public void Create_WithInvalidRpe_ShouldThrow()
    {
        var method = typeof(AthleteExerciseLog).GetMethod("Create",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        var act = () => method!.Invoke(null,
            [_validWorkoutId, _validExerciseId, 1, 5, 100m, (decimal?)11m, (string?)null, (int?)null, (decimal?)null]);

        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<ArgumentException>()
            .WithMessage("*RPE*");
    }

    // ========================
    // Update() Tests
    // ========================

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        var log = CreateLog(reps: 5, weight: 100m);

        log.Update(reps: 8, weight: 85m, rpe: 7m, notes: "Felt easy");

        log.Reps.Should().Be(8);
        log.Weight.Should().Be(85m);
        log.Rpe.Should().Be(7m);
        log.Notes.Should().Be("Felt easy");
    }

    [Fact]
    public void Update_WithInvalidReps_ShouldThrow()
    {
        var log = CreateLog();

        var act = () => log.Update(reps: -1, weight: 100m, rpe: null, notes: null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Reps*");
    }

    [Fact]
    public void Update_WithInvalidWeight_ShouldThrow()
    {
        var log = CreateLog();

        var act = () => log.Update(reps: 5, weight: 1500m, rpe: null, notes: null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Weight*");
    }

    // ========================
    // Complete() Tests
    // ========================

    [Fact]
    public void Complete_WithReps_ShouldMarkCompleted()
    {
        var log = CreateLog(reps: 5, weight: 100m);

        log.Complete();

        log.IsCompleted.Should().BeTrue();
        log.SkippedReason.Should().BeNull();
    }

    [Fact]
    public void Complete_WithZeroReps_ShouldThrow()
    {
        var log = CreateLog(reps: 0, weight: 0m);

        var act = () => log.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*0 reps*");
    }

    // ========================
    // Skip() Tests
    // ========================

    [Fact]
    public void Skip_WithReason_ShouldSetReasonAndNotComplete()
    {
        var log = CreateLog(reps: 5, weight: 100m);

        log.Skip("Shoulder pain");

        log.IsCompleted.Should().BeFalse();
        log.SkippedReason.Should().Be("Shoulder pain");
    }

    [Fact]
    public void Skip_WithoutReason_ShouldWork()
    {
        var log = CreateLog();

        log.Skip();

        log.IsCompleted.Should().BeFalse();
        log.SkippedReason.Should().BeNull();
    }

    // ========================
    // SetTargets() Tests
    // ========================

    [Fact]
    public void SetTargets_ShouldUpdateTargetValues()
    {
        var log = CreateLog();

        log.SetTargets(targetReps: 8, targetWeight: 90m);

        log.TargetReps.Should().Be(8);
        log.TargetWeight.Should().Be(90m);
    }

    [Fact]
    public void SetTargets_WithNulls_ShouldClearTargets()
    {
        var log = CreateLog(targetReps: 5, targetWeight: 100m);

        log.SetTargets(targetReps: null, targetWeight: null);

        log.TargetReps.Should().BeNull();
        log.TargetWeight.Should().BeNull();
    }
}
