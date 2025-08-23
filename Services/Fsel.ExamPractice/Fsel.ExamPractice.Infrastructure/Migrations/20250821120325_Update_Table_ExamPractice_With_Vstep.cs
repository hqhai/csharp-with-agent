using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.ExamPractice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_ExamPractice_With_Vstep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamPracticeAICriteriaSettings_ExamPracticeAISettings_ExamPracticeAISettingId",
                table: "ExamPracticeAICriteriaSettings");

            migrationBuilder.AddColumn<string>(
                name: "SubQuestionIndexsStr",
                table: "ExamPracticeSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "ExamPractices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ExamPractices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionStatus",
                table: "ExamPractices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPracticeAICriteriaSettings_ExamPracticeAISettings_ExamPracticeAISettingId",
                table: "ExamPracticeAICriteriaSettings",
                column: "ExamPracticeAISettingId",
                principalTable: "ExamPracticeAISettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamPracticeAICriteriaSettings_ExamPracticeAISettings_ExamPracticeAISettingId",
                table: "ExamPracticeAICriteriaSettings");

            migrationBuilder.DropColumn(
                name: "SubQuestionIndexsStr",
                table: "ExamPracticeSections");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "ExamPractices");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ExamPractices");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "ExamPractices");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPracticeAICriteriaSettings_ExamPracticeAISettings_ExamPracticeAISettingId",
                table: "ExamPracticeAICriteriaSettings",
                column: "ExamPracticeAISettingId",
                principalTable: "ExamPracticeAISettings",
                principalColumn: "Id");
        }
    }
}
