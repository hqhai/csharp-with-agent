using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_StudentDailyStreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("856818e6-9e20-43d8-964f-5dab2ba1a355"));

            migrationBuilder.DeleteData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"));

            migrationBuilder.DeleteData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("f144a094-3f49-4ca8-8f1e-7234289dd1a5"));

            migrationBuilder.RenameColumn(
                name: "IsReceiveGift",
                table: "StudentDailyStreak",
                newName: "IsGiftReceive");

            migrationBuilder.AddColumn<bool>(
                name: "IsArmorialReceive",
                table: "StudentDailyStreak",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArmorialReceive",
                table: "StudentDailyStreak");

            migrationBuilder.RenameColumn(
                name: "IsGiftReceive",
                table: "StudentDailyStreak",
                newName: "IsReceiveGift");

            migrationBuilder.InsertData(
                table: "Platform",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("856818e6-9e20-43d8-964f-5dab2ba1a355"), "LMS", new DateTime(2023, 9, 28, 19, 58, 15, 689, DateTimeKind.Local).AddTicks(153), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, "Learning Management System", "Learn", null, null, null },
                    { new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), "LCMS", new DateTime(2023, 9, 28, 19, 58, 15, 688, DateTimeKind.Local).AddTicks(9958), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, "Learning Content Management System", "Content", null, null, null },
                    { new Guid("f144a094-3f49-4ca8-8f1e-7234289dd1a5"), "PlanetDefender", new DateTime(2023, 9, 28, 19, 58, 15, 689, DateTimeKind.Local).AddTicks(190), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, "Planet Defender", "Game", null, null, null }
                });
        }
    }
}
