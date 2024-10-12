using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditTable_EventRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCompetitionEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolId",
                table: "EventRegistrations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolGrade",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolClass",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Province",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EventRegistrations",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "District",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSchoolarshipAdvising",
                table: "EventRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ParentEmail",
                table: "EventRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentPhoneNumber",
                table: "EventRegistrations",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherPhoneNumber",
                table: "EventRegistrations",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "CompetitionEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentEventId",
                table: "CompetitionEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionEvents_ParentEventId",
                table: "CompetitionEvents",
                column: "ParentEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionEvents_CompetitionEvents_ParentEventId",
                table: "CompetitionEvents",
                column: "ParentEventId",
                principalTable: "CompetitionEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCompetitionEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents",
                column: "CompetitionEventId",
                principalTable: "CompetitionEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionEvents_CompetitionEvents_ParentEventId",
                table: "CompetitionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCompetitionEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionEvents_ParentEventId",
                table: "CompetitionEvents");

            migrationBuilder.DropColumn(
                name: "IsSchoolarshipAdvising",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "ParentEmail",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "ParentPhoneNumber",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "TeacherPhoneNumber",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "CompetitionEvents");

            migrationBuilder.DropColumn(
                name: "ParentEventId",
                table: "CompetitionEvents");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolId",
                table: "EventRegistrations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolGrade",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolClass",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Province",
                table: "EventRegistrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254);

            migrationBuilder.AlterColumn<string>(
                name: "District",
                table: "EventRegistrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCompetitionEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents",
                column: "CompetitionEventId",
                principalTable: "CompetitionEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
