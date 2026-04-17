using CoachPlatform.Application.Features.TrainingPrograms.Commands.CreateProgramTemplate;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Commands;

public class CreateProgramTemplateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateProgramTemplateCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();

    public CreateProgramTemplateCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateProgramTemplateCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateProgramTemplate()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.ProgramTemplates)
            .ReturnsDbSet(new List<ProgramTemplate>());

        var command = new CreateProgramTemplateCommand
        {
            CoachId = _validCoachId,
            Name = "12 Week Strength Program",
            Description = "A comprehensive program for building strength",
            DurationWeeks = 12
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.ProgramTemplates.Add(It.IsAny<ProgramTemplate>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentCoach_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach>());

        var command = new CreateProgramTemplateCommand
        {
            CoachId = Guid.NewGuid(),
            Name = "Test Program",
            Description = "Test",
            DurationWeeks = 8
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        var existingProgram = ProgramTemplate.Create(
            _validCoachId, "Existing Program", "Description", 8);

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.ProgramTemplates)
            .ReturnsDbSet(new List<ProgramTemplate> { existingProgram });

        var command = new CreateProgramTemplateCommand
        {
            CoachId = _validCoachId,
            Name = "Existing Program",
            Description = "Another description",
            DurationWeeks = 10
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldCreateProgramTemplate()
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.ProgramTemplates)
            .ReturnsDbSet(new List<ProgramTemplate>());

        var command = new CreateProgramTemplateCommand
        {
            CoachId = _validCoachId,
            Name = "Simple Program",
            DurationWeeks = 4
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(52)]
    public async Task Handle_WithValidDurationWeeks_ShouldCreateProgramTemplate(int durationWeeks)
    {
        // Arrange
        var coach = Coach.Create("Test", "Coach", "coach@example.com");
        SetEntityId(coach, _validCoachId);

        _contextMock.Setup(x => x.Coaches)
            .ReturnsDbSet(new List<Coach> { coach });

        _contextMock.Setup(x => x.ProgramTemplates)
            .ReturnsDbSet(new List<ProgramTemplate>());

        var command = new CreateProgramTemplateCommand
        {
            CoachId = _validCoachId,
            Name = $"Program with {durationWeeks} weeks",
            DurationWeeks = durationWeeks
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}
