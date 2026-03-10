using CoachPlatform.Application.Shared.Interfaces;

namespace CoachPlatform.Infrastructure.Services;

/// <summary>
/// Implementation of ITenantService for multi-tenancy support.
/// Uses AsyncLocal to maintain tenant context across async operations.
/// </summary>
public class TenantService : ITenantService
{
    private static readonly AsyncLocal<Guid?> _currentTenant = new();

    public Guid? CoachId => _currentTenant.Value;

    public bool HasTenant => _currentTenant.Value.HasValue;

    public void SetTenant(Guid coachId)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("Coach ID cannot be empty.", nameof(coachId));

        _currentTenant.Value = coachId;
    }

    public void ClearTenant()
    {
        _currentTenant.Value = null;
    }
}
