using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Weekly_Student_Progress_Management_Permission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Category", "ConfigStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Index", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("b40fbc8d-355c-47f2-ad01-cc5655fcd66a"), "LMSAdmin", "{\"id\":28,\"code_title\":\"Quản lý tiến độ học sinh tuần\",\"link\":\"/weekly-goal-progress-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":[]}", new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 33, false, "Weekly Student Progress Management", null, null, null });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "ClaimType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "MenuId", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("bcd6001c-6bd4-4054-ba56-ee7e2e3a1cd5"), "StudentProgressWeeklyManagement", new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, new Guid("b40fbc8d-355c-47f2-ad01-cc5655fcd66a"), "Quản lý tiến độ học sinh tuần", true, null, null, null });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("4042a45a-8997-445e-aec8-e5dacc1bed25"), "StudentProgressWeeklyManagement.Delete", new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa", new Guid("bcd6001c-6bd4-4054-ba56-ee7e2e3a1cd5"), true, null, null, null },
                    { new Guid("ba9920d2-e1bd-4e6a-938a-58470a4cdf89"), "StudentProgressWeeklyManagement.View", new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem", new Guid("bcd6001c-6bd4-4054-ba56-ee7e2e3a1cd5"), true, null, null, null },
                    { new Guid("d3d34bdf-b472-4a2c-b7c3-b6f763153efb"), "StudentProgressWeeklyManagement.Add", new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm", new Guid("bcd6001c-6bd4-4054-ba56-ee7e2e3a1cd5"), true, null, null, null },
                    { new Guid("f7df2ab2-68c8-42cb-9ca8-e9f9945e3de5"), "StudentProgressWeeklyManagement.Update", new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Sửa", new Guid("bcd6001c-6bd4-4054-ba56-ee7e2e3a1cd5"), true, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4042a45a-8997-445e-aec8-e5dacc1bed25"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ba9920d2-e1bd-4e6a-938a-58470a4cdf89"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d3d34bdf-b472-4a2c-b7c3-b6f763153efb"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f7df2ab2-68c8-42cb-9ca8-e9f9945e3de5"));

            migrationBuilder.DeleteData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("bcd6001c-6bd4-4054-ba56-ee7e2e3a1cd5"));

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("b40fbc8d-355c-47f2-ad01-cc5655fcd66a"));
        }
    }
}
