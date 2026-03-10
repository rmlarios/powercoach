using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;

namespace CoachPlatform.UnitTests.Domain.Entities;

public class AthleteTests
{
    private readonly Guid _validCoachId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateAthlete()
    {
        // Act
        var athlete = Athlete.Create(
            _validCoachId,
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "Improve strength");

        // Assert
        athlete.Should().NotBeNull();
        athlete.CoachId.Should().Be(_validCoachId);
        athlete.Name.FirstName.Should().Be("John");
        athlete.Name.LastName.Should().Be("Doe");
        athlete.Email.Value.Should().Be("john.doe@example.com");
        athlete.Phone.Should().Be("+1234567890");
        athlete.Goals.Should().Be("Improve strength");
        athlete.Status.Should().Be(AthleteStatus.Active);
        athlete.StartDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        athlete.EndDate.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyCoachId_ShouldThrow()
    {
        // Act
        var act = () => Athlete.Create(
            Guid.Empty,
            "John",
            "Doe",
            "john@example.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Coach ID*");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrow()
    {
        // Act
        var act = () => Athlete.Create(
            _validCoachId,
            "John",
            "Doe",
            "invalid-email");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateAllFields()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");

        // Act
        athlete.UpdateProfile("Jane", "Smith", "+0987654321", "New goals", "Some notes");

        // Assert
        athlete.Name.FirstName.Should().Be("Jane");
        athlete.Name.LastName.Should().Be("Smith");
        athlete.Phone.Should().Be("+0987654321");
        athlete.Goals.Should().Be("New goals");
        athlete.Notes.Should().Be("Some notes");
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmail()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");

        // Act
        athlete.UpdateEmail("newemail@example.com");

        // Assert
        athlete.Email.Value.Should().Be("newemail@example.com");
    }

    [Fact]
    public void PutOnHold_ShouldChangeStatusToOnHold()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");

        // Act
        athlete.PutOnHold();

        // Assert
        athlete.Status.Should().Be(AthleteStatus.OnHold);
    }

    [Fact]
    public void Activate_ShouldChangeStatusToActive()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        athlete.PutOnHold();

        // Act
        athlete.Activate();

        // Assert
        athlete.Status.Should().Be(AthleteStatus.Active);
    }

    [Fact]
    public void Deactivate_ShouldChangeStatusAndSetEndDate()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");

        // Act
        athlete.Deactivate();

        // Assert
        athlete.Status.Should().Be(AthleteStatus.Inactive);
        athlete.EndDate.Should().NotBeNull();
        athlete.EndDate!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Graduate_ShouldChangeStatusAndSetEndDate()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");

        // Act
        athlete.Graduate();

        // Assert
        athlete.Status.Should().Be(AthleteStatus.Graduated);
        athlete.EndDate.Should().NotBeNull();
    }

    [Fact]
    public void SetProfilePicture_ShouldUpdateUrl()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");
        var pictureUrl = "https://storage.example.com/images/profile.jpg";

        // Act
        athlete.SetProfilePicture(pictureUrl);

        // Assert
        athlete.ProfilePictureUrl.Should().Be(pictureUrl);
    }

    [Fact]
    public void Create_WithAllFields_ShouldCreateAthleteWithPhysicalData()
    {
        // Act
        var athlete = Athlete.Create(
            _validCoachId,
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "Improve strength",
            country: "USA",
            gender: "Male",
            dateOfBirth: new DateTime(1990, 5, 15),
            height: 180m,
            weight: 85m,
            experienceLevel: "Intermediate");

        // Assert
        athlete.Country.Should().Be("USA");
        athlete.Gender.Should().Be("Male");
        athlete.DateOfBirth.Should().Be(new DateTime(1990, 5, 15));
        athlete.Height.Should().Be(180m);
        athlete.Weight.Should().Be(85m);
        athlete.ExperienceLevel.Should().Be("Intermediate");
    }

    [Fact]
    public void UpdatePhysicalData_ShouldUpdateAllPhysicalFields()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");

        // Act
        athlete.UpdatePhysicalData(
            country: "Canada",
            height: 175m,
            weight: 80m,
            experienceLevel: "Advanced");

        // Assert
        athlete.Country.Should().Be("Canada");
        athlete.Height.Should().Be(175m);
        athlete.Weight.Should().Be(80m);
        athlete.ExperienceLevel.Should().Be("Advanced");
    }

    [Fact]
    public void Suspend_ShouldChangeStatusToSuspended()
    {
        // Arrange
        var athlete = Athlete.Create(_validCoachId, "John", "Doe", "john@example.com");

        // Act
        athlete.Suspend();

        // Assert
        athlete.Status.Should().Be(AthleteStatus.Suspended);
    }
}
