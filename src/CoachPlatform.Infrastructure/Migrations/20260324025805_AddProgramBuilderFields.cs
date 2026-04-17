using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramBuilderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApplicantLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrainingExperience = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CurrentSquat = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CurrentBench = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CurrentDeadlift = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Motivation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Goals = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferralSource = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CoachNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryMuscleGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryMuscleGroups = table.Column<string>(type: "text", nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Equipment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Instructions = table.Column<string>(type: "text", nullable: false),
                    CoachingCues = table.Column<string>(type: "text", nullable: false),
                    IsCompound = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PriceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    PlanType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Features = table.Column<string>(type: "text", nullable: false),
                    MaxAthletes = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plans_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DurationWeeks = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramTemplates_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Athletes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Goals = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Height = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ExperienceLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    ProfilePictureUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Athletes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Athletes_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Athletes_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramWeekTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramWeekTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramWeekTemplates_ProgramTemplates_ProgramTemplateId",
                        column: x => x.ProgramTemplateId,
                        principalTable: "ProgramTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AthleteMaxLifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsTested = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstimationDetails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteMaxLifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AthleteMaxLifts_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AthleteMaxLifts_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AthletePrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentWeek = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthletePrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AthletePrograms_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AthletePrograms_ProgramTemplates_ProgramTemplateId",
                        column: x => x.ProgramTemplateId,
                        principalTable: "ProgramTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EnergyLevel = table.Column<int>(type: "integer", nullable: true),
                    SleepQuality = table.Column<int>(type: "integer", nullable: true),
                    SleepHours = table.Column<decimal>(type: "numeric(4,1)", nullable: true),
                    StressLevel = table.Column<int>(type: "integer", nullable: true),
                    NutritionAdherence = table.Column<int>(type: "integer", nullable: true),
                    TrainingAdherence = table.Column<int>(type: "integer", nullable: true),
                    PhotoUrls = table.Column<string>(type: "text", nullable: false),
                    CoachFeedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FeedbackDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckIns_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    PriceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DurationWeeks = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingCycles_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    RPE = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkoutDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutLogs_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutLogs_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramDayTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Focus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramDayTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramDayTemplates_ProgramWeekTemplates_WeekTemplateId",
                        column: x => x.WeekTemplateId,
                        principalTable: "ProgramWeekTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AthleteWorkouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    FatigueRating = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteWorkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AthleteWorkouts_AthletePrograms_AthleteProgramId",
                        column: x => x.AthleteProgramId,
                        principalTable: "AthletePrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ExternalReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundReason = table.Column<string>(type: "text", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProgramExerciseTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetRpe = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    RestSeconds = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ExerciseType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Standard"),
                    PercentageRM = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    RawNotation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    EmomConfigJson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TempoConfigJson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SupersetConfigJson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramExerciseTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramExerciseTemplates_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramExerciseTemplates_ProgramDayTemplates_DayTemplateId",
                        column: x => x.DayTemplateId,
                        principalTable: "ProgramDayTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AthleteExerciseLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Rpe = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteExerciseLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AthleteExerciseLogs_AthleteWorkouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "AthleteWorkouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AthleteExerciseLogs_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CoachId",
                table: "Applications",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CoachId_Status",
                table: "Applications",
                columns: new[] { "CoachId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CreatedAt",
                table: "Applications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Status",
                table: "Applications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteExerciseLogs_ExerciseId",
                table: "AthleteExerciseLogs",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteExerciseLogs_WorkoutId",
                table: "AthleteExerciseLogs",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteExerciseLogs_WorkoutId_ExerciseId_SetNumber",
                table: "AthleteExerciseLogs",
                columns: new[] { "WorkoutId", "ExerciseId", "SetNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AthleteMaxLifts_Athlete_Exercise_Date",
                table: "AthleteMaxLifts",
                columns: new[] { "AthleteId", "ExerciseId", "RecordedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AthleteMaxLifts_AthleteId",
                table: "AthleteMaxLifts",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteMaxLifts_ExerciseId",
                table: "AthleteMaxLifts",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_AthletePrograms_AthleteId",
                table: "AthletePrograms",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_AthletePrograms_AthleteId_Status",
                table: "AthletePrograms",
                columns: new[] { "AthleteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AthletePrograms_ProgramTemplateId",
                table: "AthletePrograms",
                column: "ProgramTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AthletePrograms_StartDate",
                table: "AthletePrograms",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Athletes_ApplicationId",
                table: "Athletes",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Athletes_CoachId",
                table: "Athletes",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Athletes_CoachId_Status",
                table: "Athletes",
                columns: new[] { "CoachId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Athletes_Email",
                table: "Athletes",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Athletes_Status",
                table: "Athletes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteWorkouts_AthleteProgramId",
                table: "AthleteWorkouts",
                column: "AthleteProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteWorkouts_AthleteProgramId_IsCompleted",
                table: "AthleteWorkouts",
                columns: new[] { "AthleteProgramId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_AthleteWorkouts_AthleteProgramId_Week_Day",
                table: "AthleteWorkouts",
                columns: new[] { "AthleteProgramId", "WeekNumber", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteWorkouts_ScheduledDate",
                table: "AthleteWorkouts",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_AthleteId",
                table: "CheckIns",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_AthleteId_CheckInDate",
                table: "CheckIns",
                columns: new[] { "AthleteId", "CheckInDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_CheckInDate",
                table: "CheckIns",
                column: "CheckInDate");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_Email",
                table: "Coaches",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CoachId",
                table: "Exercises",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CoachId_Category",
                table: "Exercises",
                columns: new[] { "CoachId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CoachId_IsActive",
                table: "Exercises",
                columns: new[] { "CoachId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CoachId_Name",
                table: "Exercises",
                columns: new[] { "CoachId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_CoachId",
                table: "Plans",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_IsActive",
                table: "Plans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramDayTemplates_WeekTemplateId",
                table: "ProgramDayTemplates",
                column: "WeekTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramDayTemplates_WeekTemplateId_DayNumber",
                table: "ProgramDayTemplates",
                columns: new[] { "WeekTemplateId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramExerciseTemplates_DayTemplateId",
                table: "ProgramExerciseTemplates",
                column: "DayTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramExerciseTemplates_DayTemplateId_Order",
                table: "ProgramExerciseTemplates",
                columns: new[] { "DayTemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramExerciseTemplates_ExerciseId",
                table: "ProgramExerciseTemplates",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTemplates_CoachId",
                table: "ProgramTemplates",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTemplates_CoachId_IsActive",
                table: "ProgramTemplates",
                columns: new[] { "CoachId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTemplates_CoachId_Name",
                table: "ProgramTemplates",
                columns: new[] { "CoachId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramWeekTemplates_ProgramTemplateId",
                table: "ProgramWeekTemplates",
                column: "ProgramTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramWeekTemplates_ProgramTemplateId_WeekNumber",
                table: "ProgramWeekTemplates",
                columns: new[] { "ProgramTemplateId", "WeekNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AthleteId",
                table: "Subscriptions",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AthleteId_Status",
                table: "Subscriptions",
                columns: new[] { "AthleteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_EndDate",
                table: "Subscriptions",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status",
                table: "Subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCycles_AthleteId",
                table: "TrainingCycles",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCycles_AthleteId_StartDate",
                table: "TrainingCycles",
                columns: new[] { "AthleteId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_AthleteId",
                table: "WorkoutLogs",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_AthleteId_ExerciseId_WorkoutDate",
                table: "WorkoutLogs",
                columns: new[] { "AthleteId", "ExerciseId", "WorkoutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_AthleteId_WorkoutDate",
                table: "WorkoutLogs",
                columns: new[] { "AthleteId", "WorkoutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_ExerciseId",
                table: "WorkoutLogs",
                column: "ExerciseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AthleteExerciseLogs");

            migrationBuilder.DropTable(
                name: "AthleteMaxLifts");

            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ProgramExerciseTemplates");

            migrationBuilder.DropTable(
                name: "TrainingCycles");

            migrationBuilder.DropTable(
                name: "WorkoutLogs");

            migrationBuilder.DropTable(
                name: "AthleteWorkouts");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "ProgramDayTemplates");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "AthletePrograms");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "ProgramWeekTemplates");

            migrationBuilder.DropTable(
                name: "Athletes");

            migrationBuilder.DropTable(
                name: "ProgramTemplates");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Coaches");
        }
    }
}
