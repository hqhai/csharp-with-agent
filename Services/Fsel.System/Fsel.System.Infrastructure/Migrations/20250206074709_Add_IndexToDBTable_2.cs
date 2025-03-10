using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FeatureAccessTimes_IsDeleted",
                table: "FeatureAccessTimes",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "CreatedUserId", "AccessTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeatureAccessTimes_IsDeleted",
                table: "FeatureAccessTimes");
        }
    }
}
