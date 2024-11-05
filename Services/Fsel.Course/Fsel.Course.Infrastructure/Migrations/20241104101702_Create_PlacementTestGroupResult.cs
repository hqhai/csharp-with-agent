using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_PlacementTestGroupResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlacementTestGroupResultId",
                table: "PlacementTestResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlacementTestGroupResults",
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
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    ProcessLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompletionLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SuggetLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChooseLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementTestGroupResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestGroupResultId",
                table: "PlacementTestResults",
                column: "PlacementTestGroupResultId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestGroupResults_StudentId",
                table: "PlacementTestGroupResults",
                column: "StudentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestResults_PlacementTestGroupResults_PlacementTestGroupResultId",
                table: "PlacementTestResults",
                column: "PlacementTestGroupResultId",
                principalTable: "PlacementTestGroupResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTestResults_PlacementTestGroupResults_PlacementTestGroupResultId",
                table: "PlacementTestResults");

            migrationBuilder.DropTable(
                name: "PlacementTestGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_PlacementTestGroupResultId",
                table: "PlacementTestResults");

            migrationBuilder.DropColumn(
                name: "PlacementTestGroupResultId",
                table: "PlacementTestResults");
        }
    }
}
