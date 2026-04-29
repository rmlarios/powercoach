using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWorkoutWithSets;

/// <summary>
/// Handler for GetWorkoutWithSetsQuery.
/// Loads a specific workout with all exercises, sets, and previous performance data.
/// </summary>
public class GetWorkoutWithSetsQueryHandler : IRequestHandler<GetWorkoutWithSetsQuery, TodayWorkoutDto?>
{
    private readonly IApplicationDbContext _context;

    public GetWorkoutWithSetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TodayWorkoutDto?> Handle(GetWorkoutWithSetsQuery request, CancellationToken cancellationToken)
    {
        // Load workout with exercise logs
        var workout = await _context.AthleteWorkouts
            .AsNoTracking()
            .Include(w => w.ExerciseLogs)
                .ThenInclude(l => l.Exercise)
            .Include(w => w.AthleteProgram)
                .ThenInclude(ap => ap.ProgramTemplate)
            .Where(w => w.Id == request.WorkoutId
                        && w.AthleteProgram.AthleteId == request.AthleteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout is null)
            return null;

        // Get the day template for day name, focus, and prescribed exercises
        var dayTemplate = await _context.ProgramDayTemplates
            .AsNoTracking()
            .Include(d => d.WeekTemplate)
            .Include(d => d.Exercises)
                .ThenInclude(e => e.Exercise)
            .Where(d => d.WeekTemplate.ProgramTemplateId == workout.AthleteProgram.ProgramTemplateId
                        && d.WeekTemplate.WeekNumber == workout.WeekNumber
                        && d.DayNumber == workout.DayNumber)
            .FirstOrDefaultAsync(cancellationToken);

        // Build exercise groups
        var exerciseGroups = new List<WorkoutExerciseGroupDto>();

        if (dayTemplate?.Exercises != null)
        {
            foreach (var prescribed in dayTemplate.Exercises.OrderBy(e => e.Order))
            {
                var logs = workout.ExerciseLogs
                    .Where(l => l.ExerciseId == prescribed.ExerciseId)
                    .OrderBy(l => l.SetNumber)
                    .ToList();

                // Fetch previous performance FIRST — need estimated 1RM for suggested weights
                var previousPerformance = await GetPreviousPerformanceAsync(
                    request.AthleteId, prescribed.ExerciseId, workout.Id, cancellationToken);

                var perSetTargets = ParseCompoundNotation(prescribed.RawNotation, prescribed.Sets,
                    ParseTargetReps(prescribed.Reps), prescribed.Weight, prescribed.PercentageRM);

                var sets = new List<WorkoutSetDto>();
                for (int s = 1; s <= prescribed.Sets; s++)
                {
                    var log = logs.FirstOrDefault(l => l.SetNumber == s);
                    var target = perSetTargets.Count >= s ? perSetTargets[s - 1] : null;

                    var pctRM = target?.PercentageRM ?? prescribed.PercentageRM;
                    var suggestedWeight = CalculateSuggestedWeight(pctRM, previousPerformance?.EstimatedOneRM);

                    sets.Add(new WorkoutSetDto
                    {
                        Id = log?.Id ?? Guid.Empty,
                        SetNumber = s,
                        TargetReps = log?.TargetReps ?? target?.Reps ?? ParseTargetReps(prescribed.Reps),
                        TargetWeight = log?.TargetWeight ?? target?.Weight ?? prescribed.Weight,
                        TargetRpe = target?.Rpe ?? prescribed.TargetRpe,
                        TargetPercentageRM = pctRM,
                        SuggestedWeight = suggestedWeight,
                        SetLabel = target?.Label,
                        ActualReps = log?.Reps ?? 0,
                        ActualWeight = log?.Weight ?? 0,
                        ActualRpe = log?.Rpe,
                        IsCompleted = log?.IsCompleted ?? false,
                        SkippedReason = log?.SkippedReason,
                        Notes = log?.Notes
                    });
                }

                var emomConfig = ParseJson<EmomConfigDto>(prescribed.EmomConfigJson);
                var tempoConfig = ParseJson<TempoConfigDto>(prescribed.TempoConfigJson);
                var (supersetGroupId, supersetPosition) = ParseSupersetConfig(prescribed.SupersetConfigJson);

                exerciseGroups.Add(new WorkoutExerciseGroupDto
                {
                    ExerciseId = prescribed.ExerciseId,
                    ExerciseName = prescribed.Exercise?.Name ?? "Unknown",
                    Order = prescribed.Order,
                    PrescribedSets = prescribed.Sets,
                    PrescribedReps = prescribed.Reps,
                    PrescribedRpe = prescribed.TargetRpe,
                    RestSeconds = prescribed.RestSeconds,
                    ExerciseNotes = prescribed.Notes,
                    ExerciseType = prescribed.ExerciseType.ToString(),
                    PercentageRM = prescribed.PercentageRM,
                    RawNotation = prescribed.RawNotation,
                    PrescribedWeight = prescribed.Weight,
                    EmomConfig = emomConfig,
                    TempoConfig = tempoConfig,
                    SupersetGroupId = supersetGroupId,
                    SupersetPosition = supersetPosition,
                    Sets = sets,
                    PreviousPerformance = previousPerformance
                });
            }
        }

        return new TodayWorkoutDto
        {
            WorkoutId = workout.Id,
            AthleteProgramId = workout.AthleteProgramId,
            ProgramName = workout.AthleteProgram?.ProgramTemplate?.Name ?? "Program",
            WeekNumber = workout.WeekNumber,
            DayNumber = workout.DayNumber,
            DayName = dayTemplate?.Name,
            Focus = dayTemplate?.Focus.ToString(),
            ScheduledDate = workout.ScheduledDate,
            Status = workout.Status,
            StartedAt = workout.StartedAt,
            CompletedDate = workout.CompletedDate,
            DurationMinutes = workout.DurationMinutes,
            FatigueRating = workout.FatigueRating,
            Notes = workout.Notes,
            WeekNotes = dayTemplate?.WeekTemplate?.Notes,
            Exercises = exerciseGroups
        };
    }

