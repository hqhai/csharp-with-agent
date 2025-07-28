using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_BaseAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "VideoTimeCodeAnswers");

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
                name: "DeletedDate",
                table: "PlacementTestAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "PlacementTestAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "PlacementTestAnswers");

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
                name: "DeletedDate",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "MockTestAnswers");

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
                name: "DeletedDate",
                table: "HomeWorkAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "HomeWorkAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "HomeWorkAnswers");

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
                name: "DeletedDate",
                table: "FinalTestAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "FinalTestAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "FinalTestAnswers");

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
                name: "DeletedDate",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedFullName",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "ExtraPracticeAnswers");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "VideoTimeCodeAnswers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<short>(
                name: "CorrectCount",
                table: "VideoTimeCodeAnswers",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PlacementTestAnswers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<short>(
                name: "CorrectCount",
                table: "PlacementTestAnswers",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MockTestAnswers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<short>(
                name: "CorrectCount",
                table: "MockTestAnswers",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HomeWorkAnswers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<short>(
                name: "CorrectCount",
                table: "HomeWorkAnswers",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FinalTestAnswers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<short>(
                name: "CorrectCount",
                table: "FinalTestAnswers",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ExtraPracticeAnswers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "CorrectCount",
                table: "ExtraPracticeAnswers",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "VideoTimeCodeAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CorrectCount",
                table: "VideoTimeCodeAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "VideoTimeCodeAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "VideoTimeCodeAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "VideoTimeCodeAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PlacementTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CorrectCount",
                table: "PlacementTestAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "PlacementTestAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "PlacementTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "PlacementTestAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MockTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CorrectCount",
                table: "MockTestAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "MockTestAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "MockTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "MockTestAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HomeWorkAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CorrectCount",
                table: "HomeWorkAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "HomeWorkAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "HomeWorkAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "HomeWorkAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FinalTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CorrectCount",
                table: "FinalTestAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "FinalTestAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "FinalTestAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "FinalTestAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ExtraPracticeAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CorrectCount",
                table: "ExtraPracticeAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ExtraPracticeAnswers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "ExtraPracticeAnswers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "ExtraPracticeAnswers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

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
    }
}
