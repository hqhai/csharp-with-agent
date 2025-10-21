using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Permission_CurriculumManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Category", "ConfigStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Index", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("40217f52-0b4b-4c95-b5f8-c1317deba995"), "LMSAdmin", "{\"id\":28,\"code_title\":\"Quản lý giáo trình\",\"link\":\"/curriculum-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":[]}", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 33, false, "Curriculum Management", null, null, null });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "ClaimType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "MenuId", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("58cfa719-3af0-43a0-be7f-0a476151e7bd"), "CurriculumManagement", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, new Guid("40217f52-0b4b-4c95-b5f8-c1317deba995"), "Quản lý giáo trình", true, null, null, null });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("324bb73f-6459-4d16-b33a-8db6b100e525"), "CurriculumManagement.DeleteStudents", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa học sinh khỏi giáo trình", new Guid("58cfa719-3af0-43a0-be7f-0a476151e7bd"), true, null, null, null },
                    { new Guid("5535786d-216c-4f3e-b25f-e533a1a7ea3b"), "CurriculumManagement.AddStudents", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm học sinh vào giáo trình", new Guid("58cfa719-3af0-43a0-be7f-0a476151e7bd"), true, null, null, null },
                    { new Guid("68905ab5-68ce-4cc1-a52a-e5ab46879460"), "CurriculumManagement.Delete", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa giáo trình", new Guid("58cfa719-3af0-43a0-be7f-0a476151e7bd"), true, null, null, null },
                    { new Guid("6ea0c374-f82d-4479-ad16-18d70acf92f8"), "CurriculumManagement.Update", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Cập nhật giáo trình", new Guid("58cfa719-3af0-43a0-be7f-0a476151e7bd"), true, null, null, null },
                    { new Guid("b2f659ab-38cf-4dc7-8c0e-d4c9311becd9"), "CurriculumManagement.View", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin giáo trình", new Guid("58cfa719-3af0-43a0-be7f-0a476151e7bd"), true, null, null, null },
                    { new Guid("be4ef82b-ef95-42a0-a15d-e9f9ab36905e"), "CurriculumManagement.Add", new DateTime(2025, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm giáo trình", new Guid("58cfa719-3af0-43a0-be7f-0a476151e7bd"), true, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("324bb73f-6459-4d16-b33a-8db6b100e525"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5535786d-216c-4f3e-b25f-e533a1a7ea3b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("68905ab5-68ce-4cc1-a52a-e5ab46879460"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6ea0c374-f82d-4479-ad16-18d70acf92f8"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b2f659ab-38cf-4dc7-8c0e-d4c9311becd9"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("be4ef82b-ef95-42a0-a15d-e9f9ab36905e"));

            migrationBuilder.DeleteData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("58cfa719-3af0-43a0-be7f-0a476151e7bd"));

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("40217f52-0b4b-4c95-b5f8-c1317deba995"));
        }
    }
}
