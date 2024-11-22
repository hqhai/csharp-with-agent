using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexTable_UserAndHuman : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "UserSequence",
                startValue: 100000L);

            migrationBuilder.AlterColumn<string>(
                name: "OTPCode",
                table: "UserOtpCodes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserOtpCodes_Status",
                table: "UserOtpCodes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserOtpCodes_UserId_OTPCode",
                table: "UserOtpCodes",
                columns: new[] { "UserId", "OTPCode" });

            migrationBuilder.CreateIndex(
                name: "IX_UserOtpCodes_UserId_Status",
                table: "UserOtpCodes",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Humans_IsDeleted_Code",
                table: "Humans",
                columns: new[] { "IsDeleted", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsDeleted_Email",
                table: "AspNetUsers",
                columns: new[] { "IsDeleted", "Email" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserOtpCodes_Status",
                table: "UserOtpCodes");

            migrationBuilder.DropIndex(
                name: "IX_UserOtpCodes_UserId_OTPCode",
                table: "UserOtpCodes");

            migrationBuilder.DropIndex(
                name: "IX_UserOtpCodes_UserId_Status",
                table: "UserOtpCodes");

            migrationBuilder.DropIndex(
                name: "IX_Humans_IsDeleted_Code",
                table: "Humans");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsDeleted_Email",
                table: "AspNetUsers");

            migrationBuilder.DropSequence(
                name: "UserSequence");

            migrationBuilder.AlterColumn<string>(
                name: "OTPCode",
                table: "UserOtpCodes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
