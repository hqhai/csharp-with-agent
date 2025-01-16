using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Training.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE INDEX IX_ClassStudents_IsDeleted_ClassId_WithInclude
                ON [ClassStudents] ([IsDeleted], [ClassId])
                INCLUDE ([CreatedUserId], [UpdatedUserId], [DeletedUserId], [CreatedFullName], [UpdatedFullName], [DeletedFullName], [CreatedDate], [UpdatedDate], [DeletedDate], [StudentId], [IsActive])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_ClassStudents_IsDeleted_ClassId_WithInclude ON [ClassStudents]");
        }
    }
}
