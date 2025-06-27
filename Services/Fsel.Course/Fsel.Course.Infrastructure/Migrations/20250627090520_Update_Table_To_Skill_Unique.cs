using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_To_Skill_Unique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SectionGroups_SkillId",
                table: "SectionGroups");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorks_SkillId",
                table: "HomeWorks");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_SkillId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_SkillId",
                table: "ClassForums");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroups_SkillId",
                table: "SectionGroups",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorks_SkillId",
                table: "HomeWorks",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_SkillId",
                table: "Exercises",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_SkillId",
                table: "ClassForums",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SectionGroups_SkillId",
                table: "SectionGroups");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorks_SkillId",
                table: "HomeWorks");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_SkillId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_SkillId",
                table: "ClassForums");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroups_SkillId",
                table: "SectionGroups",
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
        }
    }
}
