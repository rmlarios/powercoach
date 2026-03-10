using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using ApplicationEntity = CoachPlatform.Domain.Entities.Application;

namespace CoachPlatform.UnitTests.Domain.Entities;

public class ApplicationTests
{
    private readonly Guid _validCoachId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateApplication()
    {
        // Act
        var application = ApplicationEntity.Create(
            coachId: _validCoachId,
            firstName: "John",
            lastName: "Doe",
            email: "john.doe@example.com",
            phone: "+1234567890",
            age: 25,
            gender: "Male",
            country: "USA",
            trainingExperience: "5 years of powerlifting",
            currentSquat: 180m,
            currentBench: 120m,
            currentDeadlift: 220m,
            motivation: "Want to compete professionally",
            goals: "Increase total by 50kg");

        // Assert
        application.Should().NotBeNull();
        application.CoachId.Should().Be(_validCoachId);
        application.ApplicantName.FirstName.Should().Be("John");
        application.ApplicantName.LastName.Should().Be("Doe");
        application.Email.Value.Should().Be("john.doe@example.com");
        application.Phone.Should().Be("+1234567890");
        application.Age.Should().Be(25);
        application.Gender.Should().Be("Male");
        application.Country.Should().Be("USA");
        application.TrainingExperience.Should().Be("5 years of powerlifting");
        application.CurrentSquat.Should().Be(180m);
        application.CurrentBench.Should().Be(120m);
        application.CurrentDeadlift.Should().Be(220m);
        application.Motivation.Should().Be("Want to compete professionally");
        application.Goals.Should().Be("Increase total by 50kg");
        application.Status.Should().Be(ApplicationStatus.Pending);
    }

    [Fact]
    public void Create_WithMinimalData_ShouldCreateApplication()
    {
        // Act
        var application = ApplicationEntity.Create(
            coachId: _validCoachId,
            firstName: "Jane",
            lastName: "Smith",
            email: "jane@example.com");

        // Assert
        application.Should().NotBeNull();
        application.ApplicantName.FirstName.Should().Be("Jane");
        application.ApplicantName.LastName.Should().Be("Smith");
        application.Email.Value.Should().Be("jane@example.com");
        application.Status.Should().Be(ApplicationStatus.Pending);
        application.Age.Should().BeNull();
        application.Country.Should().BeNull();
        application.CurrentSquat.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyCoachId_ShouldThrow()
    {
        // Act
        var act = () => ApplicationEntity.Create(
            coachId: Guid.Empty,
            firstName: "John",
            lastName: "Doe",
            email: "john@example.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Coach ID*");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrow()
    {
        // Act
        var act = () => ApplicationEntity.Create(
            coachId: _validCoachId,
            firstName: "John",
            lastName: "Doe",
            email: "invalid-email");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Accept_FromPending_ShouldChangeStatusToAccepted()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");

        // Act
        application.Accept("Great candidate!");

        // Assert
        application.Status.Should().Be(ApplicationStatus.Accepted);
        application.CoachNotes.Should().Be("Great candidate!");
        application.ReviewedAt.Should().NotBeNull();
        application.ReviewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Accept_FromUnderReview_ShouldChangeStatusToAccepted()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");
        application.StartReview();

        // Act
        application.Accept();

        // Assert
        application.Status.Should().Be(ApplicationStatus.Accepted);
    }

    [Fact]
    public void Accept_FromRejected_ShouldThrow()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");
        application.Reject("Not qualified");

        // Act
        var act = () => application.Accept();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_FromPending_ShouldChangeStatusToRejected()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");

        // Act
        application.Reject("Not enough experience", "Recommend reapplying in 6 months");

        // Assert
        application.Status.Should().Be(ApplicationStatus.Rejected);
        application.RejectionReason.Should().Be("Not enough experience");
        application.CoachNotes.Should().Be("Recommend reapplying in 6 months");
        application.ReviewedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_FromAccepted_ShouldThrow()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");
        application.Accept();

        // Act
        var act = () => application.Reject();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StartReview_FromPending_ShouldChangeStatusToUnderReview()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");

        // Act
        application.StartReview();

        // Assert
        application.Status.Should().Be(ApplicationStatus.UnderReview);
    }

    [Fact]
    public void StartReview_FromAccepted_ShouldThrow()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");
        application.Accept();

        // Act
        var act = () => application.StartReview();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Withdraw_FromPending_ShouldChangeStatusToWithdrawn()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");

        // Act
        application.Withdraw();

        // Assert
        application.Status.Should().Be(ApplicationStatus.Withdrawn);
    }

    [Fact]
    public void Withdraw_FromAccepted_ShouldThrow()
    {
        // Arrange
        var application = ApplicationEntity.Create(
            _validCoachId, "John", "Doe", "john@example.com");
        application.Accept();

        // Act
        var act = () => application.Withdraw();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
