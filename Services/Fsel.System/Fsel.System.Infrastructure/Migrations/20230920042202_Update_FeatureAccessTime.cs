using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_FeatureAccessTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "FeatureAccessTimes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LessonId",
                table: "FeatureAccessTimes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "FeatureAccessTimes",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "FeatureAccessTimes");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "FeatureAccessTimes");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "FeatureAccessTimes");
        }
    }
}
