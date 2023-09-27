using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_GameVocabularyTable_And_GameCenterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "GameVocabularies",
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
                    Code = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CefrLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CourseLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitOrder = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AlternateSpellingStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsEquivalent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartSpeech = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Definition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExampleSentence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AudioPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Synonym = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Antonym = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneticTranscription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GameCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WordCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameVocabularies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameVocabularies_GameCenters_GameCenterId",
                        column: x => x.GameCenterId,
                        principalTable: "GameCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameVocabularies_GameTopics_WordCategoryId",
                        column: x => x.WordCategoryId,
                        principalTable: "GameTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GameCenters",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), new DateTime(2023, 9, 27, 17, 26, 23, 146, DateTimeKind.Local).AddTicks(418), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "Planet Defender Parameters", null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_GameVocabularies_GameCenterId",
                table: "GameVocabularies",
                column: "GameCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_GameVocabularies_WordCategoryId",
                table: "GameVocabularies",
                column: "WordCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameVocabularies");

            migrationBuilder.DropTable(
                name: "GameCenters");
        }
    }
}
