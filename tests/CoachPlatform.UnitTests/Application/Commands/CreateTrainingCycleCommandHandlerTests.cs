using CoachPlatform.Application.Features.TrainingCycles.Commands.CreateTrainingCycle;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class CreateTrainingCycleCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateTrainingCycleCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();

    public CreateTrainingCycleCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateTrainingCycleCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateTrainingCycle()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.TrainingCycles)
            .ReturnsDbSet(new List<TrainingCycle>());

        var command = new CreateTrainingCycleCommand
        {
            AthleteId = _validAthleteId,
            Name = "8-Week Strength Program",
            DurationWeeks = 8,
            StartDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.TrainingCycles.Add(It.IsAny<TrainingCycle>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentAthlete_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var command = new CreateTrainingCycleCommand
        {
            AthleteId = Guid.NewGuid(),
            Name = "8-Week Program",
            DurationWeeks = 8,
            StartDate = DateTime.UtcNow.Date
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCalculateEndDateCorrectly()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        TrainingCycle? addedCycle = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.TrainingCycles)
            .ReturnsDbSet(new List<TrainingCycle>());

        _contextMock.Setup(x => x.TrainingCycles.Add(It.IsAny<TrainingCycle>()))
            .Callback<TrainingCycle>(c => addedCycle = c);

        var startDate = new DateTime(2026, 3, 9);
        var command = new CreateTrainingCycleCommand
        {
            AthleteId = _validAthleteId,
            Name = "8-Week Program",
            DurationWeeks = 8,
            StartDate = startDate
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedCycle.Should().NotBeNull();
        addedCycle!.EndDate.Should().Be(startDate.AddDays(8 * 7)); // 56 days later
    }

    [Fact]
    public async Task Handle_WithDifferentDurations_ShouldCreateCorrectly()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        TrainingCycle? addedCycle = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.TrainingCycles)
            .ReturnsDbSet(new List<TrainingCycle>());

        _contextMock.Setup(x => x.TrainingCycles.Add(It.IsAny<TrainingCycle>()))
            .Callback<TrainingCycle>(c => addedCycle = c);

        var startDate = new DateTime(2026, 1, 1);
        var command = new CreateTrainingCycleCommand
        {
            AthleteId = _validAthleteId,
            Name = "12-Week Hypertrophy",
            DurationWeeks = 12,
            StartDate = startDate
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedCycle.Should().NotBeNull();
        addedCycle!.Name.Should().Be("12-Week Hypertrophy");
        addedCycle.DurationWeeks.Should().Be(12);
        addedCycle.EndDate.Should().Be(startDate.AddDays(12 * 7)); // 84 days later
    }

    [Fact]
    public async Task Handle_ShouldSetAthleteIdCorrectly()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        TrainingCycle? addedCycle = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.TrainingCycles)
            .ReturnsDbSet(new List<TrainingCycle>());

        _contextMock.Setup(x => x.TrainingCycles.Add(It.IsAny<TrainingCycle>()))
            .Callback<TrainingCycle>(c => addedCycle = c);

        var command = new CreateTrainingCycleCommand
        {
            AthleteId = _validAthleteId,
            Name = "Test Program",
            DurationWeeks = 4,
            StartDate = DateTime.UtcNow.Date
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedCycle.Should().NotBeNull();
        addedCycle!.AthleteId.Should().Be(_validAthleteId);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeDateToStartOfDay()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        TrainingCycle? addedCycle = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.TrainingCycles)
            .ReturnsDbSet(new List<TrainingCycle>());

        _contextMock.Setup(x => x.TrainingCycles.Add(It.IsAny<TrainingCycle>()))
            .Callback<TrainingCycle>(c => addedCycle = c);

        var startDateWithTime = new DateTime(2026, 3, 9, 14, 30, 45);
        var command = new CreateTrainingCycleCommand
        {
            AthleteId = _validAthleteId,
            Name = "Test Program",
            DurationWeeks = 6,
            StartDate = startDateWithTime
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedCycle.Should().NotBeNull();
        addedCycle!.StartDate.Should().Be(startDateWithTime.Date);
        addedCycle.StartDate.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
