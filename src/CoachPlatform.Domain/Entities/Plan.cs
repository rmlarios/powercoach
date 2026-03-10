using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.ValueObjects;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// Plan entity - represents a membership plan offered by the coach.
/// </summary>
public class Plan : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Reference to the coach who owns this plan.
    /// Required for multi-tenant support.
    /// </summary>
    public Guid CoachId { get; private set; }

    /// <summary>
    /// Name of the plan.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Description of what's included in the plan.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Price of the plan.
    /// </summary>
    public Money Price { get; private set; } = null!;

    /// <summary>
    /// Duration of the plan in days.
    /// </summary>
    public int DurationDays { get; private set; }

    /// <summary>
    /// Type of plan based on duration.
    /// </summary>
    public PlanType PlanType { get; private set; }

    /// <summary>
    /// Features included in the plan.
    /// </summary>
    public List<string> Features { get; private set; } = [];

    /// <summary>
    /// Maximum number of athletes allowed on this plan (null = unlimited).
    /// </summary>
    public int? MaxAthletes { get; private set; }

    /// <summary>
    /// Whether the plan is currently available for new subscriptions.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Display order for the plan in listings.
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public Coach Coach { get; private set; } = null!;

    private readonly List<Subscription> _subscriptions = [];
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    // EF Core constructor
    private Plan() { }

    private Plan(
        Guid coachId,
        string name,
        string? description,
        Money price,
        int durationDays,
        PlanType planType)
    {
        CoachId = coachId;
        Name = name;
        Description = description;
        Price = price;
        DurationDays = durationDays;
        PlanType = planType;
        IsActive = true;
        DisplayOrder = 0;
    }

    /// <summary>
    /// Creates a new Plan.
    /// </summary>
    public static Plan Create(
        Guid coachId,
        string name,
        decimal price,
        string currency,
        int durationDays,
        PlanType planType,
        string? description = null)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("Coach ID is required.", nameof(coachId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required.", nameof(name));

        if (durationDays <= 0)
            throw new ArgumentException("Duration must be greater than 0.", nameof(durationDays));

        var priceVo = Money.Create(price, currency);

        return new Plan(coachId, name.Trim(), description?.Trim(), priceVo, durationDays, planType);
    }

    /// <summary>
    /// Creates a monthly plan.
    /// </summary>
    public static Plan CreateMonthly(Guid coachId, string name, decimal price, string currency, string? description = null)
        => Create(coachId, name, price, currency, 30, PlanType.Monthly, description);

    /// <summary>
    /// Creates a quarterly plan.
    /// </summary>
    public static Plan CreateQuarterly(Guid coachId, string name, decimal price, string currency, string? description = null)
        => Create(coachId, name, price, currency, 90, PlanType.Quarterly, description);

    /// <summary>
    /// Creates an annual plan.
    /// </summary>
    public static Plan CreateAnnual(Guid coachId, string name, decimal price, string currency, string? description = null)
        => Create(coachId, name, price, currency, 365, PlanType.Annual, description);

    /// <summary>
    /// Updates the plan details.
    /// </summary>
    public void Update(string name, string? description, decimal price, string currency, int durationDays)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required.", nameof(name));

        if (durationDays <= 0)
            throw new ArgumentException("Duration must be greater than 0.", nameof(durationDays));

        Name = name.Trim();
        Description = description?.Trim();
        Price = Money.Create(price, currency);
        DurationDays = durationDays;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the features included in the plan.
    /// </summary>
    public void SetFeatures(IEnumerable<string> features)
    {
        Features = features.Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a feature to the plan.
    /// </summary>
    public void AddFeature(string feature)
    {
        if (!string.IsNullOrWhiteSpace(feature) && !Features.Contains(feature))
        {
            Features.Add(feature);
            SetUpdatedAt();
        }
    }

    /// <summary>
    /// Sets the maximum number of athletes.
    /// </summary>
    public void SetMaxAthletes(int? maxAthletes)
    {
        if (maxAthletes.HasValue && maxAthletes.Value <= 0)
            throw new ArgumentException("Max athletes must be greater than 0.", nameof(maxAthletes));

        MaxAthletes = maxAthletes;
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
    /// Activates the plan.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Deactivates the plan (no new subscriptions allowed).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
