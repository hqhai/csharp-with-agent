using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourseTimeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("07312932-5caf-4e01-a670-6cd4aa8650da"));

            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("18e6bccf-1c88-4886-bc71-bec2c556f913"));

            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2045c855-e5a8-4818-8bf7-d477d02c1b02"));

            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("382bae8a-55aa-4da0-8287-71182cb17a7c"));

            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("85bc760a-be1e-497d-bf62-2126c6178479"));

            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("8dd3c007-775f-4ef6-ad10-efc404c5a2be"));

            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9ceb5cf5-271c-4c53-8d2d-3d273740fccd"));

            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a0904fa2-fce2-432a-bb85-a3f87c1344b4"));

            migrationBuilder.DeleteData(
                table: "CourseTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e0a504cf-ceb2-415a-ad73-cec5429f0e07"));

            migrationBuilder.DropColumn(
                name: "CourseLevel",
                table: "CourseTimeConfigs");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "CourseTimeConfigs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "CourseTimeConfigs");

            migrationBuilder.AddColumn<int>(
                name: "CourseLevel",
                table: "CourseTimeConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
    }
}
