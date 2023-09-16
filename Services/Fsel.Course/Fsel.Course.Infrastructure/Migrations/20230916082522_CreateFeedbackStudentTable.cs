using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateFeedbackStudentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedBackNote",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "FeedBackStars",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "FeedBackNegativesStr",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "FeedBackNote",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "FeedBackPositivesStr",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "FeedBackStars",
                table: "ClassForumResults");

            migrationBuilder.CreateTable(
                name: "StudentFeedbacks",
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
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Feature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FeedBackStars = table.Column<int>(type: "int", nullable: true),
                    FeedBackNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FeedBackPositivesStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedBackNegativesStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFeedbacks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentFeedbacks");

            migrationBuilder.AddColumn<string>(
                name: "FeedBackNote",
                table: "MockTestResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeedBackStars",
                table: "MockTestResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedBackNegativesStr",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedBackNote",
                table: "ClassForumResults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedBackPositivesStr",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeedBackStars",
                table: "ClassForumResults",
                type: "int",
                nullable: true);
        }
    }
}
