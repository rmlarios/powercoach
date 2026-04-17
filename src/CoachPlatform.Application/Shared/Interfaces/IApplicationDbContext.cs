using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Shared.Interfaces;

/// <summary>
/// Interface for the application database context.
/// Abstracts Entity Framework Core from the Application layer.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Coach> Coaches { get; }
    DbSet<Athlete> Athletes { get; }
    DbSet<Domain.Entities.Application> Applications { get; }
    DbSet<Plan> Plans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Payment> Payments { get; }
    DbSet<CheckIn> CheckIns { get; }
    DbSet<TrainingCycle> TrainingCycles { get; }
    DbSet<WorkoutLog> WorkoutLogs { get; }
    DbSet<Exercise> Exercises { get; }
    
    // Training Programs module
    DbSet<ProgramTemplate> ProgramTemplates { get; }
    DbSet<ProgramWeekTemplate> ProgramWeekTemplates { get; }
    DbSet<ProgramDayTemplate> ProgramDayTemplates { get; }
    DbSet<ProgramExerciseTemplate> ProgramExerciseTemplates { get; }
    DbSet<AthleteProgram> AthletePrograms { get; }
    DbSet<AthleteWorkout> AthleteWorkouts { get; }
    DbSet<AthleteExerciseLog> AthleteExerciseLogs { get; }
    DbSet<AthleteMaxLift> AthleteMaxLifts { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
