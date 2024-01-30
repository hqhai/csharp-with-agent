using System;
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

            migrationBuilder.AddColumn<Guid>(
                name: "StudentGameInfoId",
                table: "StudentTagNames",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "StudentTagNames",
                keyColumn: "Id",
                keyValue: new Guid("402dec4e-39c9-4235-8c7d-b18eb2092305"),
                column: "StudentGameInfoId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "StudentTagNames",
                keyColumn: "Id",
                keyValue: new Guid("88bf6e6d-d9d6-4d52-bf9d-52f252f16404"),
                column: "StudentGameInfoId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "StudentTagNames",
                keyColumn: "Id",
                keyValue: new Guid("9f860757-1a3e-4a05-aaf5-2dc1797e23bf"),
                column: "StudentGameInfoId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "StudentTagNames",
                keyColumn: "Id",
                keyValue: new Guid("b38637d8-129d-4d50-bdf5-a3304684740d"),
                column: "StudentGameInfoId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "StudentTagNames",
                keyColumn: "Id",
                keyValue: new Guid("e2ef7a2e-bc23-4103-8e2b-23cfdca5264e"),
                column: "StudentGameInfoId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "StudentTagNames",
                keyColumn: "Id",
                keyValue: new Guid("edc52c09-c407-4371-9201-8f09351c1f58"),
                column: "StudentGameInfoId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StudentTagNames_StudentGameInfoId",
                table: "StudentTagNames",
                column: "StudentGameInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGameInfos_TagNameId",
                table: "StudentGameInfos",
                column: "TagNameId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTagNames_StudentGameInfos_StudentGameInfoId",
                table: "StudentTagNames",
                column: "StudentGameInfoId",
                principalTable: "StudentGameInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentTagNames_StudentGameInfos_StudentGameInfoId",
                table: "StudentTagNames");

            migrationBuilder.DropIndex(
                name: "IX_StudentTagNames_StudentGameInfoId",
                table: "StudentTagNames");

            migrationBuilder.DropIndex(
                name: "IX_StudentGameInfos_TagNameId",
                table: "StudentGameInfos");

            migrationBuilder.DropColumn(
                name: "StudentGameInfoId",
                table: "StudentTagNames");

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
