using CoachPlatform.Application.Features.CheckIns.Commands.CreateCheckIn;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class CreateCheckInCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateCheckInCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validAthleteId = Guid.NewGuid();

    public CreateCheckInCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateCheckInCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateCheckIn()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.CheckIns)
            .ReturnsDbSet(new List<CheckIn>());

        var command = new CreateCheckInCommand
        {
            AthleteId = _validAthleteId,
            Weight = 75.5m,
            FatigueLevel = 6,
            SleepQuality = 8,
            MotivationLevel = 7,
            Notes = "Feeling good today"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.CheckIns.Add(It.IsAny<CheckIn>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentAthlete_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var command = new CreateCheckInCommand
        {
            AthleteId = Guid.NewGuid(),
            Weight = 75.5m
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithOnlyWeight_ShouldCreateCheckIn()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.CheckIns)
            .ReturnsDbSet(new List<CheckIn>());

        var command = new CreateCheckInCommand
        {
            AthleteId = _validAthleteId,
            Weight = 80.0m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.CheckIns.Add(It.IsAny<CheckIn>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldCreateCheckIn()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.CheckIns)
            .ReturnsDbSet(new List<CheckIn>());

        var command = new CreateCheckInCommand
        {
            AthleteId = _validAthleteId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.CheckIns.Add(It.IsAny<CheckIn>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNotes_ShouldSetNotesCorrectly()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        CheckIn? addedCheckIn = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.CheckIns)
            .ReturnsDbSet(new List<CheckIn>());

        _contextMock.Setup(x => x.CheckIns.Add(It.IsAny<CheckIn>()))
            .Callback<CheckIn>(c => addedCheckIn = c);

        var command = new CreateCheckInCommand
        {
            AthleteId = _validAthleteId,
            Notes = "Had a great workout today!"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedCheckIn.Should().NotBeNull();
        addedCheckIn!.Notes.Should().Be("Had a great workout today!");
    }

    [Fact]
    public async Task Handle_WithAllMetrics_ShouldSetAllValuesCorrectly()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        SetEntityId(athlete, _validAthleteId);

        CheckIn? addedCheckIn = null;

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { athlete });

        _contextMock.Setup(x => x.CheckIns)
            .ReturnsDbSet(new List<CheckIn>());

        _contextMock.Setup(x => x.CheckIns.Add(It.IsAny<CheckIn>()))
            .Callback<CheckIn>(c => addedCheckIn = c);

        var command = new CreateCheckInCommand
        {
            AthleteId = _validAthleteId,
            Weight = 72.5m,
            FatigueLevel = 5,
            SleepQuality = 7,
            MotivationLevel = 9,
            Notes = "Test notes"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedCheckIn.Should().NotBeNull();
        addedCheckIn!.Weight.Should().Be(72.5m);
        addedCheckIn.StressLevel.Should().Be(5); // FatigueLevel maps to StressLevel
        addedCheckIn.SleepQuality.Should().Be(7);
        addedCheckIn.EnergyLevel.Should().Be(9); // MotivationLevel maps to EnergyLevel
        addedCheckIn.Notes.Should().Be("Test notes");
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
