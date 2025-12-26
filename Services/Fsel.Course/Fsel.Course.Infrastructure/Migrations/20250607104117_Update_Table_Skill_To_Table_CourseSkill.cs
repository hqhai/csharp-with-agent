using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_Skill_To_Table_CourseSkill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Skills",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "SectionGroups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "LessonInstructions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "HomeWorks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "ExtraPracticeExerciseResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "ClassForums",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroups_SkillId",
                table: "SectionGroups",
                column: "SkillId",
                unique: true,
                filter: "[SkillId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LessonInstructions_SkillId",
                table: "LessonInstructions",
                column: "SkillId",
                unique: true,
                filter: "[SkillId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorks_SkillId",
                table: "HomeWorks",
                column: "SkillId",
                unique: true,
                filter: "[SkillId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeExerciseResults_SkillId",
                table: "ExtraPracticeExerciseResults",
                column: "SkillId",
                unique: true,
                filter: "[SkillId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_SkillId",
                table: "Exercises",
                column: "SkillId",
                unique: true,
                filter: "[SkillId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_SkillId",
                table: "ClassForums",
                column: "SkillId",
                unique: true,
                filter: "[SkillId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForums_Skills_SkillId",
                table: "ClassForums",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Skills_SkillId",
                table: "Exercises",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraPracticeExerciseResults_Skills_SkillId",
                table: "ExtraPracticeExerciseResults",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeWorks_Skills_SkillId",
                table: "HomeWorks",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonInstructions_Skills_SkillId",
                table: "LessonInstructions",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SectionGroups_Skills_SkillId",
                table: "SectionGroups",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForums_Skills_SkillId",
                table: "ClassForums");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Skills_SkillId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_ExtraPracticeExerciseResults_Skills_SkillId",
                table: "ExtraPracticeExerciseResults");

            migrationBuilder.DropForeignKey(
                name: "FK_HomeWorks_Skills_SkillId",
                table: "HomeWorks");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonInstructions_Skills_SkillId",
                table: "LessonInstructions");

            migrationBuilder.DropForeignKey(
                name: "FK_SectionGroups_Skills_SkillId",
                table: "SectionGroups");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroups_SkillId",
                table: "SectionGroups");

            migrationBuilder.DropIndex(
                name: "IX_LessonInstructions_SkillId",
                table: "LessonInstructions");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorks_SkillId",
                table: "HomeWorks");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPracticeExerciseResults_SkillId",
                table: "ExtraPracticeExerciseResults");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_SkillId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_SkillId",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "SectionGroups");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "LessonInstructions");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "HomeWorks");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "ExtraPracticeExerciseResults");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "ClassForums");
        }
    }
}
