using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_MenuTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MenuId",
                table: "PermissionGroups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Menus",
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
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    ConfigStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Category", "ConfigStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Index", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("049a2515-641d-447b-b27b-e46f41e64ef3"), "LMSAdmin", "{\"id\":16,\"code_title\":null,\"link\":\"/student-progress\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 19, false, "Student Progress Management", null, null, null },
                    { new Guid("06641b23-3857-4b6d-b1d7-096fc98fa8b9"), "LMSAdmin", "{\"id\":4,\"code_title\":null,\"link\":\"/student\",\"icon\":\"Students.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool,CSO\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 6, false, "Student Management By AdminSchool, CSO", null, null, null },
                    { new Guid("06e2c9b5-378b-4a32-a36f-45a65b68d791"), "LMSAdmin", "{\"id\":8,\"code_title\":null,\"link\":\"/exercise-management\",\"icon\":\"Edit.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Teacher\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 11, false, "Exercise Management", null, null, null },
                    { new Guid("121436b1-ac91-46c2-b4b8-76da74e13d33"), "LMSAdmin", "{\"id\":9,\"code_title\":null,\"link\":\"/review\",\"icon\":\"Review.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 12, false, "Review Management", null, null, null },
                    { new Guid("17234df3-b92f-4103-9bc0-7b5feaad7b00"), "LMSAdmin", "{\"id\":7,\"code_title\":null,\"link\":\"/empty-calendar\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO,Teacher\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 10, false, "Empty Calendar Management", null, null, null },
                    { new Guid("18e09c83-8f23-4509-a39c-7d3d32c4d81b"), "LMSAdmin", "{\"id\":24,\"code_title\":null,\"link\":\"/report-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":220,\"code_title\":null,\"link\":\"/report-management/pt-result\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":221,\"code_title\":null,\"link\":\"/report-management/learning-progress\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":222,\"code_title\":null,\"link\":\"/report-management/learning-result\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":223,\"code_title\":null,\"link\":\"/report-management/assiduity\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null}]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 29, false, "Report Management", null, null, null },
                    { new Guid("19f86ebc-e6e9-4baf-84d8-f0e90489b2a1"), "LMSAdmin", "{\"id\":29,\"code_title\":null,\"link\":\"/banner\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 28, false, "Banner Management", null, null, null },
                    { new Guid("1c9b88c3-c865-4d91-89df-3b84fec63843"), "LMSAdmin", "{\"id\":22,\"code_title\":null,\"link\":\"/voucher\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 24, false, "Voucher Management", null, null, null },
                    { new Guid("2d1222ff-8e49-4689-8c47-c6ebca08fb61"), "LMSAdmin", "{\"id\":11,\"code_title\":null,\"link\":\"/forbidden-word\",\"icon\":\"Forbiden.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 14, false, "Store Of Forbiden Words", null, null, null },
                    { new Guid("3e1dfc7e-a768-4316-ae01-00a20cd4a281"), "LMSAdmin", "{\"id\":2,\"code_title\":null,\"link\":\"/user/detail\",\"icon\":\"People.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Teacher,CSO\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 4, false, "User Management By Teacher, CSO", null, null, null },
                    { new Guid("464add9c-629a-4a83-99e0-70d020d72a04"), "LMSAdmin", "{\"id\":21,\"code_title\":null,\"link\":\"/invite-friends\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 23, false, "Manage Referral Code", null, null, null },
                    { new Guid("4aff9531-9c38-4ef9-a965-923585d9d47a"), "LMSAdmin", "{\"id\":15,\"code_title\":null,\"link\":\"/quest-board\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 18, false, "Quest Board Management", null, null, null },
                    { new Guid("53416e56-89d7-4fec-bb3f-c9225878470d"), "LMSAdmin", "{\"id\":26,\"code_title\":null,\"link\":\"/blind-box\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 31, false, "BlindBox Management", null, null, null },
                    { new Guid("56f3a987-a5cf-4eee-88b7-baac8a82caee"), "LMSAdmin", "{\"id\":4,\"code_title\":null,\"link\":\"/student-admin\",\"icon\":\"Students.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 7, false, "Student Management By Admin", null, null, null },
                    { new Guid("600f483b-dd9e-4aa3-9364-bbdca16a391a"), "LMSAdmin", "{\"id\":23,\"code_title\":null,\"link\":\"/popup-order-config\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 25, false, "Popup Management", null, null, null },
                    { new Guid("655bbf11-95ad-4941-8739-878437acb3c2"), "LMSAdmin", "{\"id\":18,\"code_title\":null,\"link\":\"/help-and-support-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 20, false, "Help And Support Management", null, null, null },
                    { new Guid("66b606c5-ef09-4f76-89e4-e11d0d914cb5"), "LMSAdmin", "{\"id\":27,\"code_title\":null,\"link\":\"/guide\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 32, false, "Instructions For Use", null, null, null },
                    { new Guid("68365a87-73fa-4926-acb6-24f1403f77ad"), "LMSAdmin", "{\"id\":25,\"code_title\":null,\"link\":\"/dashboard\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":250,\"code_title\":null,\"link\":\"/dashboard/progress-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":251,\"code_title\":null,\"link\":\"/dashboard/result-pt\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":252,\"code_title\":null,\"link\":\"/dashboard/result-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":253,\"code_title\":null,\"link\":\"/dashboard/diligence\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null}]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 30, false, "Dashboard", null, null, null },
                    { new Guid("6bd509c9-3986-4780-8ea0-e02059c9d9e9"), "LMSAdmin", "{\"id\":19,\"code_title\":null,\"link\":\"/error-reporting-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 21, false, "Error Report Management", null, null, null },
                    { new Guid("6ee1bea6-02ea-4502-b8cc-92671e6430fc"), "LMSAdmin", "{\"id\":20,\"code_title\":null,\"link\":\"/price-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 22, false, "Price List Management", null, null, null },
                    { new Guid("77eee0bb-70b0-4672-964c-35adb0a4df6f"), "LMSAdmin", "{\"id\":1,\"code_title\":null,\"link\":\"/home\",\"icon\":\"Home.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 1, false, "Home Page", null, null, null },
                    { new Guid("b50e1cd9-5b46-467a-ba7f-b86011747b32"), "LMSAdmin", "{\"id\":6,\"code_title\":null,\"link\":\"/live-class\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO,Teacher\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 9, false, "Time Setting", null, null, null },
                    { new Guid("be37f400-f10b-40b6-bc01-ad0e3d114fce"), "LMSAdmin", "{\"id\":229,\"code_title\":null,\"link\":\"/course-target\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 26, false, "Course Objective Management", null, null, null },
                    { new Guid("c13b856d-a3ee-40fa-a018-2d33ce028f6a"), "LMSAdmin", "{\"id\":12,\"code_title\":null,\"link\":\"/payment\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 15, false, "Payment Management", null, null, null },
                    { new Guid("c3851e05-cda0-4418-b852-8304949104a0"), "LMSAdmin", "{\"id\":14,\"code_title\":null,\"link\":\"/class-forum-management\",\"icon\":\"Chat.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 17, false, "Class Forum Management", null, null, null },
                    { new Guid("cd067ac8-af65-47bd-bbd4-0bf3b9ad90fb"), "LMSAdmin", "{\"id\":13,\"code_title\":null,\"link\":\"/voucher-management\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 16, false, "Voucher Management", null, null, null },
                    { new Guid("d5b153cc-1481-40d3-b39f-b35a67dd54e6"), "LMSAdmin", "{\"id\":3,\"code_title\":null,\"link\":\"/class\",\"icon\":\"Presentation.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 5, false, "Class Management", null, null, null },
                    { new Guid("de64d9a6-6011-4a25-b73f-af31a0c530a1"), "LMSAdmin", "{\"id\":2,\"code_title\":null,\"link\":\"/user\",\"icon\":\"People.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 2, false, "User Management By Admin", null, null, null },
                    { new Guid("e7b3e33f-c1cb-48e7-bbe0-39be74dd5e36"), "LMSAdmin", "{\"id\":30,\"code_title\":null,\"link\":\"/gift\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 27, false, "FSEL Gift Management", null, null, null },
                    { new Guid("efb2d814-6b8b-4c12-92a4-21fde9feeb54"), "LMSAdmin", "{\"id\":27,\"code_title\":null,\"link\":\"/user-permissions\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[{\"id\":270,\"code_title\":null,\"link\":\"/user-permissions/user-group\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":271,\"code_title\":null,\"link\":\"/user-permissions/permission-group\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":272,\"code_title\":null,\"link\":\"/user-permissions/permission-list\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":273,\"code_title\":null,\"link\":\"/user-permissions/permission-access\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null}]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 3, false, "Permission Management", null, null, null },
                    { new Guid("f768e305-26aa-4bca-9197-c3673825373e"), "LMSAdmin", "{\"id\":5,\"code_title\":null,\"link\":\"/live-time\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 8, false, "Live Time Management", null, null, null },
                    { new Guid("f8759866-2981-4751-a447-d6c0230fa44a"), "LMSAdmin", "{\"id\":10,\"code_title\":null,\"link\":\"/setup-time\",\"icon\":\"Clock.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 13, false, "Set Time", null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("22960350-3a0f-468b-849f-bf0fd7db264b"),
                column: "MenuId",
                value: new Guid("600f483b-dd9e-4aa3-9364-bbdca16a391a"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("31bafbd0-7346-4aaf-a1fa-bf2194b1be68"),
                column: "MenuId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("39d8915b-4373-4ed2-9a79-d59db3a5285f"),
                column: "MenuId",
                value: new Guid("049a2515-641d-447b-b27b-e46f41e64ef3"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("3d33a7c7-2cf2-4383-9487-e88c0640d12c"),
                column: "MenuId",
                value: new Guid("e7b3e33f-c1cb-48e7-bbe0-39be74dd5e36"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("57cbd9f0-87d5-4c5c-8986-08e6fae58d99"),
                column: "MenuId",
                value: new Guid("1c9b88c3-c865-4d91-89df-3b84fec63843"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("5f09fd6f-3a0b-4c40-85b6-0d5cbd85c3ce"),
                column: "MenuId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("68e5e3d2-90a1-4c60-9b8f-77a03381dc92"),
                column: "MenuId",
                value: new Guid("56f3a987-a5cf-4eee-88b7-baac8a82caee"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("6f12e0d0-774c-4868-a062-e94b0b6ecf2d"),
                column: "MenuId",
                value: new Guid("53416e56-89d7-4fec-bb3f-c9225878470d"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("89b8e2df-df40-4cb6-9f86-7a7fa315a194"),
                column: "MenuId",
                value: new Guid("77eee0bb-70b0-4672-964c-35adb0a4df6f"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("95a2cf57-3b18-41b4-b5d5-9291a7bb87a5"),
                column: "MenuId",
                value: new Guid("4aff9531-9c38-4ef9-a965-923585d9d47a"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("9fc99947-c7a9-44aa-b9eb-ea42b28590fc"),
                column: "MenuId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("a1a7a7be-8865-49b1-a0be-bb573b888fd3"),
                column: "MenuId",
                value: new Guid("18e09c83-8f23-4509-a39c-7d3d32c4d81b"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("ab4582fd-e613-4904-bdc3-c50ff03a1d4d"),
                column: "MenuId",
                value: new Guid("6bd509c9-3986-4780-8ea0-e02059c9d9e9"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("b0a0b3b1-35aa-4ef5-8c17-95a5b7cf7a2b"),
                column: "MenuId",
                value: new Guid("d5b153cc-1481-40d3-b39f-b35a67dd54e6"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("b2b1272c-3b9d-4c6e-91f4-60c302c7d7e1"),
                column: "MenuId",
                value: new Guid("cd067ac8-af65-47bd-bbd4-0bf3b9ad90fb"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("b369b45f-2b8e-4cb0-91c2-2ad73d29d88d"),
                column: "MenuId",
                value: new Guid("c13b856d-a3ee-40fa-a018-2d33ce028f6a"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("b652d6d2-f5f0-4c5d-a8b1-ec58860cc6d7"),
                column: "MenuId",
                value: new Guid("464add9c-629a-4a83-99e0-70d020d72a04"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"),
                column: "MenuId",
                value: new Guid("de64d9a6-6011-4a25-b73f-af31a0c530a1"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("d9679732-17a3-44ff-b4ed-b39f631c3e13"),
                column: "MenuId",
                value: new Guid("19f86ebc-e6e9-4baf-84d8-f0e90489b2a1"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("df96b682-99c5-4ae2-9516-329d2f2d3054"),
                column: "MenuId",
                value: new Guid("be37f400-f10b-40b6-bc01-ad0e3d114fce"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("e4961eb7-931a-4403-b94a-fb2f7860b80e"),
                column: "MenuId",
                value: new Guid("6ee1bea6-02ea-4502-b8cc-92671e6430fc"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("e758b51b-74de-4a2b-87b3-394f701bce9f"),
                column: "MenuId",
                value: new Guid("2d1222ff-8e49-4689-8c47-c6ebca08fb61"));

            migrationBuilder.UpdateData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("f9142961-5fe0-4015-8d01-9e12d202a7ae"),
                column: "MenuId",
                value: new Guid("efb2d814-6b8b-4c12-92a4-21fde9feeb54"));

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "ClaimType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "MenuId", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("618be731-43b3-45ed-bfad-262945ef0d51"), "SchoolStudentManagement", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, new Guid("06641b23-3857-4b6d-b1d7-096fc98fa8b9"), "Quản lý học sinh của AdminSchool, CSO", true, null, null, null });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("1e938573-3317-449a-b322-76225eb87039"), "SchoolStudentManagement.Update", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin học sinh", new Guid("618be731-43b3-45ed-bfad-262945ef0d51"), true, null, null, null },
                    { new Guid("79e900b5-cb1a-49bb-a8ad-33925b97e095"), "SchoolStudentManagement.View", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách, thông tin học sinh", new Guid("618be731-43b3-45ed-bfad-262945ef0d51"), true, null, null, null },
                    { new Guid("90897566-3edf-4aae-969a-9acb31c80c0b"), "SchoolStudentManagement.Delete", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa học sinh", new Guid("618be731-43b3-45ed-bfad-262945ef0d51"), true, null, null, null },
                    { new Guid("ce8b975b-6f31-454e-b21f-18a35061e266"), "SchoolStudentManagement.Add", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới học sinh", new Guid("618be731-43b3-45ed-bfad-262945ef0d51"), true, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionGroups_MenuId",
                table: "PermissionGroups",
                column: "MenuId",
                unique: true,
                filter: "[MenuId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionGroups_Menus_MenuId",
                table: "PermissionGroups",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionGroups_Menus_MenuId",
                table: "PermissionGroups");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_PermissionGroups_MenuId",
                table: "PermissionGroups");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1e938573-3317-449a-b322-76225eb87039"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("79e900b5-cb1a-49bb-a8ad-33925b97e095"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("90897566-3edf-4aae-969a-9acb31c80c0b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ce8b975b-6f31-454e-b21f-18a35061e266"));

            migrationBuilder.DeleteData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("618be731-43b3-45ed-bfad-262945ef0d51"));

            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "PermissionGroups");
        }
    }
}
