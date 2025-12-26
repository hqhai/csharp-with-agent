using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Data_SurveyConfig_QuestBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProgressRequirement",
                table: "UserSurveyAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseType",
                table: "UserSurveyAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseLevel",
                table: "UserSurveyAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<bool>(
                name: "IsSurveyQuestBoard",
                table: "UserSurveyAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "SurveyConfigs",
                columns: new[] { "Id", "ApplicableProgramStr", "ApplicableSubjectsStr", "CompetitionEventIdsStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "EndDate", "IsDeleted", "Name", "ProgressRequirementsStr", "StartDate", "Status", "Title", "Tokens", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("327e645e-f09c-41ae-a8df-c797d52747b4"), "[\"QuestBoard\"]", null, null, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, new DateTime(2125, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Survey Questboard", null, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", null, 0, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_UserSurveyAssignments_SurveyConfigId",
                table: "UserSurveyAssignments",
                column: "SurveyConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSurveyAssignments_SurveyConfigs_SurveyConfigId",
                table: "UserSurveyAssignments",
                column: "SurveyConfigId",
                principalTable: "SurveyConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(
                "UPDATE SurveyQuestions SET SurveyConfigId = '327e645e-f09c-41ae-a8df-c797d52747b4' WHERE SurveyFormType = 'QuestBoard'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSurveyAssignments_SurveyConfigs_SurveyConfigId",
                table: "UserSurveyAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserSurveyAssignments_SurveyConfigId",
                table: "UserSurveyAssignments");

            migrationBuilder.Sql(
                "UPDATE SurveyQuestions SET SurveyConfigId = NULL WHERE SurveyFormType = 'QuestBoard'");

            migrationBuilder.DeleteData(
                table: "SurveyConfigs",
                keyColumn: "Id",
                keyValue: new Guid("327e645e-f09c-41ae-a8df-c797d52747b4"));

            migrationBuilder.DropColumn(
                name: "IsSurveyQuestBoard",
                table: "UserSurveyAssignments");

            migrationBuilder.AlterColumn<string>(
                name: "ProgressRequirement",
                table: "UserSurveyAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourseType",
                table: "UserSurveyAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourseLevel",
                table: "UserSurveyAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
