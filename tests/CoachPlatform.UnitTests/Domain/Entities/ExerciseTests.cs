using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;

namespace CoachPlatform.UnitTests.Domain.Entities;

public class ExerciseTests
{
    private readonly Guid _validCoachId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateExercise()
    {
        // Act
        var exercise = Exercise.Create(
            _validCoachId,
            "Low Bar Squat",
            ExerciseCategory.Squat,
            MuscleGroup.Quadriceps,
            "A powerlifting squat variation",
            isCompound: true);

        // Assert
        exercise.Should().NotBeNull();
        exercise.CoachId.Should().Be(_validCoachId);
        exercise.Name.Should().Be("Low Bar Squat");
        exercise.Category.Should().Be(ExerciseCategory.Squat);
        exercise.PrimaryMuscleGroup.Should().Be(MuscleGroup.Quadriceps);
        exercise.Description.Should().Be("A powerlifting squat variation");
        exercise.IsCompound.Should().BeTrue();
        exercise.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyCoachId_ShouldThrow()
    {
        // Act
        var act = () => Exercise.Create(
            Guid.Empty,
            "Low Bar Squat",
            ExerciseCategory.Squat,
            MuscleGroup.Quadriceps);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Coach ID*");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        // Act
        var act = () => Exercise.Create(
            _validCoachId,
            "",
            ExerciseCategory.Squat,
            MuscleGroup.Quadriceps);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name is required*");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrow()
    {
        // Act
        var act = () => Exercise.Create(
            _validCoachId,
            "   ",
            ExerciseCategory.Squat,
            MuscleGroup.Quadriceps);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name is required*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var exercise = CreateValidExercise();

        // Act
        exercise.Update(
            "High Bar Squat",
            ExerciseCategory.Squat,
            MuscleGroup.Quadriceps,
            "Updated description",
            isCompound: true);

        // Assert
        exercise.Name.Should().Be("High Bar Squat");
        exercise.Description.Should().Be("Updated description");
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrow()
    {
        // Arrange
        var exercise = CreateValidExercise();

        // Act
        var act = () => exercise.Update(
            "",
            ExerciseCategory.Squat,
            MuscleGroup.Quadriceps,
            "Description",
            isCompound: true);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name is required*");
    }

    [Fact]
    public void SetSecondaryMuscleGroups_ShouldSetMuscleGroups()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var secondaryGroups = new[] { MuscleGroup.Glutes, MuscleGroup.Hamstrings };

        // Act
        exercise.SetSecondaryMuscleGroups(secondaryGroups);

        // Assert
        exercise.SecondaryMuscleGroups.Should().HaveCount(2);
        exercise.SecondaryMuscleGroups.Should().Contain(MuscleGroup.Glutes);
        exercise.SecondaryMuscleGroups.Should().Contain(MuscleGroup.Hamstrings);
    }

    [Fact]
    public void SetSecondaryMuscleGroups_WithNull_ShouldSetEmptyList()
    {
        // Arrange
        var exercise = CreateValidExercise();
        exercise.SetSecondaryMuscleGroups([MuscleGroup.Glutes]);

        // Act
        exercise.SetSecondaryMuscleGroups(null!);

        // Assert
        exercise.SecondaryMuscleGroups.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var exercise = CreateValidExercise();
        exercise.IsActive.Should().BeTrue();

        // Act
        exercise.Deactivate();

        // Assert
        exercise.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var exercise = CreateValidExercise();
        exercise.Deactivate();
        exercise.IsActive.Should().BeFalse();

        // Act
        exercise.Activate();

        // Assert
        exercise.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SetVideoUrl_ShouldUpdateVideoUrl()
    {
        // Arrange
        var exercise = CreateValidExercise();

        // Act
        exercise.SetVideoUrl("https://youtube.com/watch?v=12345");

        // Assert
        exercise.VideoUrl.Should().Be("https://youtube.com/watch?v=12345");
    }

    [Fact]
    public void SetInstructions_ShouldUpdateInstructions()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var instructions = new[] { "Step 1: Setup", "Step 2: Execute", "Step 3: Finish" };

        // Act
        exercise.SetInstructions(instructions);

        // Assert
        exercise.Instructions.Should().HaveCount(3);
        exercise.Instructions[0].Should().Be("Step 1: Setup");
    }

    [Fact]
    public void SetCoachingCues_ShouldUpdateCoachingCues()
    {
        // Arrange
        var exercise = CreateValidExercise();
        var cues = new[] { "Keep chest up", "Drive through heels" };

        // Act
        exercise.SetCoachingCues(cues);

        // Assert
        exercise.CoachingCues.Should().HaveCount(2);
        exercise.CoachingCues.Should().Contain("Keep chest up");
    }

    [Fact]
    public void SetDisplayOrder_ShouldUpdateDisplayOrder()
    {
        // Arrange
        var exercise = CreateValidExercise();
        exercise.DisplayOrder.Should().Be(0);

        // Act
        exercise.SetDisplayOrder(5);

        // Assert
        exercise.DisplayOrder.Should().Be(5);
    }

    [Theory]
    [InlineData(ExerciseCategory.Squat)]
    [InlineData(ExerciseCategory.Bench)]
    [InlineData(ExerciseCategory.Deadlift)]
    [InlineData(ExerciseCategory.Accessory)]
    public void Create_WithDifferentCategories_ShouldSetCorrectCategory(ExerciseCategory category)
    {
        // Act
        var exercise = Exercise.Create(
            _validCoachId,
            "Test Exercise",
            category,
            MuscleGroup.Quadriceps);

        // Assert
        exercise.Category.Should().Be(category);
    }

    [Theory]
    [InlineData(MuscleGroup.Quadriceps)]
    [InlineData(MuscleGroup.Chest)]
    [InlineData(MuscleGroup.Back)]
    [InlineData(MuscleGroup.Shoulders)]
    public void Create_WithDifferentMuscleGroups_ShouldSetCorrectMuscleGroup(MuscleGroup muscleGroup)
    {
        // Act
        var exercise = Exercise.Create(
            _validCoachId,
            "Test Exercise",
            ExerciseCategory.Accessory,
            muscleGroup);

        // Assert
        exercise.PrimaryMuscleGroup.Should().Be(muscleGroup);
    }

    private Exercise CreateValidExercise()
    {
        return Exercise.Create(
            _validCoachId,
            "Low Bar Squat",
            ExerciseCategory.Squat,
            MuscleGroup.Quadriceps,
            "A powerlifting squat variation",
            isCompound: true);
    }
}
