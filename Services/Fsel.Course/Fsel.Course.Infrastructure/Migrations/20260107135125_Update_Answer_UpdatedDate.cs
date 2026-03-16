using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Answer_UpdatedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "VideoTimeCodeAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedFullName",
                table: "VideoTimeCodeAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "VideoTimeCodeAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "TestAnswer",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedFullName",
                table: "TestAnswer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "TestAnswer",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "PlacementTestAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedFullName",
                table: "PlacementTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "PlacementTestAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "MockTestAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedFullName",
                table: "MockTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "MockTestAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "HomeWorkExtraPracticeAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedFullName",
                table: "HomeWorkExtraPracticeAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "HomeWorkExtraPracticeAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "HomeWorkAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedFullName",
                table: "HomeWorkAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "HomeWorkAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "FinalTestAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedFullName",
                table: "FinalTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "FinalTestAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ExtraPracticeAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedFullName",
                table: "ExtraPracticeAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "ExtraPracticeAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "TestAnswer");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "TestAnswer");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "TestAnswer");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "PlacementTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "PlacementTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "HomeWorkExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "HomeWorkExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "HomeWorkExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "HomeWorkAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "HomeWorkAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "HomeWorkAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "FinalTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "FinalTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "FinalTestAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "ExtraPracticeAnswers");
        }
    }
}
