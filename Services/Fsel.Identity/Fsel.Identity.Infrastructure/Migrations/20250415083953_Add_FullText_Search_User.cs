using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_FullText_Search_User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE FULLTEXT CATALOG ftCatalog_AspNetUsers AS DEFAULT;", suppressTransaction: true);
            migrationBuilder.Sql(@"
                CREATE FULLTEXT INDEX ON AspNetUsers(FullName)
                KEY INDEX PK_AspNetUsers
                ON ftCatalog_AspNetUsers;
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FULLTEXT INDEX ON AspNetUsers;", suppressTransaction: true);
            migrationBuilder.Sql("DROP FULLTEXT CATALOG ftCatalog_AspNetUsers;", suppressTransaction: true);
        }
    }
}
