using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Result_Date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "VideoResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "VideoResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "VideoResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "TestResult",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "TestResult",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "TestResult",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                table: "StudentGoalAggregates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "DocumentResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "DocumentResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "DocumentResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "ClassForumResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "ClassForumResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "ClassForumResults",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "StudentGoalAggregates");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "DocumentResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "DocumentResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "DocumentResults");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "ClassForumResults");
        }
    }
}
