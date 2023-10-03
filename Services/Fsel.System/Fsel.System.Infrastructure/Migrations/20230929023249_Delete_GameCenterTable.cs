using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Delete_GameCenterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameVocabularies_GameCenters_GameCenterId",
                table: "GameVocabularies");

            migrationBuilder.DropTable(
                name: "GameCenters");

            migrationBuilder.DropIndex(
                name: "IX_GameVocabularies_GameCenterId",
                table: "GameVocabularies");

            migrationBuilder.RenameColumn(
                name: "GameCenterId",
                table: "GameVocabularies",
                newName: "PlatformId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlatformId",
                table: "GameVocabularies",
                newName: "GameCenterId");

            migrationBuilder.CreateTable(
                name: "GameCenters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCenters", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "GameCenters",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), new DateTime(2023, 9, 27, 20, 28, 59, 971, DateTimeKind.Local).AddTicks(8393), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "Planet Defender Parameters", null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_GameVocabularies_GameCenterId",
                table: "GameVocabularies",
                column: "GameCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameVocabularies_GameCenters_GameCenterId",
                table: "GameVocabularies",
                column: "GameCenterId",
                principalTable: "GameCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
