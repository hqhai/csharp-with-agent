using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacherAndCSO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleLivesStr",
                table: "Teachers");

            migrationBuilder.RenameColumn(
                name: "SubscriptionClassesStr",
                table: "CSOs",
                newName: "PackageIdsStr");

            migrationBuilder.RenameColumn(
                name: "RoleLivesStr",
                table: "CSOs",
                newName: "CourseTypesStr");

            migrationBuilder.AlterColumn<string>(
                name: "CourseTypesStr",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "LiveCourseTypesStr",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LiveCourseTypesStr",
                table: "Teachers");

            migrationBuilder.RenameColumn(
                name: "PackageIdsStr",
                table: "CSOs",
                newName: "SubscriptionClassesStr");

            migrationBuilder.RenameColumn(
                name: "CourseTypesStr",
                table: "CSOs",
                newName: "RoleLivesStr");

            migrationBuilder.AlterColumn<string>(
                name: "CourseTypesStr",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleLivesStr",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
