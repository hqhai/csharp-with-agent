using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_IsDeleted_UserId",
                table: "NotificationMessages",
                columns: new[] { "IsDeleted", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_IsDeleted_UserId_Status",
                table: "NotificationMessages",
                columns: new[] { "IsDeleted", "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_IsDeleted_UserId",
                table: "NotificationMessages");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_IsDeleted_UserId_Status",
                table: "NotificationMessages");
        }
    }
}
