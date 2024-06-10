using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Course_And_CourseResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "UnitResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "UnitResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentCourseId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "CourseResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "CourseResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingStatus",
                table: "CourseResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "ParentCourseId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "CourseResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "CourseResults");

            migrationBuilder.DropColumn(
                name: "WorkingStatus",
                table: "CourseResults");
        }
    }
}
