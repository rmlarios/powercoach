using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeExercisesGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Coaches_CoachId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CoachId_Category",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CoachId_IsActive",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CoachId_Name",
                table: "Exercises");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoachId",
                table: "Exercises",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Category",
                table: "Exercises",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_IsActive",
                table: "Exercises",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Coaches_CoachId",
                table: "Exercises",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Coaches_CoachId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Category",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_IsActive",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Name",
                table: "Exercises");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoachId",
                table: "Exercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Coaches_CoachId",
                table: "Exercises",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
