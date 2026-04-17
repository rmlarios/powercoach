using CoachPlatform.Application.Features.Dashboard.Queries.GetCoachDashboard;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Queries;

public class GetCoachDashboardQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetCoachDashboardQueryHandler _handler;
    private readonly Guid _coachId = Guid.NewGuid();

    public GetCoachDashboardQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetCoachDashboardQueryHandler(_contextMock.Object);
    }

    // ═══════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProp = typeof(T).BaseType?.BaseType?.GetProperty("Id")
                     ?? typeof(T).BaseType?.GetProperty("Id");
        idProp?.SetValue(entity, id);
    }

    private static void SetProperty<T>(T entity, string propertyName, object? value) where T : class
    {
        typeof(T).GetProperty(propertyName)!.SetValue(entity, value);
    }

    private Athlete CreateAthlete(string firstName = "Juan", string lastName = "Perez", Guid? id = null)
    {
        var emailSafe = $"{Guid.NewGuid():N}@test.com";
        var athlete = Athlete.Create(_coachId, firstName, lastName, emailSafe);
        SetEntityId(athlete, id ?? Guid.NewGuid());
        return athlete;
    }

    private CoachPlatform.Domain.Entities.Application CreateApplication(string firstName = "Carlos", string lastName = "Lopez", Guid? id = null)
    {
        var emailSafe = $"{Guid.NewGuid():N}@test.com";
        var app = CoachPlatform.Domain.Entities.Application.Create(_coachId, firstName, lastName, emailSafe);
        SetEntityId(app, id ?? Guid.NewGuid());
        return app;
    }

    private CheckIn CreateCheckIn(Guid athleteId, DateTime? checkInDate = null, bool reviewed = false)
    {
        var ci = CheckIn.Create(athleteId, checkInDate ?? DateTime.UtcNow);
        SetEntityId(ci, Guid.NewGuid());
        if (reviewed) ci.MarkAsReviewed();
        return ci;
    }

    private ProgramTemplate CreateProgramTemplate(string name = "Fuerza 5x5")
    {
        var template = ProgramTemplate.Create(_coachId, name, "Desc", 8);
        SetEntityId(template, Guid.NewGuid());
        return template;
    }

    private AthleteProgram CreateActiveProgram(Guid athleteId, Guid templateId, Guid? id = null)
    {
        var ap = AthleteProgram.Create(athleteId, templateId, DateTime.UtcNow.Date);
        ap.Start();
        SetEntityId(ap, id ?? Guid.NewGuid());
        return ap;
    }

    private AthleteWorkout CreateWorkout(AthleteProgram program, int week, int day, DateTime scheduledDate, Guid? id = null)
    {
        var workout = program.AddWorkout(week, day, scheduledDate);
        SetEntityId(workout, id ?? Guid.NewGuid());
        return workout;
    }

    private Subscription CreateActiveSubscription(Guid athleteId, DateTime endDate)
    {
        var plan = Plan.CreateMonthly(_coachId, "Pro Plan", 50, "USD");
        SetEntityId(plan, Guid.NewGuid());

        var sub = Subscription.Create(athleteId, plan, DateTime.UtcNow.AddDays(-20));
        sub.Activate();
        SetEntityId(sub, Guid.NewGuid());

        // Override EndDate via reflection to control test scenario
        SetProperty(sub, nameof(Subscription.EndDate), endDate);
        return sub;
    }

    /// <summary>
    /// Sets up empty DbSets for all context properties the handler may touch.
    /// Call this first, then override specific DbSets for your scenario.
    /// </summary>
    private void SetupEmptyContext()
    {
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(Array.Empty<Athlete>());
        _contextMock.Setup(x => x.Applications).ReturnsDbSet(Array.Empty<CoachPlatform.Domain.Entities.Application>());
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(Array.Empty<AthleteProgram>());
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(Array.Empty<AthleteWorkout>());
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(Array.Empty<CheckIn>());
        _contextMock.Setup(x => x.Subscriptions).ReturnsDbSet(Array.Empty<Subscription>());
        _contextMock.Setup(x => x.ProgramTemplates).ReturnsDbSet(Array.Empty<ProgramTemplate>());
    }

    // ═══════════════════════════════════════════
    // 1. Stats Tests
    // ═══════════════════════════════════════════

    [Fact]
    public async Task Handle_WithActiveAthletes_ShouldReturnCorrectStats()
    {
        // Arrange
        var athlete1 = CreateAthlete("Ana", "García");
        var athlete2 = CreateAthlete("Pedro", "Martínez");
        var inactiveAthlete = CreateAthlete("María", "Inactiva");
        inactiveAthlete.Deactivate();

        var template = CreateProgramTemplate();
        var program1 = CreateActiveProgram(athlete1.Id, template.Id);
        var program2 = CreateActiveProgram(athlete2.Id, template.Id);

        var today = DateTime.UtcNow.Date;

        // Workouts in last 7 days: 2 completed, 1 not started
        var w1 = CreateWorkout(program1, 1, 1, today.AddDays(-3));
        w1.Complete(45, 6);
        SetProperty(w1, nameof(AthleteWorkout.CompletedDate), today.AddDays(-3));

        var w2 = CreateWorkout(program2, 1, 1, today.AddDays(-1));
        w2.Complete(60, 5);
        SetProperty(w2, nameof(AthleteWorkout.CompletedDate), today.AddDays(-1));

        var w3 = CreateWorkout(program1, 1, 2, today);

        // Pending application
        var pendingApp = CreateApplication();

        // Unreviewed check-in
        var checkIn = CreateCheckIn(athlete1.Id, today.AddDays(-1));

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete1, athlete2, inactiveAthlete });
        _contextMock.Setup(x => x.Applications).ReturnsDbSet(new[] { pendingApp });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program1, program2 });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { w1, w2, w3 });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { checkIn });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Stats.ActiveAthletes.Should().Be(2); // excludes inactive
        result.Stats.PendingApplications.Should().Be(1);
        result.Stats.ActivePrograms.Should().Be(2);
        result.Stats.CompletionRate.Should().BeGreaterThan(0); // 2 completed out of 3
        result.Stats.PendingCheckIns.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoAthletes_ShouldReturnEmptyDashboard()
    {
        // Arrange
        SetupEmptyContext();
        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Stats.ActiveAthletes.Should().Be(0);
        result.Stats.PendingApplications.Should().Be(0);
        result.Stats.ActivePrograms.Should().Be(0);
        result.Stats.CompletionRate.Should().Be(0);
        result.Stats.PendingCheckIns.Should().Be(0);
        result.Stats.AthletesTrend.Should().Be(0);
        result.Alerts.Should().BeEmpty();
        result.AthleteStatuses.Should().BeEmpty();
        result.RecentActivity.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════
    // 2. Alert Tests
    // ═══════════════════════════════════════════

    [Fact]
    public async Task Handle_WithHighFatigueWorkout_ShouldGenerateWarningAlert()
    {
        // Arrange
        var athlete = CreateAthlete("Luis", "Fatiga");
        var template = CreateProgramTemplate();
        var program = CreateActiveProgram(athlete.Id, template.Id);

        var today = DateTime.UtcNow.Date;
        var workout = CreateWorkout(program, 1, 1, today.AddDays(-1));
        workout.Complete(50, 9); // High fatigue = 9/10
        SetProperty(workout, nameof(AthleteWorkout.CompletedDate), today.AddDays(-1));

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { workout });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Alerts.Should().Contain(a => a.AlertType == "HighFatigue" && a.Severity == "warning");
        var fatigueAlert = result.Alerts.First(a => a.AlertType == "HighFatigue");
        fatigueAlert.AthleteName.Should().Contain("Luis");
        fatigueAlert.Message.Should().Contain("fatiga");
        fatigueAlert.Message.Should().Contain("9/10");
    }

    [Fact]
    public async Task Handle_WithMissedWorkout_ShouldGenerateDangerAlert()
    {
        // Arrange — athlete has active program but no completed workout in 2+ days
        var athlete = CreateAthlete("Sara", "Ausente");
        var template = CreateProgramTemplate();
        var program = CreateActiveProgram(athlete.Id, template.Id);

        // Workout was 5 days ago — too old, counts as missed
        var oldWorkout = CreateWorkout(program, 1, 1, DateTime.UtcNow.Date.AddDays(-5));
        oldWorkout.Complete(45, 5);
        SetProperty(oldWorkout, nameof(AthleteWorkout.CompletedDate), DateTime.UtcNow.Date.AddDays(-5));

        // Recent check-in so we don't also trigger MissedCheckIn
        var checkIn = CreateCheckIn(athlete.Id, DateTime.UtcNow.Date.AddDays(-1));

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { oldWorkout });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { checkIn });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Alerts.Should().Contain(a => a.AlertType == "MissedWorkout" && a.Severity == "danger");
        var missedAlert = result.Alerts.First(a => a.AlertType == "MissedWorkout");
        missedAlert.AthleteName.Should().Contain("Sara");
        missedAlert.Message.Should().Contain("no ha entrenado");
    }

    [Fact]
    public async Task Handle_WithUnreviewedCheckIn_ShouldCountAsPending()
    {
        // Arrange
        var athlete = CreateAthlete("Diego", "Review");
        var ci1 = CreateCheckIn(athlete.Id, DateTime.UtcNow.Date.AddDays(-2), reviewed: false);
        var ci2 = CreateCheckIn(athlete.Id, DateTime.UtcNow.Date.AddDays(-5), reviewed: true);

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { ci1, ci2 });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Stats.PendingCheckIns.Should().Be(1); // only 1 unreviewed
    }

    [Fact]
    public async Task Handle_WithMissedCheckIn_ShouldGenerateAlert()
    {
        // Arrange — athlete with no check-in in 7+ days
        var athlete = CreateAthlete("Marta", "SinCheckIn");

        // Last check-in was 10 days ago
        var oldCheckIn = CreateCheckIn(athlete.Id, DateTime.UtcNow.Date.AddDays(-10), reviewed: true);

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { oldCheckIn });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Alerts.Should().Contain(a => a.AlertType == "MissedCheckIn" && a.Severity == "warning");
        var alert = result.Alerts.First(a => a.AlertType == "MissedCheckIn");
        alert.AthleteName.Should().Contain("Marta");
        alert.Message.Should().Contain("check-in");
    }

    [Fact]
    public async Task Handle_WithExpiringSubscription_ShouldGenerateAlert()
    {
        // Arrange
        var athlete = CreateAthlete("Raúl", "Expira");
        var sub = CreateActiveSubscription(athlete.Id, DateTime.UtcNow.Date.AddDays(3)); // Expires in 3 days

        // Recent check-in to avoid MissedCheckIn noise
        var checkIn = CreateCheckIn(athlete.Id, DateTime.UtcNow.Date.AddDays(-1));

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.Subscriptions).ReturnsDbSet(new[] { sub });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { checkIn });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Alerts.Should().Contain(a => a.AlertType == "ExpiringSubscription" && a.Severity == "warning");
        var alert = result.Alerts.First(a => a.AlertType == "ExpiringSubscription");
        alert.AthleteName.Should().Contain("Raúl");
        alert.Message.Should().Contain("expira en 3 días");
    }

    [Fact]
    public async Task Handle_WithStaleApplication_ShouldGenerateAlert()
    {
        // Arrange — application older than 48 hours
        var app = CreateApplication("Nuevo", "Aspirante");
        app.SetAuditTimestamps(DateTime.UtcNow.AddHours(-50), DateTime.UtcNow.AddHours(-50));

        SetupEmptyContext();
        _contextMock.Setup(x => x.Applications).ReturnsDbSet(new[] { app });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Alerts.Should().Contain(a => a.AlertType == "PendingApplication" && a.Severity == "warning");
        var alert = result.Alerts.First(a => a.AlertType == "PendingApplication");
        alert.AthleteName.Should().Contain("Nuevo");
        alert.Message.Should().Contain("48 horas");
    }

    [Fact]
    public async Task Handle_AlertsSortedDangerFirst()
    {
        // Arrange — one danger (MissedWorkout) and one warning (MissedCheckIn)
        var athlete = CreateAthlete("Test", "Sort");
        var template = CreateProgramTemplate();
        var program = CreateActiveProgram(athlete.Id, template.Id);

        // Missed workout (no recent completed workout) → danger
        // Also missed check-in (no recent check-in) → warning

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        // No workouts, no check-ins → triggers both alerts

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert — danger alerts should come before warning alerts
        var dangerAlerts = result.Alerts.Where(a => a.Severity == "danger").ToList();
        var warningAlerts = result.Alerts.Where(a => a.Severity == "warning").ToList();
        dangerAlerts.Should().NotBeEmpty();
        warningAlerts.Should().NotBeEmpty();

        // First alert should be danger
        result.Alerts.First().Severity.Should().Be("danger");
    }

    // ═══════════════════════════════════════════
    // 3. Athlete Status Tests
    // ═══════════════════════════════════════════

    [Fact]
    public async Task Handle_AthleteWithRecentWorkout_ShouldBeGreen()
    {
        // Arrange — recent workout, recent check-in reviewed
        var athlete = CreateAthlete("Verde", "OK");
        var template = CreateProgramTemplate();
        var program = CreateActiveProgram(athlete.Id, template.Id);

        var today = DateTime.UtcNow.Date;
        var workout = CreateWorkout(program, 2, 3, today.AddDays(-1));
        workout.Complete(50, 5);
        SetProperty(workout, nameof(AthleteWorkout.CompletedDate), today.AddDays(-1));

        var checkIn = CreateCheckIn(athlete.Id, today.AddDays(-2), reviewed: true);

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { workout });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { checkIn });
        _contextMock.Setup(x => x.ProgramTemplates).ReturnsDbSet(new[] { template });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AthleteStatuses.Should().HaveCount(1);
        var status = result.AthleteStatuses.First();
        status.StatusColor.Should().Be("green");
        status.Status.Should().Be("OK");
        status.Name.Should().Contain("Verde");
        status.LastWorkoutDate.Should().NotBeNull();
        status.LastWorkoutName.Should().Contain("Sem 2");
        status.LastWorkoutName.Should().Contain("Día 3");
    }

    [Fact]
    public async Task Handle_AthleteWithHighFatigue_ShouldBeRed()
    {
        // Arrange — recent workout with high fatigue
        var athlete = CreateAthlete("Rojo", "Fatiga");
        var template = CreateProgramTemplate();
        var program = CreateActiveProgram(athlete.Id, template.Id);

        var today = DateTime.UtcNow.Date;
        var workout = CreateWorkout(program, 1, 1, today.AddDays(-1));
        workout.Complete(60, 9); // High fatigue
        SetProperty(workout, nameof(AthleteWorkout.CompletedDate), today.AddDays(-1));

        var checkIn = CreateCheckIn(athlete.Id, today.AddDays(-1), reviewed: true);

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { workout });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { checkIn });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AthleteStatuses.Should().HaveCount(1);
        result.AthleteStatuses.First().StatusColor.Should().Be("red");
        result.AthleteStatuses.First().Status.Should().Be("Problema");
    }

    [Fact]
    public async Task Handle_AthleteWithUnreviewedCheckIn_ShouldBeYellow()
    {
        // Arrange — recent workout (not high fatigue) + unreviewed check-in
        var athlete = CreateAthlete("Amarillo", "Pendiente");
        var template = CreateProgramTemplate();
        var program = CreateActiveProgram(athlete.Id, template.Id);

        var today = DateTime.UtcNow.Date;
        var workout = CreateWorkout(program, 1, 1, today.AddDays(-1));
        workout.Complete(40, 4); // Low fatigue
        SetProperty(workout, nameof(AthleteWorkout.CompletedDate), today.AddDays(-1));

        var checkIn = CreateCheckIn(athlete.Id, today.AddDays(-1), reviewed: false); // Unreviewed

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { workout });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { checkIn });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AthleteStatuses.Should().HaveCount(1);
        result.AthleteStatuses.First().StatusColor.Should().Be("yellow");
        result.AthleteStatuses.First().Status.Should().Be("Atención");
    }

    [Fact]
    public async Task Handle_AthleteStatuses_SortedRedYellowGreen()
    {
        // Arrange — 3 athletes: red, yellow, green
        var redAthlete = CreateAthlete("Carlos", "Rojo"); // No recent workout → red
        var yellowAthlete = CreateAthlete("Ana", "Amarillo"); // Recent workout + unreviewed check-in → yellow
        var greenAthlete = CreateAthlete("Bruno", "Verde"); // Recent workout + reviewed check-in → green

        var template = CreateProgramTemplate();
        var programRed = CreateActiveProgram(redAthlete.Id, template.Id);
        var programYellow = CreateActiveProgram(yellowAthlete.Id, template.Id);
        var programGreen = CreateActiveProgram(greenAthlete.Id, template.Id);

        var today = DateTime.UtcNow.Date;

        // Yellow: recent workout, unreviewed check-in
        var wYellow = CreateWorkout(programYellow, 1, 1, today.AddDays(-1));
        wYellow.Complete(45, 4);
        SetProperty(wYellow, nameof(AthleteWorkout.CompletedDate), today.AddDays(-1));
        var ciYellow = CreateCheckIn(yellowAthlete.Id, today.AddDays(-1), reviewed: false);

        // Green: recent workout, reviewed check-in
        var wGreen = CreateWorkout(programGreen, 1, 1, today);
        wGreen.Complete(50, 3);
        SetProperty(wGreen, nameof(AthleteWorkout.CompletedDate), today);
        var ciGreen = CreateCheckIn(greenAthlete.Id, today.AddDays(-2), reviewed: true);

        // Red: no recent workout (check-in to avoid double alert noise)
        var ciRed = CreateCheckIn(redAthlete.Id, today.AddDays(-1), reviewed: true);

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { redAthlete, yellowAthlete, greenAthlete });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { programRed, programYellow, programGreen });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { wYellow, wGreen });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { ciYellow, ciGreen, ciRed });
        _contextMock.Setup(x => x.ProgramTemplates).ReturnsDbSet(new[] { template });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AthleteStatuses.Should().HaveCount(3);
        result.AthleteStatuses[0].StatusColor.Should().Be("red");
        result.AthleteStatuses[1].StatusColor.Should().Be("yellow");
        result.AthleteStatuses[2].StatusColor.Should().Be("green");
    }

    // ═══════════════════════════════════════════
    // 4. Recent Activity Tests
    // ═══════════════════════════════════════════

    [Fact]
    public async Task Handle_WithRecentActivity_ShouldReturnOrderedFeed()
    {
        // Arrange
        var athlete = CreateAthlete("María", "Activa");
        var template = CreateProgramTemplate();
        var program = CreateActiveProgram(athlete.Id, template.Id);

        var today = DateTime.UtcNow.Date;

        // Completed workout yesterday
        var w1 = CreateWorkout(program, 1, 1, today.AddDays(-1));
        w1.Complete(55, 5, "Good session");
        SetProperty(w1, nameof(AthleteWorkout.CompletedDate), today.AddDays(-1));

        // In-progress workout today
        var w2 = CreateWorkout(program, 1, 2, today);
        w2.Start();
        SetProperty(w2, nameof(AthleteWorkout.StartedAt), today);

        // Check-in today
        var checkIn = CreateCheckIn(athlete.Id, today);
        checkIn.SetWeight(72.5m);

        // Recent application (from a different person, same coach)
        var app = CreateApplication("Nuevo", "Postulante");
        app.SetAuditTimestamps(today.AddHours(-5), today.AddHours(-5));

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { program });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { w1, w2 });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { checkIn });
        _contextMock.Setup(x => x.Applications).ReturnsDbSet(new[] { app });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.RecentActivity.Should().NotBeEmpty();
        result.RecentActivity.Should().HaveCountGreaterThanOrEqualTo(3); // workout completed, started, check-in (+app)

        // Should contain various activity types
        result.RecentActivity.Should().Contain(a => a.Type == "WorkoutCompleted");
        result.RecentActivity.Should().Contain(a => a.Type == "CheckInSubmitted");
        result.RecentActivity.Should().Contain(a => a.Type == "ApplicationReceived");

        // Should be ordered by timestamp descending (most recent first)
        var timestamps = result.RecentActivity.Select(a => a.Timestamp).ToList();
        timestamps.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_RecentActivity_LimitedTo20Items()
    {
        // Arrange — create many check-ins to exceed 20 limit
        var athlete = CreateAthlete("Prolífico", "Test");
        var today = DateTime.UtcNow.Date;

        var checkIns = Enumerable.Range(0, 25).Select(i =>
        {
            var ci = CreateCheckIn(athlete.Id, today.AddHours(-i));
            return ci;
        }).ToList();

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(checkIns);

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.RecentActivity.Should().HaveCountLessThanOrEqualTo(20);
    }

    // ═══════════════════════════════════════════
    // 5. Cross-section Integration Tests
    // ═══════════════════════════════════════════

    [Fact]
    public async Task Handle_FullScenario_ReturnsAllSections()
    {
        // Arrange — a realistic scenario with multiple athletes and data
        var athlete1 = CreateAthlete("Jorge", "Sano");    // Green status
        var athlete2 = CreateAthlete("Laura", "Cansada"); // Red status (high fatigue)

        var template = CreateProgramTemplate("Hipertrofia 4x");
        var prog1 = CreateActiveProgram(athlete1.Id, template.Id);
        var prog2 = CreateActiveProgram(athlete2.Id, template.Id);

        var today = DateTime.UtcNow.Date;

        // Athlete 1: recent workout (low fatigue) + reviewed check-in → green
        var w1 = CreateWorkout(prog1, 3, 2, today);
        w1.Complete(50, 4);
        SetProperty(w1, nameof(AthleteWorkout.CompletedDate), today);

        var ci1 = CreateCheckIn(athlete1.Id, today.AddDays(-1), reviewed: true);

        // Athlete 2: recent workout (high fatigue 10) → red
        var w2 = CreateWorkout(prog2, 2, 1, today.AddDays(-1));
        w2.Complete(70, 10);
        SetProperty(w2, nameof(AthleteWorkout.CompletedDate), today.AddDays(-1));

        var ci2 = CreateCheckIn(athlete2.Id, today.AddDays(-2), reviewed: true);

        // Pending application
        var app = CreateApplication("Diego", "Nuevo");

        // Expiring subscription
        var sub = CreateActiveSubscription(athlete1.Id, today.AddDays(5));

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { athlete1, athlete2 });
        _contextMock.Setup(x => x.AthletePrograms).ReturnsDbSet(new[] { prog1, prog2 });
        _contextMock.Setup(x => x.AthleteWorkouts).ReturnsDbSet(new[] { w1, w2 });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { ci1, ci2 });
        _contextMock.Setup(x => x.Applications).ReturnsDbSet(new[] { app });
        _contextMock.Setup(x => x.Subscriptions).ReturnsDbSet(new[] { sub });
        _contextMock.Setup(x => x.ProgramTemplates).ReturnsDbSet(new[] { template });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert — All 4 sections populated
        result.Stats.Should().NotBeNull();
        result.Stats.ActiveAthletes.Should().Be(2);
        result.Stats.ActivePrograms.Should().Be(2);
        result.Stats.PendingApplications.Should().Be(1);

        result.Alerts.Should().NotBeEmpty();
        result.Alerts.Should().Contain(a => a.AlertType == "HighFatigue");
        result.Alerts.Should().Contain(a => a.AlertType == "ExpiringSubscription");

        result.AthleteStatuses.Should().HaveCount(2);
        // Athlete 2 (high fatigue) should be red, before athlete 1 (green)
        result.AthleteStatuses.First().StatusColor.Should().Be("red");
        result.AthleteStatuses.First().Name.Should().Contain("Laura");

        result.RecentActivity.Should().NotBeEmpty();
        result.RecentActivity.Should().Contain(a => a.Type == "WorkoutCompleted");
    }

    [Fact]
    public async Task Handle_OnlyReturnsDataForCoachAthletes()
    {
        // Arrange — athlete from a different coach should not appear
        var myAthlete = CreateAthlete("MiAtleta", "Mío");
        var otherCoachId = Guid.NewGuid();
        var otherAthlete = Athlete.Create(otherCoachId, "Otro", "Ajeno", $"{Guid.NewGuid():N}@test.com");
        SetEntityId(otherAthlete, Guid.NewGuid());

        var otherApp = CoachPlatform.Domain.Entities.Application.Create(otherCoachId, "Ajeno", "App", $"{Guid.NewGuid():N}@test.com");
        SetEntityId(otherApp, Guid.NewGuid());

        var checkIn = CreateCheckIn(myAthlete.Id, DateTime.UtcNow.Date.AddDays(-1));

        SetupEmptyContext();
        _contextMock.Setup(x => x.Athletes).ReturnsDbSet(new[] { myAthlete, otherAthlete });
        _contextMock.Setup(x => x.Applications).ReturnsDbSet(new[] { otherApp });
        _contextMock.Setup(x => x.CheckIns).ReturnsDbSet(new[] { checkIn });

        var query = new GetCoachDashboardQuery { CoachId = _coachId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Stats.ActiveAthletes.Should().Be(1); // Only my athlete
        result.Stats.PendingApplications.Should().Be(0); // Other coach's app excluded
        result.AthleteStatuses.Should().HaveCount(1);
        result.AthleteStatuses.First().Name.Should().Contain("Miatleta");
    }
}
