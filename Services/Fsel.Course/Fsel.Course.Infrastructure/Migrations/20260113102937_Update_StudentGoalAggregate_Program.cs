using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_StudentGoalAggregate_Program : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseLevel",
                table: "StudentGoalAggregates");

            migrationBuilder.DropColumn(
                name: "CourseType",
                table: "StudentGoalAggregates");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseResultId",
                table: "StudentGoalAggregates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "StudentGoalAggregates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGoalAggregates_LevelId",
                table: "StudentGoalAggregates",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGoalAggregates_ProgramId",
                table: "StudentGoalAggregates",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGoalAggregates_Categorys_ProgramId",
                table: "StudentGoalAggregates",
                column: "ProgramId",
                principalTable: "Categorys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGoalAggregates_Levels_LevelId",
                table: "StudentGoalAggregates",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGoalAggregates_Categorys_ProgramId",
                table: "StudentGoalAggregates");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGoalAggregates_Levels_LevelId",
                table: "StudentGoalAggregates");

            migrationBuilder.DropIndex(
                name: "IX_StudentGoalAggregates_LevelId",
                table: "StudentGoalAggregates");

            migrationBuilder.DropIndex(
                name: "IX_StudentGoalAggregates_ProgramId",
                table: "StudentGoalAggregates");

            migrationBuilder.DropColumn(
                name: "CourseResultId",
                table: "StudentGoalAggregates");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "StudentGoalAggregates");

            migrationBuilder.AddColumn<string>(
                name: "CourseLevel",
                table: "StudentGoalAggregates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CourseType",
                table: "StudentGoalAggregates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
