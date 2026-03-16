using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations.PostgreMigrations
{
    /// <inheritdoc />
    public partial class Create_DictionaryAITable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable pgvector extension
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

            migrationBuilder.CreateTable(
                name: "DictionaryAIs",
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
                    HighlightedItemSource = table.Column<string>(type: "text", nullable: true),
                    HighlightedItemTarget = table.Column<string>(type: "text", nullable: true),
                    DefinitionSource = table.Column<string>(type: "text", nullable: true),
                    DefinitionTarget = table.Column<string>(type: "text", nullable: true),
                    JsonPayload = table.Column<string>(type: "text", nullable: true),
                    NotesJson = table.Column<string>(type: "text", nullable: true),
                    ExampleSentenceSource = table.Column<string>(type: "text", nullable: true),
                    ExampleSentenceTarget = table.Column<string>(type: "text", nullable: true),
                    SourceLanguage = table.Column<string>(type: "text", nullable: true),
                    TargetLanguage = table.Column<string>(type: "text", nullable: true),
                    HasAudioFromLegacy = table.Column<bool>(type: "boolean", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: true),
                    InputLength = table.Column<int>(type: "integer", nullable: false),
                    Embedding = table.Column<List<float>>(type: "vector(1536)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryAIs", x => x.Id);
                });

            // Create IVFFlat index for vector search
            migrationBuilder.Sql(@"
                CREATE INDEX idx_dictionaryai_embedding
                ON ""DictionaryAIs""
                USING ivfflat (""Embedding"" vector_cosine_ops)
                WITH (lists = 100);
            ");

            // Index for language filter
            migrationBuilder.Sql(@"
                CREATE INDEX idx_dictionaryai_language
                ON ""DictionaryAIs"" (""SourceLanguage"", ""TargetLanguage"")
                WHERE ""IsDeleted"" = false;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_dictionaryai_embedding;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_dictionaryai_language;");

            migrationBuilder.DropTable(
                name: "DictionaryAIs");

            migrationBuilder.Sql("DROP EXTENSION IF EXISTS vector;");
        }
    }
}
