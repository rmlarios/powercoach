using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// Exercise entity - represents an exercise in the coach's exercise library.
/// Aggregate Root - each coach maintains their own exercise catalog.
/// </summary>
public class Exercise : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Reference to the coach who owns this exercise.
    /// Required for multi-tenant support.
    /// </summary>
    public Guid CoachId { get; private set; }

    /// <summary>
    /// Name of the exercise.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Description or instructions for performing the exercise.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Category of the exercise (Squat, Bench, Deadlift, etc.)
    /// </summary>
    public ExerciseCategory Category { get; private set; }

    /// <summary>
    /// Primary muscle group targeted.
    /// </summary>
    public MuscleGroup PrimaryMuscleGroup { get; private set; }

    /// <summary>
    /// Secondary muscle groups targeted.
    /// </summary>
    public List<MuscleGroup> SecondaryMuscleGroups { get; private set; } = [];

    /// <summary>
    /// URL to a video demonstration.
    /// </summary>
    public string? VideoUrl { get; private set; }

    /// <summary>
    /// URL to an image demonstration.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Equipment required for the exercise.
    /// </summary>
    public string? Equipment { get; private set; }

    /// <summary>
    /// Detailed instructions broken into steps.
    /// </summary>
    public List<string> Instructions { get; private set; } = [];

    /// <summary>
    /// Common coaching cues for the exercise.
    /// </summary>
    public List<string> CoachingCues { get; private set; } = [];

    /// <summary>
    /// Whether this is a compound or isolation exercise.
    /// </summary>
    public bool IsCompound { get; private set; }

    /// <summary>
    /// Whether this exercise is currently active in the library.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Display order for sorting exercises.
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation property
    public Coach Coach { get; private set; } = null!;

    // EF Core constructor
    private Exercise() { }

    private Exercise(
        Guid coachId,
        string name,
        ExerciseCategory category,
        MuscleGroup primaryMuscleGroup,
        string? description,
        bool isCompound)
    {
        CoachId = coachId;
        Name = name;
        Category = category;
        PrimaryMuscleGroup = primaryMuscleGroup;
        Description = description;
        IsCompound = isCompound;
        IsActive = true;
        DisplayOrder = 0;
    }

    /// <summary>
    /// Creates a new Exercise.
    /// </summary>
    public static Exercise Create(
        Guid coachId,
        string name,
        ExerciseCategory category,
        MuscleGroup primaryMuscleGroup,
        string? description = null,
        bool isCompound = true)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("Coach ID is required.", nameof(coachId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exercise name is required.", nameof(name));

        return new Exercise(coachId, name.Trim(), category, primaryMuscleGroup, description?.Trim(), isCompound);
    }

    /// <summary>
    /// Updates basic exercise information.
    /// </summary>
    public void Update(
        string name,
        ExerciseCategory category,
        MuscleGroup primaryMuscleGroup,
        string? description,
        bool isCompound)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exercise name is required.", nameof(name));

        Name = name.Trim();
        Category = category;
        PrimaryMuscleGroup = primaryMuscleGroup;
        Description = description?.Trim();
        IsCompound = isCompound;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the secondary muscle groups.
    /// </summary>
    public void SetSecondaryMuscleGroups(IEnumerable<MuscleGroup> muscleGroups)
    {
        SecondaryMuscleGroups = muscleGroups?.ToList() ?? [];
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the video demonstration URL.
    /// </summary>
    public void SetVideoUrl(string? url)
    {
        VideoUrl = url;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the image URL.
    /// </summary>
    public void SetImageUrl(string? url)
    {
        ImageUrl = url;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the equipment required.
    /// </summary>
    public void SetEquipment(string? equipment)
    {
        Equipment = equipment?.Trim();
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the exercise instructions.
    /// </summary>
    public void SetInstructions(IEnumerable<string> instructions)
    {
        Instructions = instructions?.Select(i => i.Trim()).ToList() ?? [];
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the coaching cues.
    /// </summary>
    public void SetCoachingCues(IEnumerable<string> cues)
    {
        CoachingCues = cues?.Select(c => c.Trim()).ToList() ?? [];
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        SetUpdatedAt();
    }

    /// <summary>
    /// Activates the exercise.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Deactivates the exercise.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
