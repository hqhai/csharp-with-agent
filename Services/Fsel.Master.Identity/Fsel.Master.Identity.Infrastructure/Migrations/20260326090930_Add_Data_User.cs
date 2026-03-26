using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Master.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Data_User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserEvents",
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
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolIdsStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEvents", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayOrder", "IsActive", "IsDefault", "IsDeleted", "Name", "NormalizedName", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("45c50c05-b829-4e08-99ee-fe33ee5d9c7d"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "School Admin", 0, true, false, false, "AdminSchool", "ADMINSCHOOL", null, null, null },
                    { new Guid("5b2931aa-ec59-4d6d-9d41-ccadddeecb3e"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Admin", 0, true, false, false, "Admin", "ADMIN", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "AvatarPath", "Birthday", "Code", "ConcurrencyStamp", "CreatedDate", "CreatedFullName", "CreatedUserId", "DefaultPassword", "DeletedDate", "DeletedFullName", "DeletedUserId", "Email", "EmailConfirmed", "FirstName", "Gender", "IsDeleted", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Status", "TwoFactorEnabled", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "UserName" },
                values: new object[,]
                {
                    { new Guid("a0280eb2-9b2f-4eeb-baf9-d3ddfe7081df"), 0, null, null, null, null, "34567890-1234-5678-9012-345678901234", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, "admin@gmail.com", true, "System", null, false, "Admin", false, null, "ADMIN@GMAIL.COM", "ADMIN", "AQAAAAIAAYagAAAAEN83Sc3QsvUH8lVUlz81plwrfPu5gnMLM16gxOrs31HfkTkb5PKqDyBNf70RIA4EQg==", null, false, "ASDFGHJKLQWERTYUIOPZXCVBNM123456", 1, false, null, null, null, "admin" },
                    { new Guid("cf7a86ad-acdc-4f6c-8bed-a1aa8c8cb5f4"), 0, null, null, null, null, "34567890-1234-5678-9012-345678901234", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, "adminschool@gmail.com", true, "School", null, false, "Admin", false, null, "ADMINSCHOOL@GMAIL.COM", "ADMINSCHOOL", "AQAAAAIAAYagAAAAEN83Sc3QsvUH8lVUlz81plwrfPu5gnMLM16gxOrs31HfkTkb5PKqDyBNf70RIA4EQg==", null, false, "ASDFGHJKLQWERTYUIOPZXCVBNM123456", 1, false, null, null, null, "adminschool" }
                });

            migrationBuilder.InsertData(
                table: "UserEvents",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "EventCode", "IsDeleted", "SchoolIdsStr", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "UserId" },
                values: new object[,]
                {
                    { new Guid("1fde20e0-4786-4447-aa72-88baec834af4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "EVT_SCHOOL_02", false, "[\"d942c4b8-f078-4ea7-86c2-1d5ea4aa1c85\", \"e089d80d-85fa-4c4c-befb-d98c39e262bb\"]", null, null, null, new Guid("cf7a86ad-acdc-4f6c-8bed-a1aa8c8cb5f4") },
                    { new Guid("8fcaae15-9c16-43e5-9a84-0abcb6afc353"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "EVT_ADMIN_01", false, null, null, null, null, new Guid("a0280eb2-9b2f-4eeb-baf9-d3ddfe7081df") }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId", "IsActive" },
                values: new object[,]
                {
                    { new Guid("5b2931aa-ec59-4d6d-9d41-ccadddeecb3e"), new Guid("a0280eb2-9b2f-4eeb-baf9-d3ddfe7081df"), true },
                    { new Guid("45c50c05-b829-4e08-99ee-fe33ee5d9c7d"), new Guid("cf7a86ad-acdc-4f6c-8bed-a1aa8c8cb5f4"), true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEvents");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("5b2931aa-ec59-4d6d-9d41-ccadddeecb3e"), new Guid("a0280eb2-9b2f-4eeb-baf9-d3ddfe7081df") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("45c50c05-b829-4e08-99ee-fe33ee5d9c7d"), new Guid("cf7a86ad-acdc-4f6c-8bed-a1aa8c8cb5f4") });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("45c50c05-b829-4e08-99ee-fe33ee5d9c7d"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("5b2931aa-ec59-4d6d-9d41-ccadddeecb3e"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("a0280eb2-9b2f-4eeb-baf9-d3ddfe7081df"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("cf7a86ad-acdc-4f6c-8bed-a1aa8c8cb5f4"));
        }
    }
}
