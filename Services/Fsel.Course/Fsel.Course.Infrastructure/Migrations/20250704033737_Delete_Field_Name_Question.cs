using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Delete_Field_Name_Question : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Skills_SkillId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_SkillId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "QuestionName",
                table: "Questions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionName",
                table: "Questions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_SkillId",
                table: "Sections",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Skills_SkillId",
                table: "Sections",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");
        }
    }
}
