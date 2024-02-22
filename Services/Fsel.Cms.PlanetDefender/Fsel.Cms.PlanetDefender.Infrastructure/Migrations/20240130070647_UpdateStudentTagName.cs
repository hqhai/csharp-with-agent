using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Cms.PlanetDefender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentTagName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGameInfos_StudentTagNames_StudentTagNameId",
                table: "StudentGameInfos");

            migrationBuilder.DropIndex(
                name: "IX_StudentGameInfos_StudentTagNameId",
                table: "StudentGameInfos");

            migrationBuilder.DropColumn(
                name: "StudentTagNameId",
                table: "StudentGameInfos");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGameInfos_TagNameId",
                table: "StudentGameInfos",
                column: "TagNameId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGameInfos_StudentTagNames_TagNameId",
                table: "StudentGameInfos",
                column: "TagNameId",
                principalTable: "StudentTagNames",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGameInfos_StudentTagNames_TagNameId",
                table: "StudentGameInfos");

            migrationBuilder.DropIndex(
                name: "IX_StudentGameInfos_TagNameId",
                table: "StudentGameInfos");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentTagNameId",
                table: "StudentGameInfos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGameInfos_StudentTagNameId",
                table: "StudentGameInfos",
                column: "StudentTagNameId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGameInfos_StudentTagNames_StudentTagNameId",
                table: "StudentGameInfos",
                column: "StudentTagNameId",
                principalTable: "StudentTagNames",
                principalColumn: "Id");
        }
    }
}
