using CoachPlatform.Application.Features.TrainingPrograms.Commands.AssignProgramToAthlete;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class AssignProgramToAthleteCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly AssignProgramToAthleteCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();
    private readonly Guid _validProgramId = Guid.NewGuid();

    public AssignProgramToAthleteCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new AssignProgramToAthleteCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldAssignProgramToAthlete()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var programTemplate = ProgramTemplate.Create(_validCoachId, "Test Program", "Description", 4);
        SetEntityId(programTemplate, _validProgramId);

        // Add a week and day to the program template
        var week = programTemplate.AddWeek(1);
        week.AddDay(1, DayFocus.Squat);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.ProgramTemplates)
            .ReturnsDbSet(new List<ProgramTemplate> { programTemplate });

        _contextMock.Setup(x => x.AthletePrograms)
            .ReturnsDbSet(new List<AthleteProgram>());

        var command = new AssignProgramToAthleteCommand
        {
            CoachId = _validCoachId,
            AthleteId = _validAthleteId,
            ProgramTemplateId = _validProgramId,
            StartDate = DateTime.UtcNow.AddDays(1),
            Notes = "Starting this athlete on a strength program"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.AthletePrograms.Add(It.IsAny<AthleteProgram>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentAthlete_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var command = new AssignProgramToAthleteCommand
        {
            CoachId = _validCoachId,
            AthleteId = Guid.NewGuid(),
            ProgramTemplateId = _validProgramId,
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Athlete*");
    }

    [Fact]
    public async Task Handle_WithNonexistentProgram_ShouldThrowNotFoundException()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.ProgramTemplates)
            .ReturnsDbSet(new List<ProgramTemplate>());

        var command = new AssignProgramToAthleteCommand
        {
            CoachId = _validCoachId,
            AthleteId = _validAthleteId,
            ProgramTemplateId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*ProgramTemplate*");
    }

    [Fact]
    public async Task Handle_WithAthleteHavingActiveProgram_ShouldThrowConflictException()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var programTemplate = ProgramTemplate.Create(_validCoachId, "Test Program", "Description", 4);
        SetEntityId(programTemplate, _validProgramId);

        var existingProgram = AthleteProgram.Create(_validAthleteId, _validProgramId, DateTime.UtcNow);
        existingProgram.Start(); // Make it active

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.ProgramTemplates)
            .ReturnsDbSet(new List<ProgramTemplate> { programTemplate });

        _contextMock.Setup(x => x.AthletePrograms)
            .ReturnsDbSet(new List<AthleteProgram> { existingProgram });

        var command = new AssignProgramToAthleteCommand
        {
            CoachId = _validCoachId,
            AthleteId = _validAthleteId,
            ProgramTemplateId = _validProgramId,
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithInactiveProgram_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        var programTemplate = ProgramTemplate.Create(_validCoachId, "Test Program", "Description", 4);
        SetEntityId(programTemplate, _validProgramId);
        programTemplate.Deactivate(); // Make it inactive

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.ProgramTemplates)
            .ReturnsDbSet(new List<ProgramTemplate> { programTemplate });

        _contextMock.Setup(x => x.AthletePrograms)
            .ReturnsDbSet(new List<AthleteProgram>());

        var command = new AssignProgramToAthleteCommand
        {
            CoachId = _validCoachId,
            AthleteId = _validAthleteId,
            ProgramTemplateId = _validProgramId,
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public async Task Handle_WithAthleteFromDifferentCoach_ShouldThrowNotFoundException()
    {
        // Arrange
        var differentCoachId = Guid.NewGuid();
        var athlete = Athlete.Create(differentCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        var command = new AssignProgramToAthleteCommand
        {
            CoachId = _validCoachId,
            AthleteId = _validAthleteId,
            ProgramTemplateId = _validProgramId,
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
