using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Sender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_MessageHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Template",
                table: "MessageHistories",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Template",
                table: "MessageHistories");
        }
    }
}
