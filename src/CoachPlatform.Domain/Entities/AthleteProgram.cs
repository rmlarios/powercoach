using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// AthleteProgram entity - represents an assigned training program instance for an athlete.
/// Aggregate Root - tracks the athlete's progress through an assigned program.
/// </summary>
public class AthleteProgram : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Reference to the athlete assigned to this program.
    /// </summary>
    public Guid AthleteId { get; private set; }

    /// <summary>
    /// Reference to the program template this was created from.
    /// </summary>
    public Guid ProgramTemplateId { get; private set; }

    /// <summary>
    /// Start date of the program.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// End date of the program (calculated or actual).
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Current week the athlete is on (1-based).
    /// </summary>
    public int CurrentWeek { get; private set; }

    /// <summary>
    /// Current status of the program.
    /// </summary>
    public ProgramStatus Status { get; private set; }

    /// <summary>
    /// Optional notes from the coach about this assignment.
    /// </summary>
    public string? Notes { get; private set; }

    // Navigation properties
    public Athlete Athlete { get; private set; } = null!;
    public ProgramTemplate ProgramTemplate { get; private set; } = null!;
    private readonly List<AthleteWorkout> _workouts = [];
    public IReadOnlyCollection<AthleteWorkout> Workouts => _workouts.AsReadOnly();

    // EF Core constructor
    private AthleteProgram() { }

    private AthleteProgram(
        Guid athleteId,
        Guid programTemplateId,
        DateTime startDate,
        string? notes)
    {
        AthleteId = athleteId;
        ProgramTemplateId = programTemplateId;
        StartDate = startDate;
        CurrentWeek = 1;
        Status = ProgramStatus.NotStarted;
        Notes = notes;
    }

    /// <summary>
    /// Creates a new athlete program assignment.
    /// </summary>
    public static AthleteProgram Create(
        Guid athleteId,
        Guid programTemplateId,
        DateTime startDate,
        string? notes = null)
    {
        if (athleteId == Guid.Empty)
            throw new ArgumentException("Athlete ID is required.", nameof(athleteId));

        if (programTemplateId == Guid.Empty)
            throw new ArgumentException("Program template ID is required.", nameof(programTemplateId));

        return new AthleteProgram(athleteId, programTemplateId, startDate, notes?.Trim());
    }

    /// <summary>
    /// Starts the program (changes status from NotStarted to Active).
    /// </summary>
    public void Start()
    {
        if (Status != ProgramStatus.NotStarted)
            throw new InvalidOperationException("Can only start a program that hasn't started yet.");

        Status = ProgramStatus.Active;
    }

    /// <summary>
    /// Pauses the program.
    /// </summary>
    public void Pause()
    {
        if (Status != ProgramStatus.Active)
            throw new InvalidOperationException("Can only pause an active program.");

        Status = ProgramStatus.Paused;
    }

    /// <summary>
    /// Resumes a paused program.
    /// </summary>
    public void Resume()
    {
        if (Status != ProgramStatus.Paused)
            throw new InvalidOperationException("Can only resume a paused program.");

        Status = ProgramStatus.Active;
    }

    /// <summary>
    /// Completes the program.
    /// </summary>
    public void Complete()
    {
        if (Status != ProgramStatus.Active)
            throw new InvalidOperationException("Can only complete an active program.");

        Status = ProgramStatus.Completed;
        EndDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the program.
    /// </summary>
    public void Cancel()
    {
        if (Status == ProgramStatus.Completed || Status == ProgramStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a completed or already cancelled program.");

        Status = ProgramStatus.Cancelled;
        EndDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Advances to the next week.
    /// </summary>
    public void AdvanceToNextWeek()
    {
        if (Status != ProgramStatus.Active)
            throw new InvalidOperationException("Can only advance weeks in an active program.");

        CurrentWeek++;
    }

    /// <summary>
    /// Sets the current week manually (for adjustments).
    /// </summary>
    public void SetCurrentWeek(int week)
    {
        if (week < 1)
            throw new ArgumentException("Week must be at least 1.", nameof(week));

        CurrentWeek = week;
    }

    /// <summary>
    /// Updates the notes.
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Adds a workout to the program.
    /// </summary>
    public AthleteWorkout AddWorkout(int weekNumber, int dayNumber, DateTime scheduledDate)
    {
        if (_workouts.Any(w => w.WeekNumber == weekNumber && w.DayNumber == dayNumber))
            throw new InvalidOperationException($"Workout for week {weekNumber} day {dayNumber} already exists.");

        var workout = AthleteWorkout.Create(Id, weekNumber, dayNumber, scheduledDate);
        _workouts.Add(workout);
        return workout;
    }

    /// <summary>
    /// Gets a specific workout.
    /// </summary>
    public AthleteWorkout? GetWorkout(int weekNumber, int dayNumber)
    {
        return _workouts.FirstOrDefault(w => w.WeekNumber == weekNumber && w.DayNumber == dayNumber);
    }
}
