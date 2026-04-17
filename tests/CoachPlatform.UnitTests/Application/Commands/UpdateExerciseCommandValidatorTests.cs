using CoachPlatform.Application.Features.Exercises.Commands.UpdateExercise;
using CoachPlatform.Domain.Enums;
using FluentAssertions;

namespace CoachPlatform.UnitTests.Application.Commands;

public class UpdateExerciseCommandValidatorTests
{
    private readonly UpdateExerciseCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            Name = "Bench Press",
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest,
            IsCompound = true
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCoachId_ShouldBeInvalid()
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.Empty,
            ExerciseId = Guid.NewGuid(),
            Name = "Bench Press",
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CoachId");
    }

    [Fact]
    public void Validate_WithEmptyExerciseId_ShouldBeInvalid()
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.NewGuid(),
            ExerciseId = Guid.Empty,
            Name = "Bench Press",
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ExerciseId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullName_ShouldBeInvalid(string? name)
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            Name = name!,
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldBeInvalid()
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            Name = new string('a', 201),
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldBeInvalid()
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            Name = "Bench Press",
            Description = new string('a', 2001),
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WithVideoUrlExceedingMaxLength_ShouldBeInvalid()
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            Name = "Bench Press",
            VideoUrl = new string('h', 501),
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VideoUrl");
    }

    [Fact]
    public void Validate_WithEquipmentExceedingMaxLength_ShouldBeInvalid()
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            Name = "Bench Press",
            Equipment = new string('e', 201),
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Equipment");
    }

    [Fact]
    public void Validate_WithOptionalFieldsNull_ShouldBeValid()
    {
        var command = new UpdateExerciseCommand
        {
            CoachId = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            Name = "Bench Press",
            Description = null,
            VideoUrl = null,
            Equipment = null,
            Category = ExerciseCategory.Bench,
            PrimaryMuscleGroup = MuscleGroup.Chest
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
