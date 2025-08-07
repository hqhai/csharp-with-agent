using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Dashboard_Report_Manager_Permission_Group : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("a1a7a7be-8865-49b1-a0be-bb573b888fd3"),
                columns: new[] { "ClaimType", "Name" },
                values: new object[] { "ReportManagementByAdminSchool", "Quản lý báo cáo của Admin School" });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "ClaimType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "MenuId", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("933dca96-a6a7-4b37-870b-b423f3c2165c"), "DashboardManagementByAdminSchool", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, new Guid("68365a87-73fa-4926-acb6-24f1403f77ad"), "Quản lý Dashboard của AdminSchool", true, null, null, null });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09748430-9ce7-4f22-a777-2516fc98d16c"),
                column: "ClaimValue",
                value: "ReportManagementByAdminSchool.ViewAttendanceReport");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("27b1c0ae-d63a-4553-a130-e008183940c7"),
                column: "ClaimValue",
                value: "ReportManagementByAdminSchool.ViewLearningResultsReport");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6df0f361-0d99-4a38-b5be-706225b7f22b"),
                column: "ClaimValue",
                value: "ReportManagementByAdminSchool.ViewPTResultsReport");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a092a54c-1306-4600-af02-8cae97875977"),
                column: "ClaimValue",
                value: "ReportManagementByAdminSchool.ViewLearningProgressReport");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("18f69e03-68d9-4be2-8d86-126f3fd87022"), "DashboardManagementByAdminSchool.ViewLearningProgressReport", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo tiến độ học tập", new Guid("933dca96-a6a7-4b37-870b-b423f3c2165c"), true, null, null, null },
                    { new Guid("1aacc9b4-8abb-4e18-9583-acaab2a302aa"), "DashboardManagementByAdminSchool.ViewLearningResultsReport", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo kết quả học tập", new Guid("933dca96-a6a7-4b37-870b-b423f3c2165c"), true, null, null, null },
                    { new Guid("29799ab5-71de-444e-b68e-9c00f9160346"), "DashboardManagementByAdminSchool.ViewAttendanceReport", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo chuyên cần", new Guid("933dca96-a6a7-4b37-870b-b423f3c2165c"), true, null, null, null },
                    { new Guid("e5e6f8f3-aaa3-480f-bd6e-f005aec41167"), "DashboardManagementByAdminSchool.ViewPTResultsReport", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo kết quả đánh giá đầu vào", new Guid("933dca96-a6a7-4b37-870b-b423f3c2165c"), true, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("18f69e03-68d9-4be2-8d86-126f3fd87022"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1aacc9b4-8abb-4e18-9583-acaab2a302aa"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("29799ab5-71de-444e-b68e-9c00f9160346"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5e6f8f3-aaa3-480f-bd6e-f005aec41167"));

            migrationBuilder.DeleteData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("933dca96-a6a7-4b37-870b-b423f3c2165c"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("a1a7a7be-8865-49b1-a0be-bb573b888fd3"),
                columns: new[] { "ClaimType", "Name" },
                values: new object[] { "ReportManagement", "Quản lý báo cáo" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09748430-9ce7-4f22-a777-2516fc98d16c"),
                column: "ClaimValue",
                value: "ReportManagement.ViewDiligence");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("27b1c0ae-d63a-4553-a130-e008183940c7"),
                column: "ClaimValue",
                value: "ReportManagement.ViewLearningResults");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6df0f361-0d99-4a38-b5be-706225b7f22b"),
                column: "ClaimValue",
                value: "ReportManagement.ViewPT");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a092a54c-1306-4600-af02-8cae97875977"),
                column: "ClaimValue",
                value: "ReportManagement.ViewLearningProgress");
        }
    }
}
