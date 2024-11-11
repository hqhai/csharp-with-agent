using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexOTPCodeAndStatus_To_UserOtpTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserOtpCodes_UserId_OTPCode",
                table: "UserOtpCodes",
                columns: new[] { "UserId", "OTPCode" });

            migrationBuilder.CreateIndex(
                name: "IX_UserOtpCodes_UserId_Status",
                table: "UserOtpCodes",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserOtpCodes_UserId_OTPCode",
                table: "UserOtpCodes");

            migrationBuilder.DropIndex(
                name: "IX_UserOtpCodes_UserId_Status",
                table: "UserOtpCodes");
        }
    }
}
