using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Permission_LoginAsUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGroupMemberShips_AspNetUsers_UserId",
                table: "UserGroupMemberShips");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroupMemberShips_UserGroups_GroupId",
                table: "UserGroupMemberShips");

            migrationBuilder.DropIndex(
                name: "IX_UserGroupMemberShips_GroupId",
                table: "UserGroupMemberShips");

            migrationBuilder.DropIndex(
                name: "IX_UserGroupMemberShips_IsActive",
                table: "UserGroupMemberShips");

            migrationBuilder.DropIndex(
                name: "IX_UserGroupMemberShips_UserId",
                table: "UserGroupMemberShips");

            migrationBuilder.DropIndex(
                name: "IX_UserGroupMemberShips_UserId_GroupId_IsActive",
                table: "UserGroupMemberShips");

            migrationBuilder.AddColumn<Guid>(
                name: "UserGroupId",
                table: "UserGroupMemberShips",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6124621e-ecf9-4c80-81cc-48d0d16e22d0"),
                column: "Name",
                value: "Login as user");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("20346a71-33f6-490e-a1a3-c7ee757cf84c"), "SchoolStudentManagement.LoginAsUser", new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Login as user", new Guid("618be731-43b3-45ed-bfad-262945ef0d51"), true, null, null, null },
                    { new Guid("fe9af0ba-2ae1-4e4a-bbb9-b6975234afe8"), "SchoolStudentManagement.ViewProgress", new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem tiến độ học sinh", new Guid("618be731-43b3-45ed-bfad-262945ef0d51"), true, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberShips_UserGroupId",
                table: "UserGroupMemberShips",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroupMemberShips_UserGroups_UserGroupId",
                table: "UserGroupMemberShips",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGroupMemberShips_UserGroups_UserGroupId",
                table: "UserGroupMemberShips");

            migrationBuilder.DropIndex(
                name: "IX_UserGroupMemberShips_UserGroupId",
                table: "UserGroupMemberShips");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20346a71-33f6-490e-a1a3-c7ee757cf84c"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fe9af0ba-2ae1-4e4a-bbb9-b6975234afe8"));

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "UserGroupMemberShips");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6124621e-ecf9-4c80-81cc-48d0d16e22d0"),
                column: "Name",
                value: "Log in as user");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberShips_GroupId",
                table: "UserGroupMemberShips",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberShips_IsActive",
                table: "UserGroupMemberShips",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberShips_UserId",
                table: "UserGroupMemberShips",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberShips_UserId_GroupId_IsActive",
                table: "UserGroupMemberShips",
                columns: new[] { "UserId", "GroupId", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroupMemberShips_AspNetUsers_UserId",
                table: "UserGroupMemberShips",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroupMemberShips_UserGroups_GroupId",
                table: "UserGroupMemberShips",
                column: "GroupId",
                principalTable: "UserGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
