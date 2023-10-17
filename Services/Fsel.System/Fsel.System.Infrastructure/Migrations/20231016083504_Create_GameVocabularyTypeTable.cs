using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_GameVocabularyTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternateSpellingStr",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "Antonym",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "AudioPath",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "Definition",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "ExampleSentence",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "Hint",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "PhoneticTranscription",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "Synonym",
                table: "GameVocabularies");

            migrationBuilder.DropColumn(
                name: "UsEquivalent",
                table: "GameVocabularies");

            migrationBuilder.CreateTable(
                name: "GameVocabularyTypes",
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
                    GameVocabType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuestionContent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GameVocabularyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameVocabularyTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameVocabularyTypes_GameVocabularies_GameVocabularyId",
                        column: x => x.GameVocabularyId,
                        principalTable: "GameVocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameVocabularyTypes_GameVocabularyId",
                table: "GameVocabularyTypes",
                column: "GameVocabularyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameVocabularyTypes");

            migrationBuilder.AddColumn<string>(
                name: "AlternateSpellingStr",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Antonym",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioPath",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Definition",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExampleSentence",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hint",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneticTranscription",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Synonym",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsEquivalent",
                table: "GameVocabularies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
