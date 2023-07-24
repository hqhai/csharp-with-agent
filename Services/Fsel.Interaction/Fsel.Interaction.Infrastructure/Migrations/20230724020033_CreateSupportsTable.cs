using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSupportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentReviewDetails_StudentReviews_StudentReviewId",
                table: "StudentReviewDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentReviewDetails",
                table: "StudentReviewDetails");

            migrationBuilder.RenameTable(
                name: "StudentReviewDetails",
                newName: "StudentReviewDetail");

            migrationBuilder.RenameIndex(
                name: "IX_StudentReviewDetails_StudentReviewId",
                table: "StudentReviewDetail",
                newName: "IX_StudentReviewDetail_StudentReviewId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentReviewDetail",
                table: "StudentReviewDetail",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SupportCategorys",
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
                    Title = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IconPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportCategorys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportQuestions",
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
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IsFrequent = table.Column<bool>(type: "bit", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SupportCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportQuestions_SupportCategorys_SupportCategoryId",
                        column: x => x.SupportCategoryId,
                        principalTable: "SupportCategorys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
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
                    Code = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UserCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FilePathsStr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OtherProblem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SupportCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupportQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportCategorys_SupportCategoryId",
                        column: x => x.SupportCategoryId,
                        principalTable: "SupportCategorys",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportQuestions_SupportQuestionId",
                        column: x => x.SupportQuestionId,
                        principalTable: "SupportQuestions",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_SupportQuestions_SupportCategoryId",
                table: "SupportQuestions",
                column: "SupportCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_SupportCategoryId",
                table: "SupportTickets",
                column: "SupportCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_SupportQuestionId",
                table: "SupportTickets",
                column: "SupportQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentReviewDetail_StudentReviews_StudentReviewId",
                table: "StudentReviewDetail",
                column: "StudentReviewId",
                principalTable: "StudentReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentReviewDetail_StudentReviews_StudentReviewId",
                table: "StudentReviewDetail");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "SupportQuestions");

            migrationBuilder.DropTable(
                name: "SupportCategorys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentReviewDetail",
                table: "StudentReviewDetail");

            migrationBuilder.RenameTable(
                name: "StudentReviewDetail",
                newName: "StudentReviewDetails");

            migrationBuilder.RenameIndex(
                name: "IX_StudentReviewDetail_StudentReviewId",
                table: "StudentReviewDetails",
                newName: "IX_StudentReviewDetails_StudentReviewId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentReviewDetails",
                table: "StudentReviewDetails",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_StudentReviewDetails_StudentReviews_StudentReviewId",
                table: "StudentReviewDetails",
                column: "StudentReviewId",
                principalTable: "StudentReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
