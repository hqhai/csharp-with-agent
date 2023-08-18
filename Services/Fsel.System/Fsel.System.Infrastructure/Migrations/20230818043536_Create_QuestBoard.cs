using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_QuestBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestBoardConfigs",
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
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestBoardConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestBoards",
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
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumberOfStars = table.Column<int>(type: "int", nullable: false),
                    IsLifeTime = table.Column<bool>(type: "bit", nullable: false),
                    RepeatType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PackageIdsStr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DependentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestBoards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestBoardStudents",
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
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuestBoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestBoardStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestBoardStudents_QuestBoards_QuestBoardId",
                        column: x => x.QuestBoardId,
                        principalTable: "QuestBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("64c0a3a5-849c-412d-86b6-3f5e4809cc84"), "FinishOneFinalTest", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8248), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("68bd35d3-c09b-45f3-9a05-0e10691f7c42"), "FinishOnelesson", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(7897), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("72933280-e14b-4715-a802-dcd88e031e79"), "FinishOneHomeworkMiniProject", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8193), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("7b436441-ef2f-4a83-be79-ee5ec5f18355"), "FinishOneUnit", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8237), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("bb35cdbf-98a7-4ce7-9b71-f457b386d64c"), "FinishOneLevelPass", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8260), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"), "FinishOneUnitTest", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8222), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestBoardStudents_QuestBoardId",
                table: "QuestBoardStudents",
                column: "QuestBoardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestBoardConfigs");

            migrationBuilder.DropTable(
                name: "QuestBoardStudents");

            migrationBuilder.DropTable(
                name: "QuestBoards");
        }
    }
}
