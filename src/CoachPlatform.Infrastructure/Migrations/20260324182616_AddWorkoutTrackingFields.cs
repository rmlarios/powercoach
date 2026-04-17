using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SkippedReason",
                table: "AthleteWorkouts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "AthleteWorkouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AthleteWorkouts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "NotStarted");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "AthleteExerciseLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SkippedReason",
                table: "AthleteExerciseLogs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetReps",
                table: "AthleteExerciseLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetWeight",
                table: "AthleteExerciseLogs",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkippedReason",
                table: "AthleteWorkouts");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "AthleteWorkouts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AthleteWorkouts");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "AthleteExerciseLogs");

            migrationBuilder.DropColumn(
                name: "SkippedReason",
                table: "AthleteExerciseLogs");

            migrationBuilder.DropColumn(
                name: "TargetReps",
                table: "AthleteExerciseLogs");

            migrationBuilder.DropColumn(
                name: "TargetWeight",
                table: "AthleteExerciseLogs");
        }
    }
}
