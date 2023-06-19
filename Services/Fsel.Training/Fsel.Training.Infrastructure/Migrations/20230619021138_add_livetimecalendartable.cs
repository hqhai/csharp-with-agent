using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Training.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_livetimecalendartable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CsoId",
                table: "Classes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LiveDays",
                table: "Classes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "LiveTimeFrameId",
                table: "Classes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "Classes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LiveTimeCalendar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LiveTimeFrameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveTimeCalendar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveTimeCalendar_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveTimeCalendar_ClassId",
                table: "LiveTimeCalendar",
                column: "ClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveTimeCalendar");

            migrationBuilder.DropColumn(
                name: "CsoId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "LiveDays",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "LiveTimeFrameId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Classes");
        }
    }
}
