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
            migrationBuilder.CreateIndex(
                name: "IX_ClassStudents_IsDeleted_ClassId",
                table: "ClassStudents",
                columns: new[] { "IsDeleted", "ClassId" })
                .Annotation("SqlServer:Include", new[] { "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsActive", "StudentId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassStudents_IsDeleted_ClassId",
                table: "ClassStudents");
        }
    }
}