    private static decimal? CalculateSuggestedWeight(decimal? percentageRM, decimal? estimated1RM)
    {
        if (percentageRM is null or <= 0 || estimated1RM is null or <= 0)
            return null;

        var raw = estimated1RM.Value * percentageRM.Value / 100m;
        return Math.Round(raw / 2.5m) * 2.5m;
    }

    private async Task<PreviousPerformanceDto?> GetPreviousPerformanceAsync(
        Guid athleteId, Guid exerciseId, Guid currentWorkoutId, CancellationToken cancellationToken)
    {
        var previousLogs = await _context.AthleteExerciseLogs
            .AsNoTracking()
            .Where(l => l.ExerciseId == exerciseId
                        && l.Workout.AthleteProgram.AthleteId == athleteId
                        && l.WorkoutId != currentWorkoutId
                        && l.IsCompleted)
            .OrderByDescending(l => l.Workout.ScheduledDate)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (previousLogs.Count == 0)
            return null;

        var lastWorkoutLogs = previousLogs
            .GroupBy(l => l.WorkoutId)
            .First()
            .ToList();

        var maxWeight = lastWorkoutLogs.Max(l => l.Weight);
        var maxReps = lastWorkoutLogs.Max(l => l.Reps);
        var bestRpe = lastWorkoutLogs.Where(l => l.Rpe.HasValue).MinBy(l => l.Rpe)?.Rpe;

        var bestSet = lastWorkoutLogs.OrderByDescending(l => l.Weight).First();
        decimal? estimated1RM = bestSet.Weight > 0 && bestSet.Reps > 0
            ? Math.Round(bestSet.Weight * (1 + (decimal)bestSet.Reps / 30), 1)
            : null;

        return new PreviousPerformanceDto
        {
            Date = previousLogs.First().CreatedAt,
            MaxWeight = maxWeight,
            MaxReps = maxReps,
            BestRpe = bestRpe,
            EstimatedOneRM = estimated1RM,
            TotalSets = lastWorkoutLogs.Count
        };
    }

