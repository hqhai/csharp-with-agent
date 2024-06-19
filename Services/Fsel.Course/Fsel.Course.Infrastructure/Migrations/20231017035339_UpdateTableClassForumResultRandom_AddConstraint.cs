using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableClassForumResultRandom_AddConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ClassId",
                table: "ClassForumResultRandoms",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResultRandoms_ClassForumId",
                table: "ClassForumResultRandoms",
                column: "ClassForumId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResultRandoms_ClassForumResultId",
                table: "ClassForumResultRandoms",
                column: "ClassForumResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForumResultRandoms_ClassForumResults_ClassForumResultId",
                table: "ClassForumResultRandoms",
                column: "ClassForumResultId",
                principalTable: "ClassForumResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForumResultRandoms_ClassForums_ClassForumId",
                table: "ClassForumResultRandoms",
                column: "ClassForumId",
                principalTable: "ClassForums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForumResultRandoms_ClassForumResults_ClassForumResultId",
                table: "ClassForumResultRandoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassForumResultRandoms_ClassForums_ClassForumId",
                table: "ClassForumResultRandoms");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResultRandoms_ClassForumId",
                table: "ClassForumResultRandoms");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResultRandoms_ClassForumResultId",
                table: "ClassForumResultRandoms");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassId",
                table: "ClassForumResultRandoms",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
