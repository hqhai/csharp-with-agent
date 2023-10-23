using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Cms.PlanetDefender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_GameplayTimeConfigTable_And_GameplayRuleConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameplayRuleConfigs",
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
                    StartRoundNumber = table.Column<int>(type: "int", nullable: false),
                    EndRoundNumber = table.Column<int>(type: "int", nullable: false),
                    CurrentUnit = table.Column<int>(type: "int", nullable: false),
                    CurrentUnitOutside = table.Column<int>(type: "int", nullable: false),
                    PreviousUnit = table.Column<int>(type: "int", nullable: false),
                    PreviousUnitOutside = table.Column<int>(type: "int", nullable: false),
                    NumberQuestionPerGame = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameplayRuleConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameplayTimeConfigs",
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
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    GameVocabPDType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Time = table.Column<double>(type: "float", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameplayTimeConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "GameplayRuleConfigs",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "CurrentUnit", "CurrentUnitOutside", "DeletedDate", "DeletedFullName", "DeletedUserId", "EndRoundNumber", "IsDeleted", "NumberQuestionPerGame", "PreviousUnit", "PreviousUnitOutside", "StartRoundNumber", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("a3561911-db7f-4942-a5bb-8a2615e4dca8"), new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin", new Guid("00000000-0000-0000-0000-000000000000"), 5, 5, null, null, null, 10000, false, 20, 5, 5, 10, null, null, null },
                    { new Guid("ad566fae-9d00-48b7-a76c-e8fe84a9125f"), new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin", new Guid("00000000-0000-0000-0000-000000000000"), 2, 4, null, null, null, 9, false, 10, 2, 2, 1, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameplayRuleConfigs");

            migrationBuilder.DropTable(
                name: "GameplayTimeConfigs");
        }
    }
}
