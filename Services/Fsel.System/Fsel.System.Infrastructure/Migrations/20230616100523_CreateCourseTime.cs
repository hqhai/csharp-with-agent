using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCourseTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseTimeConfigs",
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
                    CourseLevel = table.Column<int>(type: "int", nullable: false),
                    DurationMonth = table.Column<int>(type: "int", nullable: false),
                    EnrollmentWeek = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTimeConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LiveTimeFrames",
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
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveTimeFrames", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CourseTimeConfigs",
                columns: new[] { "Id", "CourseLevel", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DurationMonth", "EnrollmentWeek", "IsDeleted", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("07312932-5caf-4e01-a670-6cd4aa8650da"), 1, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(633), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null },
                    { new Guid("18e6bccf-1c88-4886-bc71-bec2c556f913"), 4, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(640), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null },
                    { new Guid("2045c855-e5a8-4818-8bf7-d477d02c1b02"), 9, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(643), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null },
                    { new Guid("382bae8a-55aa-4da0-8287-71182cb17a7c"), 8, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(645), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null },
                    { new Guid("85bc760a-be1e-497d-bf62-2126c6178479"), 6, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(641), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null },
                    { new Guid("8dd3c007-775f-4ef6-ad10-efc404c5a2be"), 2, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(636), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null },
                    { new Guid("9ceb5cf5-271c-4c53-8d2d-3d273740fccd"), 0, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(578), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null },
                    { new Guid("a0904fa2-fce2-432a-bb85-a3f87c1344b4"), 3, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(638), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null },
                    { new Guid("e0a504cf-ceb2-415a-ad73-cec5429f0e07"), 7, new DateTime(2023, 6, 16, 17, 5, 23, 346, DateTimeKind.Local).AddTicks(646), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 0, 0, false, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseTimeConfigs");

            migrationBuilder.DropTable(
                name: "LiveTimeFrames");
        }
    }
}
