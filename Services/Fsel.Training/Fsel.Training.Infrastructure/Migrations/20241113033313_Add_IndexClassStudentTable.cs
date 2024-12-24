using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Training.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexClassStudentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ClassStudents_IsDeleted_ClassId_StudentId",
                table: "ClassStudents",
                columns: new[] { "IsDeleted", "ClassId", "StudentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassStudents_IsDeleted_ClassId_StudentId",
                table: "ClassStudents");
        }
    }
}
