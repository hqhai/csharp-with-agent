using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Permission_WeeklyProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("f0bf2b43-f5b9-490b-8c8f-822fa70188a4"), "DashboardManagementByAdminSchool.ViewWeeklyProgress", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo mục tiêu tuần", new Guid("933dca96-a6a7-4b37-870b-b423f3c2165c"), true, null, null, null },
                    { new Guid("f4d1e599-553a-4cbe-ae13-8aaa61785978"), "StudentManagement.Delete", new DateTime(2025, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa", new Guid("68e5e3d2-90a1-4c60-9b8f-77a03381dc92"), true, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f0bf2b43-f5b9-490b-8c8f-822fa70188a4"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f4d1e599-553a-4cbe-ae13-8aaa61785978"));
        }
    }
}
