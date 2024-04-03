using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClassForumAttemp2Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetryContent",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "RetryGradingAlFeedBack",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "RetryWordContent",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "IsRetry",
                table: "ClassForumResultFiles");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ClassForumResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassForumResultId",
                table: "ClassForumResultFiles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassForumDetailResultId",
                table: "ClassForumResultFiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ClassForumDetailResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WordContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WordCount = table.Column<int>(type: "int", nullable: true),
                    GradingAlFeedback = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClassForumResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassForumDetailResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassForumDetailResults_ClassForumResults_ClassForumResultId",
                        column: x => x.ClassForumResultId,
                        principalTable: "ClassForumResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResultFiles_ClassForumDetailResultId",
                table: "ClassForumResultFiles",
                column: "ClassForumDetailResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId",
                table: "ClassForumDetailResults",
                column: "ClassForumResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForumResultFiles_ClassForumDetailResults_ClassForumDetailResultId",
                table: "ClassForumResultFiles",
                column: "ClassForumDetailResultId",
                principalTable: "ClassForumDetailResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForumResultFiles_ClassForumDetailResults_ClassForumDetailResultId",
                table: "ClassForumResultFiles");

            migrationBuilder.DropTable(
                name: "ClassForumDetailResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResultFiles_ClassForumDetailResultId",
                table: "ClassForumResultFiles");

            migrationBuilder.DropColumn(
                name: "ClassForumDetailResultId",
                table: "ClassForumResultFiles");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ClassForumResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetryContent",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetryGradingAlFeedBack",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetryWordContent",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassForumResultId",
                table: "ClassForumResultFiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRetry",
                table: "ClassForumResultFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
