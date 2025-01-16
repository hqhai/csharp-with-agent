using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE INDEX IX_FeatureAccessTimes_IsDeleted_WithInclude
                ON [FeatureAccessTimes] ([IsDeleted])
                INCLUDE ([CreatedUserId], [AccessTime])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_FeatureAccessTimes_IsDeleted_WithInclude ON [FeatureAccessTimes]");
        }
    }
}
