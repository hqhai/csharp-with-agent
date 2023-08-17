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
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfStars = table.Column<int>(type: "int", nullable: false),
                    RepeatType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                name: "QuestBoardTasks",
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
                    ImplementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DependentTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuestBoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestBoardTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestBoardTasks_QuestBoards_QuestBoardId",
                        column: x => x.QuestBoardId,
                        principalTable: "QuestBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestBoardTaskStudents",
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
                    QuestBoardTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestBoardTaskStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestBoardTaskStudents_QuestBoardTasks_QuestBoardTaskId",
                        column: x => x.QuestBoardTaskId,
                        principalTable: "QuestBoardTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("64c0a3a5-849c-412d-86b6-3f5e4809cc84"), "FinishOneFinalTest", new DateTime(2023, 8, 15, 17, 52, 3, 918, DateTimeKind.Local).AddTicks(8537), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("68bd35d3-c09b-45f3-9a05-0e10691f7c42"), "FinishOnelesson", new DateTime(2023, 8, 15, 17, 52, 3, 918, DateTimeKind.Local).AddTicks(8193), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("72933280-e14b-4715-a802-dcd88e031e79"), "FinishOneHomeworkMiniProject", new DateTime(2023, 8, 15, 17, 52, 3, 918, DateTimeKind.Local).AddTicks(8462), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("7b436441-ef2f-4a83-be79-ee5ec5f18355"), "FinishOneUnit", new DateTime(2023, 8, 15, 17, 52, 3, 918, DateTimeKind.Local).AddTicks(8499), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("bb35cdbf-98a7-4ce7-9b71-f457b386d64c"), "FinishOneLevelPass", new DateTime(2023, 8, 15, 17, 52, 3, 918, DateTimeKind.Local).AddTicks(8548), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"), "FinishOneUnitTest", new DateTime(2023, 8, 15, 17, 52, 3, 918, DateTimeKind.Local).AddTicks(8484), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("e0f52102-85de-4719-99e0-72f88450dcdd"), "FinishTheFirstFinalTest", new DateTime(2023, 8, 15, 17, 52, 3, 918, DateTimeKind.Local).AddTicks(8511), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("eff567fd-a061-48fb-9a9c-3407d17e7b0f"), "FinishTheFirstLevelPass", new DateTime(2023, 8, 15, 17, 52, 3, 918, DateTimeKind.Local).AddTicks(8526), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestBoardTasks_QuestBoardId",
                table: "QuestBoardTasks",
                column: "QuestBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestBoardTaskStudents_QuestBoardTaskId",
                table: "QuestBoardTaskStudents",
                column: "QuestBoardTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestBoardConfigs");

            migrationBuilder.DropTable(
                name: "QuestBoardTaskStudents");

            migrationBuilder.DropTable(
                name: "QuestBoardTasks");

            migrationBuilder.DropTable(
                name: "QuestBoards");
        }
    }
}
