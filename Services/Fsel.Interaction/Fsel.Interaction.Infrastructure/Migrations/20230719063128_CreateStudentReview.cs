using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateStudentReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentReviews",
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
                    ReviewType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentReviewDetails",
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
                    ReviewQuestionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoteStars = table.Column<double>(type: "float", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StudentReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentReviewDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentReviewDetails_StudentReviews_StudentReviewId",
                        column: x => x.StudentReviewId,
                        principalTable: "StudentReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 19, 13, 31, 28, 662, DateTimeKind.Local).AddTicks(4260));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 19, 13, 31, 28, 662, DateTimeKind.Local).AddTicks(2579));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 19, 13, 31, 28, 662, DateTimeKind.Local).AddTicks(4109));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 19, 13, 31, 28, 662, DateTimeKind.Local).AddTicks(3849));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 19, 13, 31, 28, 662, DateTimeKind.Local).AddTicks(3505));

            migrationBuilder.CreateIndex(
                name: "IX_StudentReviewDetails_StudentReviewId",
                table: "StudentReviewDetails",
                column: "StudentReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentReviewDetails");

            migrationBuilder.DropTable(
                name: "StudentReviews");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 14, 10, 27, 22, 717, DateTimeKind.Local).AddTicks(5809));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 14, 10, 27, 22, 717, DateTimeKind.Local).AddTicks(3673));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 14, 10, 27, 22, 717, DateTimeKind.Local).AddTicks(5638));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 14, 10, 27, 22, 717, DateTimeKind.Local).AddTicks(5380));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 14, 10, 27, 22, 717, DateTimeKind.Local).AddTicks(4957));
        }
    }
}
