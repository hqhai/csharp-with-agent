using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PermissionTable_Add_PermissionReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("09748430-9ce7-4f22-a777-2516fc98d16c"), "ReportManagement.ViewDiligence", new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo chuyên cần", new Guid("a1a7a7be-8865-49b1-a0be-bb573b888fd3"), true, null, null, null },
                    { new Guid("27b1c0ae-d63a-4553-a130-e008183940c7"), "ReportManagement.ViewLearningResults", new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo kết quả học tập", new Guid("a1a7a7be-8865-49b1-a0be-bb573b888fd3"), true, null, null, null },
                    { new Guid("6df0f361-0d99-4a38-b5be-706225b7f22b"), "ReportManagement.ViewPT", new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo kết quả đánh giá đầu vào", new Guid("a1a7a7be-8865-49b1-a0be-bb573b888fd3"), true, null, null, null },
                    { new Guid("a092a54c-1306-4600-af02-8cae97875977"), "ReportManagement.ViewLearningProgress", new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo tiến độ học tập", new Guid("a1a7a7be-8865-49b1-a0be-bb573b888fd3"), true, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09748430-9ce7-4f22-a777-2516fc98d16c"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("27b1c0ae-d63a-4553-a130-e008183940c7"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6df0f361-0d99-4a38-b5be-706225b7f22b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a092a54c-1306-4600-af02-8cae97875977"));
        }
    }
}
