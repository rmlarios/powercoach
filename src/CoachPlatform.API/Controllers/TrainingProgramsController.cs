using CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramDay;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramExercise;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramWeek;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.AssignProgramToAthlete;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.CreateProgramTemplate;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramDay;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramExercise;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramWeek;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.LogWorkout;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.SaveProgramTemplate;
using CoachPlatform.Application.Features.TrainingPrograms.Commands.UpdateProgramTemplate;
using CoachPlatform.Application.Features.TrainingPrograms.Queries.GetAthleteProgram;
using CoachPlatform.Application.Features.TrainingPrograms.Queries.GetAthleteWorkout;
using CoachPlatform.Application.Features.TrainingPrograms.Queries.GetProgramTemplateById;
using CoachPlatform.Application.Features.TrainingPrograms.Queries.GetProgramTemplates;
using CoachPlatform.Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing training programs.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class TrainingProgramsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TrainingProgramsController> _logger;

    public TrainingProgramsController(IMediator mediator, ILogger<TrainingProgramsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Program Templates

    /// <summary>
    /// Get program templates for a coach.
    /// </summary>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <param name="isActive">Optional filter by active status.</param>
    /// <param name="searchTerm">Optional search term.</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100).</param>
    /// <returns>Paginated list of program templates.</returns>
    [HttpGet("programs")]
    [ProducesResponseType(typeof(PagedResult<ProgramTemplateListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProgramTemplateListItemDto>>> GetPrograms(
        [FromQuery] Guid coachId,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetProgramTemplatesQuery
        {
            CoachId = coachId,
            IsActive = isActive,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = Math.Min(pageSize, 100)
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a program template by ID.
    /// </summary>
    /// <param name="id">The program template's unique identifier.</param>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <returns>The program template details.</returns>
    [HttpGet("programs/{id:guid}")]
    [ProducesResponseType(typeof(ProgramTemplateDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramTemplateDetailDto>> GetProgramById(
        Guid id,
        [FromQuery] Guid coachId)
    {
        var query = new GetProgramTemplateByIdQuery { Id = id, CoachId = coachId };
        var result = await _mediator.Send(query);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new program template.
    /// </summary>
    /// <param name="dto">The program template data.</param>
    /// <returns>The created program template's ID.</returns>
    [HttpPost("programs")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> CreateProgram([FromBody] CreateProgramTemplateDto dto)
    {
        var command = new CreateProgramTemplateCommand
        {
            CoachId = dto.CoachId,
            Name = dto.Name,
            Description = dto.Description,
            DurationWeeks = dto.DurationWeeks
        };

        var programId = await _mediator.Send(command);

        _logger.LogInformation("Created program template {ProgramId} for coach {CoachId}", programId, dto.CoachId);

        return CreatedAtAction(
            nameof(GetProgramById),
            new { id = programId, coachId = dto.CoachId },
            programId);
    }

    /// <summary>
    /// Update a program template's metadata.
    /// </summary>
    /// <param name="id">The program template's unique identifier.</param>
    /// <param name="dto">The updated program data.</param>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <returns>No content.</returns>
    [HttpPut("programs/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProgram(
        Guid id,
        [FromBody] UpdateProgramTemplateDto dto,
        [FromQuery] Guid coachId)
    {
        var command = new UpdateProgramTemplateCommand
        {
            CoachId = coachId,
            ProgramTemplateId = id,
            Name = dto.Name,
            Description = dto.Description,
            DurationWeeks = dto.DurationWeeks
        };

        await _mediator.Send(command);

        _logger.LogInformation("Updated program template {ProgramId}", id);

        return NoContent();
    }

    /// <summary>
    /// Bulk save (replace) the entire program structure: weeks, days, and exercises.
    /// </summary>
    /// <param name="id">The program template's unique identifier.</param>
    /// <param name="dto">The full program data.</param>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <returns>The program template's ID.</returns>
    [HttpPut("programs/{id:guid}/full")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> SaveProgram(
        Guid id,
        [FromBody] SaveProgramTemplateDto dto,
        [FromQuery] Guid coachId)
    {
        var command = new SaveProgramTemplateCommand
        {
            CoachId = coachId,
            ProgramTemplateId = id,
            Data = dto
        };

        var programId = await _mediator.Send(command);

        _logger.LogInformation("Bulk saved program template {ProgramId}", programId);

        return Ok(programId);
    }

    #endregion

    #region Program Weeks

    /// <summary>
    /// Add a week to a program template.
    /// </summary>
    /// <param name="programId">The program template's unique identifier.</param>
    /// <param name="dto">The week data.</param>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <returns>The created week's ID.</returns>
    [HttpPost("programs/{programId:guid}/weeks")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> AddProgramWeek(
        Guid programId,
        [FromBody] AddProgramWeekDto dto,
        [FromQuery] Guid coachId)
    {
        var command = new AddProgramWeekCommand
        {
            CoachId = coachId,
            ProgramTemplateId = programId,
            WeekNumber = dto.WeekNumber,
            Name = dto.Name,
            Notes = dto.Notes
        };

        var weekId = await _mediator.Send(command);

        _logger.LogInformation("Added week {WeekNumber} to program {ProgramId}", dto.WeekNumber, programId);

        return CreatedAtAction(
            nameof(GetProgramById),
            new { id = programId, coachId },
            weekId);
    }

    /// <summary>
    /// Delete a week from a program template.
    /// </summary>
    [HttpDelete("programs/{programId:guid}/weeks/{weekId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProgramWeek(
        Guid programId,
        Guid weekId,
        [FromQuery] Guid coachId)
    {
        var command = new DeleteProgramWeekCommand
        {
            CoachId = coachId,
            ProgramTemplateId = programId,
            WeekId = weekId
        };

        await _mediator.Send(command);

        _logger.LogInformation("Deleted week {WeekId} from program {ProgramId}", weekId, programId);

        return NoContent();
    }

    #endregion

    #region Program Days

    /// <summary>
    /// Add a day to a program week.
    /// </summary>
    /// <param name="programId">The program template's unique identifier.</param>
    /// <param name="weekId">The week template's unique identifier.</param>
    /// <param name="dto">The day data.</param>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <returns>The created day's ID.</returns>
    [HttpPost("programs/{programId:guid}/weeks/{weekId:guid}/days")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> AddProgramDay(
        Guid programId,
        Guid weekId,
        [FromBody] AddProgramDayDto dto,
        [FromQuery] Guid coachId)
    {
        var command = new AddProgramDayCommand
        {
            CoachId = coachId,
            WeekTemplateId = weekId,
            DayNumber = dto.DayNumber,
            Name = dto.Name,
            Focus = dto.Focus,
            Notes = dto.Notes
        };

        var dayId = await _mediator.Send(command);

        _logger.LogInformation("Added day {DayNumber} to week {WeekId}", dto.DayNumber, weekId);

        return Created($"/api/programs/{programId}/days/{dayId}", dayId);
    }

    /// <summary>
    /// Delete a day from a program week.
    /// </summary>
    [HttpDelete("programs/{programId:guid}/days/{dayId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProgramDay(
        Guid programId,
        Guid dayId,
        [FromQuery] Guid coachId)
    {
        var command = new DeleteProgramDayCommand
        {
            CoachId = coachId,
            DayId = dayId
        };

        await _mediator.Send(command);

        _logger.LogInformation("Deleted day {DayId} from program {ProgramId}", dayId, programId);

        return NoContent();
    }

    #endregion

    #region Program Exercises

    /// <summary>
    /// Add an exercise to a program day.
    /// </summary>
    /// <param name="programId">The program template's unique identifier.</param>
    /// <param name="weekId">The week template's unique identifier.</param>
    /// <param name="dayId">The day template's unique identifier.</param>
    /// <param name="dto">The exercise data.</param>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <returns>The created exercise template's ID.</returns>
    [HttpPost("programs/{programId:guid}/weeks/{weekId:guid}/days/{dayId:guid}/exercises")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> AddProgramExercise(
        Guid programId,
        Guid weekId,
        Guid dayId,
        [FromBody] AddProgramExerciseDto dto,
        [FromQuery] Guid coachId)
    {
        var command = new AddProgramExerciseCommand
        {
            CoachId = coachId,
            DayTemplateId = dayId,
            ExerciseId = dto.ExerciseId,
            Sets = dto.Sets,
            Reps = dto.Reps,
            TargetRpe = dto.TargetRpe,
            RestSeconds = dto.RestSeconds,
            Notes = dto.Notes,
            ExerciseType = dto.ExerciseType,
            PercentageRM = dto.PercentageRM,
            RawNotation = dto.RawNotation,
            Weight = dto.Weight,
            EmomConfigJson = dto.EmomConfigJson,
            TempoConfigJson = dto.TempoConfigJson,
            SupersetConfigJson = dto.SupersetConfigJson
        };

        var exerciseId = await _mediator.Send(command);

        _logger.LogInformation("Added exercise {ExerciseId} to day {DayId}", dto.ExerciseId, dayId);

        return Created($"/api/programs/{programId}/exercises/{exerciseId}", exerciseId);
    }

    /// <summary>
    /// Delete an exercise from a program day.
    /// </summary>
    [HttpDelete("programs/{programId:guid}/exercises/{exerciseTemplateId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProgramExercise(
        Guid programId,
        Guid exerciseTemplateId,
        [FromQuery] Guid coachId)
    {
        var command = new DeleteProgramExerciseCommand
        {
            CoachId = coachId,
            ExerciseTemplateId = exerciseTemplateId
        };

        await _mediator.Send(command);

        _logger.LogInformation("Deleted exercise {ExerciseTemplateId} from program {ProgramId}", exerciseTemplateId, programId);

        return NoContent();
    }

    #endregion

    #region Athlete Programs

    /// <summary>
    /// Assign a program to an athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="dto">The assignment data.</param>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <returns>The created athlete program's ID.</returns>
    [HttpPost("athletes/{athleteId:guid}/assign-program")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> AssignProgramToAthlete(
        Guid athleteId,
        [FromBody] AssignProgramToAthleteDto dto,
        [FromQuery] Guid coachId)
    {
        var command = new AssignProgramToAthleteCommand
        {
            CoachId = coachId,
            AthleteId = athleteId,
            ProgramTemplateId = dto.ProgramTemplateId,
            StartDate = dto.StartDate,
            Notes = dto.Notes
        };

        var athleteProgramId = await _mediator.Send(command);

        _logger.LogInformation(
            "Assigned program {ProgramId} to athlete {AthleteId}",
            dto.ProgramTemplateId,
            athleteId);

        return CreatedAtAction(
            nameof(GetAthleteProgram),
            new { athleteId },
            athleteProgramId);
    }

    /// <summary>
    /// Get an athlete's current program.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <returns>The athlete's current program details.</returns>
    [HttpGet("athletes/{athleteId:guid}/program")]
    [ProducesResponseType(typeof(AthleteCurrentProgramDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteCurrentProgramDto>> GetAthleteProgram(Guid athleteId)
    {
        var query = new GetAthleteProgramQuery { AthleteId = athleteId };
        var result = await _mediator.Send(query);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    #endregion

    #region Workouts

    /// <summary>
    /// Log a completed workout.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="dto">The workout log data.</param>
    /// <returns>The workout ID.</returns>
    [HttpPost("athletes/{athleteId:guid}/workouts/{workoutId:guid}/log")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> LogWorkout(
        Guid athleteId,
        Guid workoutId,
        [FromBody] LogWorkoutDto dto)
    {
        var command = new LogWorkoutCommand
        {
            WorkoutId = workoutId,
            DurationMinutes = dto.DurationMinutes,
            FatigueRating = dto.FatigueRating,
            Notes = dto.Notes,
            ExerciseSets = dto.ExerciseSets
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("Logged workout {WorkoutId}", workoutId);

        return Ok(result);
    }

    #endregion
}
