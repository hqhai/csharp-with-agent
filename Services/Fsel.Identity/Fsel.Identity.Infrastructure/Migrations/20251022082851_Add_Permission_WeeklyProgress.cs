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
                table: "Menus",
                columns: new[] { "Id", "Category", "ConfigStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Index", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("715d7a0c-0691-41f6-8d0f-721d286f555a"), "LMSAdmin", "{\"id\":33,\"code_title\":\"Quản lý tiến độ tuần\",\"link\":\"/weekly-goal-progress-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,AdminSchool\",\"children\":[]}", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 36, false, "Weekly Progress Management", null, null, null });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("89289a4e-af45-45ad-aaf1-ab7064f7d008"), "StudentManagement.DeleteStudentGoal", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa thiết lập mục tiêu tuần", new Guid("68e5e3d2-90a1-4c60-9b8f-77a03381dc92"), true, null, null, null },
                    { new Guid("9fb7b4ae-206a-49bb-a6a2-9b35f05342e2"), "DashboardManagementByAdminSchool.ViewWeeklyProgress", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem báo cáo mục tiêu tuần", new Guid("933dca96-a6a7-4b37-870b-b423f3c2165c"), true, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "ClaimType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "MenuId", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("e275d7b4-ab06-4c7a-8058-4fae05f78681"), "WeeklyProgressManagement", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, new Guid("715d7a0c-0691-41f6-8d0f-721d286f555a"), "Quản lý tiến độ tuần", true, null, null, null });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0ab9fdc3-44b5-4755-a29b-f6c9557492c2"), "WeeklyProgressManagement.ViewStudentGoal", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem cấu hình mục tiêu khóa học", new Guid("e275d7b4-ab06-4c7a-8058-4fae05f78681"), true, null, null, null },
                    { new Guid("310ec95e-d314-49e2-b3a3-ac3705fa9049"), "WeeklyProgressManagement.AddStudentGoal", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm cấu hình mục tiêu khóa học", new Guid("e275d7b4-ab06-4c7a-8058-4fae05f78681"), true, null, null, null },
                    { new Guid("718ed155-b71e-4a5e-aece-c78441d18ede"), "WeeklyProgressManagement.View", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem tiến độ tuần", new Guid("e275d7b4-ab06-4c7a-8058-4fae05f78681"), true, null, null, null },
                    { new Guid("eac0e1e4-dfdd-45fb-bda7-3f8f5ce3cecc"), "WeeklyProgressManagement.DeleteStudentGoal", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa cấu hình mục tiêu khóa học", new Guid("e275d7b4-ab06-4c7a-8058-4fae05f78681"), true, null, null, null },
                    { new Guid("f9d496fe-b18c-47b0-96e8-925091bac4ff"), "WeeklyProgressManagement.UpdateStudentGoal", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Update cấu hình mục tiêu khóa học", new Guid("e275d7b4-ab06-4c7a-8058-4fae05f78681"), true, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0ab9fdc3-44b5-4755-a29b-f6c9557492c2"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("310ec95e-d314-49e2-b3a3-ac3705fa9049"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("718ed155-b71e-4a5e-aece-c78441d18ede"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("89289a4e-af45-45ad-aaf1-ab7064f7d008"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9fb7b4ae-206a-49bb-a6a2-9b35f05342e2"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("eac0e1e4-dfdd-45fb-bda7-3f8f5ce3cecc"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f9d496fe-b18c-47b0-96e8-925091bac4ff"));

            migrationBuilder.DeleteData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("e275d7b4-ab06-4c7a-8058-4fae05f78681"));

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("715d7a0c-0691-41f6-8d0f-721d286f555a"));
        }
    }
}
