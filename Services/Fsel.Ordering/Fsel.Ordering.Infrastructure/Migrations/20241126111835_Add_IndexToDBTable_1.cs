using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderTransactions_CreatedUserId_IsDeleted_Status_Type",
                table: "OrderTransactions",
                columns: new[] { "CreatedUserId", "IsDeleted", "Status", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IsDeleted_Status_UserId_IsTrial",
                table: "Orders",
                columns: new[] { "IsDeleted", "Status", "UserId", "IsTrial" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IsDeleted_UserId",
                table: "Orders",
                columns: new[] { "IsDeleted", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderTransactions_CreatedUserId_IsDeleted_Status_Type",
                table: "OrderTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IsDeleted_Status_UserId_IsTrial",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IsDeleted_UserId",
                table: "Orders");
        }
    }
}
