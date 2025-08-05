using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Menu_Survey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Category", "ConfigStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Index", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("49dece94-b2de-4271-88ef-72f3e2461a2f"), "LMSAdmin", "{\"id\":27,\"code_title\":\"Quản lý khảo sát\",\"link\":\"/config-servey\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 33, false, "Survey management", null, null, null });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "ClaimType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "MenuId", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("d99eb96a-55dd-4fed-b755-9f33fe52f0e7"), "SurveyManagement", new DateTime(2025, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, new Guid("49dece94-b2de-4271-88ef-72f3e2461a2f"), "Quản lý khảo sát", true, null, null, null });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("21243a48-7cf4-40bd-8f65-0209e97ebd9e"), "SurveyManagement.Delete", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa khảo sát", new Guid("d99eb96a-55dd-4fed-b755-9f33fe52f0e7"), true, null, null, null },
                    { new Guid("22751359-1559-4b69-962b-cb401a4abc36"), "SurveyManagement.Update", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Cập nhật khảo sát", new Guid("d99eb96a-55dd-4fed-b755-9f33fe52f0e7"), true, null, null, null },
                    { new Guid("3ac8acfd-aa22-418e-9937-120196778127"), "SurveyManagement.Add", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm khảo sát", new Guid("d99eb96a-55dd-4fed-b755-9f33fe52f0e7"), true, null, null, null },
                    { new Guid("63b8f5af-7432-4ad2-9499-95eb9368612d"), "SurveyManagement.View", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem khảo sát", new Guid("d99eb96a-55dd-4fed-b755-9f33fe52f0e7"), true, null, null, null },
                    { new Guid("af2ea7ab-77dc-487c-bda2-082baa34d4e8"), "SurveyManagement.Export", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xuất dữ liệu khảo sát", new Guid("d99eb96a-55dd-4fed-b755-9f33fe52f0e7"), true, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("21243a48-7cf4-40bd-8f65-0209e97ebd9e"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("22751359-1559-4b69-962b-cb401a4abc36"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3ac8acfd-aa22-418e-9937-120196778127"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("63b8f5af-7432-4ad2-9499-95eb9368612d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("af2ea7ab-77dc-487c-bda2-082baa34d4e8"));

            migrationBuilder.DeleteData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("d99eb96a-55dd-4fed-b755-9f33fe52f0e7"));

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("49dece94-b2de-4271-88ef-72f3e2461a2f"));
        }
    }
}