    private static int? ParseTargetReps(string? reps)
    {
        if (string.IsNullOrWhiteSpace(reps))
            return null;

        if (reps.Contains('-'))
        {
            var parts = reps.Split('-');
            if (int.TryParse(parts[0].Trim(), out var lower))
                return lower;
        }

        if (int.TryParse(reps.Trim(), out var single))
            return single;

        return null;
    }

    private static T? ParseJson<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private static (string? GroupId, int? Position) ParseSupersetConfig(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return (null, null);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var groupId = root.TryGetProperty("groupId", out var gid) ? gid.GetString() : null;
            var position = root.TryGetProperty("position", out var pos) ? pos.GetInt32() : (int?)null;
            return (groupId, position);
        }
        catch
        {
            return (null, null);
        }
    }

    private record SetTarget(int Reps, decimal? Weight, decimal? Rpe, decimal? PercentageRM, string? Label);

    private static readonly Regex SxRPattern = new(
        @"(\d+)\s*x\s*(\d+)(?:-(\d+))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RpePattern = new(
        @"@\s*(?>(\d+(?:\.\d+)?))(?!\s*%)",
        RegexOptions.Compiled);

    private static readonly Regex PctPattern = new(
        @"(\d+(?:\.\d+)?)\s*%",
        RegexOptions.Compiled);

    private static List<SetTarget> ParseCompoundNotation(
        string? rawNotation, int totalSets, int? defaultReps, decimal? defaultWeight, decimal? defaultPercentageRM)
    {
        var results = new List<SetTarget>();

        if (string.IsNullOrWhiteSpace(rawNotation))
        {
            for (int i = 0; i < totalSets; i++)
                results.Add(new SetTarget(defaultReps ?? 0, defaultWeight, null, defaultPercentageRM, null));
            return results;
        }

        var cleaned = rawNotation.Trim().Replace("+", " ");
        var sxrMatches = SxRPattern.Matches(cleaned);

        if (sxrMatches.Count == 0)
        {
            for (int i = 0; i < totalSets; i++)
                results.Add(new SetTarget(defaultReps ?? 0, defaultWeight, null, defaultPercentageRM, null));
            return results;
        }

        var isCompound = sxrMatches.Count > 1;

        for (int idx = 0; idx < sxrMatches.Count; idx++)
        {
            var sxr = sxrMatches[idx];
            int sets = int.Parse(sxr.Groups[1].Value);
            int reps = int.Parse(sxr.Groups[2].Value);

            int trailStart = sxr.Index + sxr.Length;
            int trailEnd = idx + 1 < sxrMatches.Count ? sxrMatches[idx + 1].Index : cleaned.Length;
            var trailing = cleaned.Substring(trailStart, trailEnd - trailStart);

            decimal? rpe = null;
            var rpeMatch = RpePattern.Match(trailing);
            if (rpeMatch.Success)
                rpe = decimal.Parse(rpeMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

            decimal? pctRM = null;
            var pctMatch = PctPattern.Match(trailing);
            if (pctMatch.Success)
                pctRM = decimal.Parse(pctMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

            for (int s = 0; s < sets; s++)
            {
                string? label = null;
                if (isCompound)
                {
                    label = idx == 0 && sets == 1
                        ? "Top Set"
                        : idx == 0
                            ? $"Top Set {s + 1}"
                            : sets == 1
                                ? $"Backoff {idx}"
                                : $"Backoff {idx}.{s + 1}";
                }

                results.Add(new SetTarget(reps, defaultWeight, rpe, pctRM ?? defaultPercentageRM, label));
            }
        }

        while (results.Count < totalSets)
        {
            var last = results.LastOrDefault();
            results.Add(new SetTarget(last?.Reps ?? defaultReps ?? 0, defaultWeight, last?.Rpe, last?.PercentageRM ?? defaultPercentageRM, null));
        }

        return results;
    }
}
