using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWeekWorkouts;
using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetExerciseLiftHistory;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Queries;

public class WorkoutTrackingQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Guid _coachId = Guid.NewGuid();
    private readonly Guid _athleteId = Guid.NewGuid();
    private readonly Guid _programTemplateId = Guid.NewGuid();

    public WorkoutTrackingQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    // ========================
    // Helpers
    // ========================

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProp = typeof(T).BaseType?.BaseType?.GetProperty("Id")
                     ?? typeof(T).BaseType?.GetProperty("Id");
        idProp?.SetValue(entity, id);
    }

    private ProgramTemplate CreateProgramTemplate(int weeks = 4)
    {
        var template = ProgramTemplate.Create(_coachId, "Test Program", "Desc", weeks);
        SetEntityId(template, _programTemplateId);
        return template;
    }

    private AthleteProgram CreateAthleteProgram(ProgramTemplate? template = null)
    {
        var ap = AthleteProgram.Create(_athleteId, _programTemplateId, DateTime.UtcNow.Date);
        ap.Start(); // Handler filters for Active status
        SetEntityId(ap, Guid.NewGuid());
        // Set the ProgramTemplate navigation property via reflection
        if (template != null)
        {
            typeof(AthleteProgram).GetProperty(nameof(AthleteProgram.ProgramTemplate))!
                .SetValue(ap, template);
        }
        return ap;
    }

    private AthleteWorkout CreateWorkout(AthleteProgram program, int week, int day, DateTime scheduledDate)
    {
        var workout = program.AddWorkout(week, day, scheduledDate);
        SetEntityId(workout, Guid.NewGuid());
        typeof(AthleteWorkout).GetProperty(nameof(AthleteWorkout.AthleteProgram))!
            .SetValue(workout, program);
        return workout;
    }

    private Exercise CreateExercise(string name = "Bench Press")
    {
        var exercise = Exercise.Create(_coachId, name, ExerciseCategory.Bench, MuscleGroup.Chest);
        SetEntityId(exercise, Guid.NewGuid());
        exercise.SetInstructions(new[] { "Lie on bench", "Grip bar", "Press up" });
        exercise.SetCoachingCues(new[] { "Arch back", "Drive feet" });
        exercise.SetVideoUrl("https://example.com/bench.mp4");
        return exercise;
    }

    private AthleteExerciseLog AddCompletedSet(AthleteWorkout workout, Exercise exercise, int setNumber, int reps, decimal weight, decimal? rpe = null)
    {
        var log = workout.LogExercise(exercise.Id, setNumber, reps, weight, rpe);
        log.Complete();
        // Set the Exercise navigation property
        typeof(AthleteExerciseLog).GetProperty(nameof(AthleteExerciseLog.Exercise))!
            .SetValue(log, exercise);
        typeof(AthleteExerciseLog).GetProperty(nameof(AthleteExerciseLog.Workout))!
            .SetValue(log, workout);
        SetEntityId(log, Guid.NewGuid());
        return log;
    }

    // ========================
    // GetWeekWorkoutsQueryHandler Tests
    // ========================

    [Fact]
    public async Task GetWeekWorkouts_WithActiveProgram_ReturnsWeekDays()
    {
        // Arrange
        var template = CreateProgramTemplate(4);
        var week1 = template.AddWeek(1, "Week 1");
        SetEntityId(week1, Guid.NewGuid());
        var day1 = week1.AddDay(1, DayFocus.Squat, "Squat Day");
        SetEntityId(day1, Guid.NewGuid());
        var day2 = week1.AddDay(2, DayFocus.Bench, "Bench Day");
        SetEntityId(day2, Guid.NewGuid());
        var day3 = week1.AddDay(3, DayFocus.Deadlift, "Pull Day");
        SetEntityId(day3, Guid.NewGuid());

        // Set WeekTemplate navigation for each day (needed for in-memory LINQ)
        foreach (var d in week1.Days)
        {
            typeof(ProgramDayTemplate).GetProperty(nameof(ProgramDayTemplate.WeekTemplate))!
                .SetValue(d, week1);
        }

        var program = CreateAthleteProgram(template);
        var today = DateTime.UtcNow.Date;

        var w1 = CreateWorkout(program, 1, 1, today.AddDays(-2));
        w1.Complete();
        var w2 = CreateWorkout(program, 1, 2, today);
        var w3 = CreateWorkout(program, 1, 3, today.AddDays(2));

        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { w1, w2, w3 });
        _contextMock.Setup(x => x.ProgramWeekTemplates).ReturnsDbSet(new[] { week1 });
        _contextMock.Setup(x => x.ProgramDayTemplates).ReturnsDbSet(week1.Days.ToList());

        var handler = new GetWeekWorkoutsQueryHandler(_contextMock.Object);
        var query = new GetWeekWorkoutsQuery { AthleteId = _athleteId, WeekNumber = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ProgramName.Should().Be("Test Program");
        result.WeekNumber.Should().Be(1);
        result.TotalWeeks.Should().Be(1); // Only 1 week added to DbSet
        result.Days.Should().HaveCount(3);
        result.Days[0].DayNumber.Should().Be(1);
        result.Days[0].Status.Should().Be(WorkoutStatus.PartiallyCompleted); // Complete() with no logs = PartiallyCompleted
        result.Days[1].DayNumber.Should().Be(2);
        result.Days[1].IsToday.Should().BeTrue();
        result.Days[2].DayNumber.Should().Be(3);
        result.Days[2].Status.Should().Be(WorkoutStatus.NotStarted);
    }

    [Fact]
    public async Task GetWeekWorkouts_WithNoActiveProgram_ReturnsNull()
    {
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(Array.Empty<AthleteProgram>());

        var handler = new GetWeekWorkoutsQueryHandler(_contextMock.Object);
        var query = new GetWeekWorkoutsQuery { AthleteId = _athleteId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWeekWorkouts_WithNoWeekNumber_InfersCurrentWeek()
    {
        var template = CreateProgramTemplate(4);
        var week2 = template.AddWeek(1);
        SetEntityId(week2, Guid.NewGuid());

        var program = CreateAthleteProgram(template);
        var today = DateTime.UtcNow.Date;

        // Only week 2 workouts exist, with today's scheduled
        var w1 = CreateWorkout(program, 2, 1, today);

        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { w1 });
        _contextMock.Setup(x => x.ProgramWeekTemplates).ReturnsDbSet(new[] { week2 });
        _contextMock.Setup(x => x.ProgramDayTemplates).ReturnsDbSet(Array.Empty<ProgramDayTemplate>());

        var handler = new GetWeekWorkoutsQueryHandler(_contextMock.Object);
        var query = new GetWeekWorkoutsQuery { AthleteId = _athleteId }; // No weekNumber

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.WeekNumber.Should().Be(2); // Inferred from today's workout
        result.Days.Should().HaveCount(1);
    }

    // ========================
    // GetExerciseLiftHistoryQueryHandler Tests
    // ========================

    [Fact]
    public async Task GetExerciseLiftHistory_WithData_ReturnsEntriesAndPRs()
    {
        // Arrange
        var exercise = CreateExercise("Bench Press");
        var template = CreateProgramTemplate();
        var program = CreateAthleteProgram(template);

        var w1 = CreateWorkout(program, 1, 1, DateTime.UtcNow.Date.AddDays(-7));
        AddCompletedSet(w1, exercise, 1, 8, 100, 7);
        AddCompletedSet(w1, exercise, 2, 8, 100, 8);
        AddCompletedSet(w1, exercise, 3, 6, 105, 9);

        var w2 = CreateWorkout(program, 2, 1, DateTime.UtcNow.Date);
        AddCompletedSet(w2, exercise, 1, 8, 105, 7);
        AddCompletedSet(w2, exercise, 2, 5, 110, 9);

        // Collect all logs from both workouts
        var allLogs = w1.ExerciseLogs.Concat(w2.ExerciseLogs).ToList();

        _contextMock.Setup(x => x.Exercises).ReturnsDbSet(new[] { exercise });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteExerciseLogs).ReturnsDbSet(allLogs);

        var handler = new GetExerciseLiftHistoryQueryHandler(_contextMock.Object);
        var query = new GetExerciseLiftHistoryQuery
        {
            AthleteId = _athleteId,
            ExerciseId = exercise.Id,
            Limit = 20
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ExerciseName.Should().Be("Bench Press");
        result.VideoUrl.Should().Be("https://example.com/bench.mp4");
        result.Instructions.Should().HaveCount(3);
        result.CoachingCues.Should().HaveCount(2);
        result.IsCompound.Should().BeTrue();
        result.Entries.Should().HaveCount(2);

        // Most recent first
        result.Entries[0].MaxWeight.Should().Be(110);
        result.Entries[1].MaxWeight.Should().Be(105);

        // PRs
        result.PersonalRecords.Should().NotBeNull();
        result.PersonalRecords!.MaxWeight.Should().Be(110);
        result.PersonalRecords.MaxReps.Should().Be(8);

        // e1RM check: 110 × (1 + 5/30) = 128.3
        result.CurrentEstimated1RM.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetExerciseLiftHistory_WithNoExercise_ReturnsNull()
    {
        _contextMock.Setup(x => x.Exercises).ReturnsDbSet(Array.Empty<Exercise>());

        var handler = new GetExerciseLiftHistoryQueryHandler(_contextMock.Object);
        var query = new GetExerciseLiftHistoryQuery
        {
            AthleteId = _athleteId,
            ExerciseId = Guid.NewGuid()
        };

        var result = await handler.Handle(query, CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetExerciseLiftHistory_WithNoLogs_ReturnsEmptyEntries()
    {
        var exercise = CreateExercise();

        _contextMock.Setup(x => x.Exercises).ReturnsDbSet(new[] { exercise });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(Array.Empty<AthleteProgram>());
        _contextMock.Setup(x => x.AthleteExerciseLogs).ReturnsDbSet(Array.Empty<AthleteExerciseLog>());

        var handler = new GetExerciseLiftHistoryQueryHandler(_contextMock.Object);
        var query = new GetExerciseLiftHistoryQuery
        {
            AthleteId = _athleteId,
            ExerciseId = exercise.Id
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ExerciseName.Should().Be("Bench Press");
        result.Entries.Should().BeEmpty();
        result.PersonalRecords.Should().BeNull();
        result.CurrentEstimated1RM.Should().BeNull();
    }

    [Fact]
    public async Task GetExerciseLiftHistory_CalculatesEpleyCorrectly()
    {
        var exercise = CreateExercise();
        var template = CreateProgramTemplate();
        var program = CreateAthleteProgram(template);

        var w1 = CreateWorkout(program, 1, 1, DateTime.UtcNow.Date);
        // 100kg × 10 reps → Epley e1RM = 100 × (1 + 10/30) = 133.3
        AddCompletedSet(w1, exercise, 1, 10, 100);

        var allLogs = w1.ExerciseLogs.ToList();

        _contextMock.Setup(x => x.Exercises).ReturnsDbSet(new[] { exercise });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteExerciseLogs).ReturnsDbSet(allLogs);

        var handler = new GetExerciseLiftHistoryQueryHandler(_contextMock.Object);
        var query = new GetExerciseLiftHistoryQuery
        {
            AthleteId = _athleteId,
            ExerciseId = exercise.Id
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Entries.Should().HaveCount(1);
        // 100 × (1 + 10/30) = 133.333... → rounded to 133.3
        result.Entries[0].Estimated1RM.Should().Be(133.3m);
        result.CurrentEstimated1RM.Should().Be(133.3m);
        result.PersonalRecords!.MaxEstimated1RM.Should().Be(133.3m);
    }

    [Fact]
    public async Task GetExerciseLiftHistory_SingleRep_E1RMEqualsWeight()
    {
        var exercise = CreateExercise();
        var template = CreateProgramTemplate();
        var program = CreateAthleteProgram(template);

        var w1 = CreateWorkout(program, 1, 1, DateTime.UtcNow.Date);
        // 1RM: 150kg × 1 rep → e1RM = 150
        AddCompletedSet(w1, exercise, 1, 1, 150);

        var allLogs = w1.ExerciseLogs.ToList();

        _contextMock.Setup(x => x.Exercises).ReturnsDbSet(new[] { exercise });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteExerciseLogs).ReturnsDbSet(allLogs);

        var handler = new GetExerciseLiftHistoryQueryHandler(_contextMock.Object);
        var query = new GetExerciseLiftHistoryQuery
        {
            AthleteId = _athleteId,
            ExerciseId = exercise.Id
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result!.Entries[0].Estimated1RM.Should().Be(150m);
    }
}
