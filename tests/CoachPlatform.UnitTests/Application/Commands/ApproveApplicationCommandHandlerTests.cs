using CoachPlatform.Application.Features.Applications.Commands.ApproveApplication;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using ApplicationEntity = CoachPlatform.Domain.Entities.Application;

namespace CoachPlatform.UnitTests.Application.Commands;

public class ApproveApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly ApproveApplicationCommandHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();
    private readonly Guid _validApplicationId = Guid.NewGuid();

    public ApproveApplicationCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new ApproveApplicationCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPendingApplication_ShouldApproveAndCreateAthlete()
    {
        // Arrange
        var application = CreateTestApplication();

        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity> { application });

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var command = new ApproveApplicationCommand
        {
            ApplicationId = _validApplicationId,
            Notes = "Great candidate!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ApplicationId.Should().Be(_validApplicationId);
        result.AthleteId.Should().NotBe(Guid.Empty);
        application.Status.Should().Be(ApplicationStatus.Accepted);
        application.CoachNotes.Should().Be("Great candidate!");
        
        _contextMock.Verify(x => x.Athletes.Add(It.IsAny<Athlete>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentApplication_ShouldThrowNotFoundException()
    {
        // Arrange
        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity>());

        var command = new ApproveApplicationCommand
        {
            ApplicationId = Guid.NewGuid()
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyAcceptedApplication_ShouldThrowConflictException()
    {
        // Arrange
        var application = CreateTestApplication();
        application.Accept(); // Already accepted

        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity> { application });

        var command = new ApproveApplicationCommand
        {
            ApplicationId = _validApplicationId
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithRejectedApplication_ShouldThrowConflictException()
    {
        // Arrange
        var application = CreateTestApplication();
        application.Reject("Not qualified");

        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity> { application });

        var command = new ApproveApplicationCommand
        {
            ApplicationId = _validApplicationId
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithExistingAthleteEmail_ShouldThrowConflictException()
    {
        // Arrange
        var application = CreateTestApplication();
        
        // Create an existing athlete with the same email
        var existingAthlete = Athlete.Create(
            _validCoachId,
            "Existing",
            "Athlete",
            "john.doe@example.com"); // Same email as application

        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity> { application });

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { existingAthlete });

        var command = new ApproveApplicationCommand
        {
            ApplicationId = _validApplicationId
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithUnderReviewApplication_ShouldApprove()
    {
        // Arrange
        var application = CreateTestApplication();
        application.StartReview(); // Set to UnderReview status

        _contextMock.Setup(x => x.Applications)
            .ReturnsDbSet(new List<ApplicationEntity> { application });

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var command = new ApproveApplicationCommand
        {
            ApplicationId = _validApplicationId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        application.Status.Should().Be(ApplicationStatus.Accepted);
    }

    private ApplicationEntity CreateTestApplication()
    {
        var application = ApplicationEntity.Create(
            coachId: _validCoachId,
            firstName: "John",
            lastName: "Doe",
            email: "john.doe@example.com",
            phone: "+1234567890",
            age: 25,
            goals: "Increase total");

        // Set the Id using reflection
        var idProperty = typeof(ApplicationEntity).BaseType?.BaseType?.GetProperty("Id");
        idProperty?.SetValue(application, _validApplicationId);

        return application;
    }
}
