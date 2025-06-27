using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_To_Skill_Unique1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonInstructions_SkillId",
                table: "LessonInstructions");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPracticeExerciseResults_SkillId",
                table: "ExtraPracticeExerciseResults");

            migrationBuilder.CreateIndex(
                name: "IX_LessonInstructions_SkillId",
                table: "LessonInstructions",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeExerciseResults_SkillId",
                table: "ExtraPracticeExerciseResults",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonInstructions_SkillId",
                table: "LessonInstructions");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPracticeExerciseResults_SkillId",
                table: "ExtraPracticeExerciseResults");

            migrationBuilder.CreateIndex(
                name: "IX_LessonInstructions_SkillId",
                table: "LessonInstructions",
                column: "SkillId",
                unique: true,
                filter: "[SkillId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeExerciseResults_SkillId",
                table: "ExtraPracticeExerciseResults",
                column: "SkillId",
                unique: true,
                filter: "[SkillId] IS NOT NULL");
        }
    }
}
