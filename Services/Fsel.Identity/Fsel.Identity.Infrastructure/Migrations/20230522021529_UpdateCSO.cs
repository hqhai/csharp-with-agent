using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCSO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubscriptionClassesStr",
                table: "CSOs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionClassesStr",
                table: "CSOs");
        }
    }
}
