using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_DailyQuizHistoryTable_Add_Field_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "DailyQuizHistories",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "DailyQuizHistories");
        }
    }
}
