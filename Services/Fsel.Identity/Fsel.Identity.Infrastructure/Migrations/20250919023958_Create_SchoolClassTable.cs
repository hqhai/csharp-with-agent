using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_SchoolClassTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolClassId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SchoolClasses",
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
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolClasses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayOrder", "IsActive", "IsDefault", "IsDeleted", "LocationIdStr", "Name", "NormalizedName", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("52120645-e701-4e83-a61d-32d99192abec"), null, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 0, false, true, false, null, "StudentCampus", "STUDENTCAMPUS", null, null, null },
                    { new Guid("daaa1fa3-e040-4344-bfbd-a7b0d32c6bd4"), null, new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 0, false, true, false, null, "TeacherCampus", "TEACHERCAMPUS", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Category", "ConfigStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Index", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("9b06098b-4328-43b0-a1bd-0d0d12d87d7c"), "LMSAdmin", "{\"id\":31,\"code_title\":\"Quản lý học sinh(School)\",\"link\":\"/student-school-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":[]}", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 34, false, "School Student Management", null, null, null },
                    { new Guid("d7c5e83c-5ec4-479e-b0dc-4dd3784e36e4"), "LMSAdmin", "{\"id\":32,\"code_title\":\"Quản lý lớp(School)\",\"link\":\"/class-school-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":[]}", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 35, false, "School Class Management", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "ClaimType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "MenuId", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0b734124-0372-488c-8321-2129484f8adb"), "SchoolClassCampusManagement", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, new Guid("d7c5e83c-5ec4-479e-b0dc-4dd3784e36e4"), "Quản lý lớp Campus", true, null, null, null },
                    { new Guid("642f9c9c-c338-48fa-a42a-6d22e766bfca"), "StudentCampusManagement", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, new Guid("9b06098b-4328-43b0-a1bd-0d0d12d87d7c"), "Quản lý học sinh Campus", true, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("1d7feea3-a183-4a24-a9fa-bc6fa77847c7"), "SchoolClassCampusManagement.View", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin lớp", new Guid("0b734124-0372-488c-8321-2129484f8adb"), true, null, null, null },
                    { new Guid("2638eed4-5578-4e3b-98c2-b493ee8b5aef"), "SchoolClassCampusManagement.Update", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Cập nhật lớp", new Guid("0b734124-0372-488c-8321-2129484f8adb"), true, null, null, null },
                    { new Guid("2e7ab492-59cf-4b14-a2ce-5cf2a2692427"), "StudentCampusManagement.View", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin học sinh", new Guid("642f9c9c-c338-48fa-a42a-6d22e766bfca"), true, null, null, null },
                    { new Guid("873f2abd-1de2-41a9-8ab8-ce49de12ca60"), "SchoolClassCampusManagement.DeleteStudents", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa học sinh khỏi lớp", new Guid("0b734124-0372-488c-8321-2129484f8adb"), true, null, null, null },
                    { new Guid("a1a48ab9-8502-4d3b-8346-f267d8d8c680"), "SchoolClassCampusManagement.Delete", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa lớp", new Guid("0b734124-0372-488c-8321-2129484f8adb"), true, null, null, null },
                    { new Guid("da8ac310-dd13-4386-9c49-2fe376d33a0d"), "SchoolClassCampusManagement.AddStudents", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm học sinh vào lớp", new Guid("0b734124-0372-488c-8321-2129484f8adb"), true, null, null, null },
                    { new Guid("ec393b2d-0a9f-45e8-a1a0-13e8fb66ecd9"), "SchoolClassCampusManagement.Add", new DateTime(2025, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới lớp", new Guid("0b734124-0372-488c-8321-2129484f8adb"), true, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "HumanId", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProvinceId", "School", "SchoolClassId", "SchoolFaculty", "SchoolGrade", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolClassId",
                table: "Students",
                column: "SchoolClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_SchoolClasses_SchoolClassId",
                table: "Students",
                column: "SchoolClassId",
                principalTable: "SchoolClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_SchoolClasses_SchoolClassId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "SchoolClasses");

            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_SchoolClassId",
                table: "Students");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("52120645-e701-4e83-a61d-32d99192abec"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("daaa1fa3-e040-4344-bfbd-a7b0d32c6bd4"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1d7feea3-a183-4a24-a9fa-bc6fa77847c7"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2638eed4-5578-4e3b-98c2-b493ee8b5aef"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2e7ab492-59cf-4b14-a2ce-5cf2a2692427"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("873f2abd-1de2-41a9-8ab8-ce49de12ca60"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1a48ab9-8502-4d3b-8346-f267d8d8c680"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("da8ac310-dd13-4386-9c49-2fe376d33a0d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ec393b2d-0a9f-45e8-a1a0-13e8fb66ecd9"));

            migrationBuilder.DeleteData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("0b734124-0372-488c-8321-2129484f8adb"));

            migrationBuilder.DeleteData(
                table: "PermissionGroups",
                keyColumn: "Id",
                keyValue: new Guid("642f9c9c-c338-48fa-a42a-6d22e766bfca"));

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("9b06098b-4328-43b0-a1bd-0d0d12d87d7c"));

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("d7c5e83c-5ec4-479e-b0dc-4dd3784e36e4"));

            migrationBuilder.DropColumn(
                name: "SchoolClassId",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "HumanId", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProvinceId", "School", "SchoolFaculty", "SchoolGrade", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }
    }
}
