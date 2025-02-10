using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_UserTable_AddIndexUserHuman : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Humans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Humans",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Humans_IsDeleted_Email",
                table: "Humans",
                columns: new[] { "IsDeleted", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Humans_IsDeleted_PhoneNumber",
                table: "Humans",
                columns: new[] { "IsDeleted", "PhoneNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsDeleted_Id_UserName_Email",
                table: "AspNetUsers",
                columns: new[] { "IsDeleted", "Id", "UserName", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsDeleted_PhoneNumber",
                table: "AspNetUsers",
                columns: new[] { "IsDeleted", "PhoneNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsDeleted_UserName",
                table: "AspNetUsers",
                columns: new[] { "IsDeleted", "UserName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Humans_IsDeleted_Email",
                table: "Humans");

            migrationBuilder.DropIndex(
                name: "IX_Humans_IsDeleted_PhoneNumber",
                table: "Humans");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsDeleted_Id_UserName_Email",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsDeleted_PhoneNumber",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsDeleted_UserName",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Humans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Humans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(70)",
                oldMaxLength: 70,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
