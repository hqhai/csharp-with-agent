using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations.PostgreMigrations
{
    /// <inheritdoc />
    public partial class ChangeEmbeddingToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the IVFFlat index first (incompatible with text column)
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_dictionaryai_embedding;");

            // Convert vector column to text
            migrationBuilder.Sql("ALTER TABLE \"DictionaryAIs\" ALTER COLUMN \"Embedding\" TYPE text;");

            // Note: GIN/HNSW indexes don't work with CAST from text to vector
            // Vector search will still work but slower (full table scan)
            // To use indexes, keep column as vector(1536) type
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert text back to vector
            migrationBuilder.Sql("ALTER TABLE \"DictionaryAIs\" ALTER COLUMN \"Embedding\" TYPE vector(1536);");

            // Recreate IVFFlat index
            migrationBuilder.Sql(@"
                CREATE INDEX idx_dictionaryai_embedding
                ON ""DictionaryAIs""
                USING ivfflat (""Embedding"" vector_cosine_ops)
                WITH (lists = 100);
            ");
        }
    }
}
