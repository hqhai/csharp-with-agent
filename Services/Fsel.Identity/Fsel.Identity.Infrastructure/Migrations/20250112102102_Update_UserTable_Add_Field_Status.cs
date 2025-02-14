using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_UserTable_Add_Field_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE AspNetUsers set Status = 'Active'"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspNetUsers");
        }
    }
}
