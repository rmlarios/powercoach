using CoachPlatform.Application.Features.Applications.Commands.CreateApplication;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using ApplicationEntity = CoachPlatform.Domain.Entities.Application;

namespace CoachPlatform.UnitTests.Application.Commands;

public class CreateApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateApplicationCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();

    public CreateApplicationCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateApplicationCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateApplication()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity>());

        var command = new CreateApplicationCommand
        {
            CoachId = _validCoachId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Age = 25,
            Gender = "Male",
            Country = "USA",
            TrainingExperience = "5 years",
            CurrentSquat = 180m,
            CurrentBench = 120m,
            CurrentDeadlift = 220m,
            Motivation = "Want to compete",
            Goals = "Increase total"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.Applications.Add(It.IsAny<ApplicationEntity>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentCoach_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach>());

        var command = new CreateApplicationCommand
        {
            CoachId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldThrowConflictException()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        var existingApplication = ApplicationEntity.Create(
            _validCoachId, "Jane", "Smith", "john.doe@example.com");

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity> { existingApplication });

        var command = new CreateApplicationCommand
        {
            CoachId = _validCoachId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com" // Same email as existing application
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldCreateApplication()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity>());

        var command = new CreateApplicationCommand
        {
            CoachId = _validCoachId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
            // All other fields are optional
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
