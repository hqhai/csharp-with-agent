using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations.PostgreMigrations
{
    /// <inheritdoc />
    public partial class Add_DictionarySearchHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DictionarySearchHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedFullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchTerm = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SearchContext = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DictionaryAIId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceLanguage = table.Column<string>(type: "text", nullable: true),
                    TargetLanguage = table.Column<string>(type: "text", nullable: true),
                    FoundResult = table.Column<bool>(type: "boolean", nullable: false),
                    ResultCount = table.Column<int>(type: "integer", nullable: false),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionarySearchHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DictionarySearchHistory_CreatedDate",
                table: "DictionarySearchHistories",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DictionarySearchHistory_UserId",
                table: "DictionarySearchHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DictionarySearchHistory_UserId_CreatedDate",
                table: "DictionarySearchHistories",
                columns: new[] { "UserId", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DictionarySearchHistories");
        }
    }
}
