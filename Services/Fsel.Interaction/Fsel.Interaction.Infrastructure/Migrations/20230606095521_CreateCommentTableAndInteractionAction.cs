using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCommentTableAndInteractionAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("adeaeda6-0e41-4ceb-a3fd-1b195317d776"));

            migrationBuilder.AlterColumn<string>(
                name: "AnswerStr",
                table: "SurveyQuestions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CustomerSurveys",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "Comments",
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
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InteractionActions",
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
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionActions", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                columns: new[] { "AnswerStr", "CreatedDate", "DisplayOrder" },
                values: new object[] { "{\"birthday\":null,\"ageGenderQuestions\":[{\"id\":1,\"gender\":\"Male\"},{\"id\":2,\"gender\":\"Female\"},{\"id\":3,\"gender\":\"Other\"}]}", new DateTime(2023, 6, 6, 16, 55, 21, 194, DateTimeKind.Local).AddTicks(7618), 5 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                columns: new[] { "AnswerStr", "CreatedDate", "Type" },
                values: new object[] { "[{\"id\":1,\"content\":\"T\\u00ECm ki\\u1EBFm Google\",\"image\":\"Google\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"Facebook\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"Youtube\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"Tiktok\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/Gia \\u0111\\u00ECnh\",\"image\":\"B\\u1EA1n b\\u00E8/Gia \\u0111\\u00ECnh\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/B\\u00E1o ch\\u00ED/Blog\",\"image\":\"Tin t\\u1EE9c/B\\u00E1o ch\\u00ED/Blog\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"Tivi\"},{\"id\":8,\"content\":\"Kh\\u00E1c....\",\"image\":\"Kh\\u00E1c....\"}]", new DateTime(2023, 6, 6, 16, 55, 21, 194, DateTimeKind.Local).AddTicks(5513), "YourDirection" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                columns: new[] { "AnswerStr", "CreatedDate" },
                values: new object[] { "{\"countryCode\":123,\"countryName\":\"Vi\\u1EC7t Nam\",\"provinceCode\":29,\"provinceName\":\"H\\u00E0 N\\u1ED9i\"}", new DateTime(2023, 6, 6, 16, 55, 21, 194, DateTimeKind.Local).AddTicks(7399) });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                columns: new[] { "AnswerStr", "CreatedDate", "Type" },
                values: new object[] { "[{\"id\":1,\"content\":\"V\\u0103n h\\u00F3a\",\"image\":\"V\\u0103n h\\u00F3a\"},{\"id\":2,\"content\":\"Du l\\u1ECBch\",\"image\":\"Du l\\u1ECBch\"},{\"id\":3,\"content\":\"K\\u1EBFt b\\u1EA1n v\\u00E0 chia s\\u1EBB\",\"image\":\"K\\u1EBFt b\\u1EA1n v\\u00E0 chia s\\u1EBB\"},{\"id\":4,\"content\":\"H\\u1ECDc t\\u1EADp\",\"image\":\"H\\u1ECDc t\\u1EADp\"},{\"id\":5,\"content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"image\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\"},{\"id\":6,\"content\":\"Kh\\u00E1c....\",\"image\":\"Kh\\u00E1c....\"}]", new DateTime(2023, 6, 6, 16, 55, 21, 194, DateTimeKind.Local).AddTicks(7024), "ChooseMultipleColumn" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                columns: new[] { "AnswerStr", "CreatedDate", "Type" },
                values: new object[] { "[{\"id\":1,\"content\":\"B\\u1EA1n \\u0111\\u00E3 bi\\u1EBFt m\\u1ED9t ch\\u00FAt Ti\\u1EBFng Anh\"},{\"id\":2,\"content\":\"\\u0110\\u00E2y l\\u00E0 l\\u1EA7n \\u0111\\u1EA7u b\\u1EA1n h\\u1ECDc Ti\\u1EBFng Anh\"}]", new DateTime(2023, 6, 6, 16, 55, 21, 194, DateTimeKind.Local).AddTicks(6649), "ChooseMultipleColumn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "InteractionActions");

            migrationBuilder.AlterColumn<string>(
                name: "AnswerStr",
                table: "SurveyQuestions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "CustomerSurveys",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                columns: new[] { "AnswerStr", "CreatedDate", "DisplayOrder" },
                values: new object[] { "{\"Birthday\":null,\"AgeGenderQuestions\":[{\"Id\":1,\"Gender\":\"Male\"},{\"Id\":2,\"Gender\":\"Female\"},{\"Id\":3,\"Gender\":\"Other\"}]}", new DateTime(2023, 3, 27, 10, 3, 50, 614, DateTimeKind.Local).AddTicks(8042), 6 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                columns: new[] { "AnswerStr", "CreatedDate", "Type" },
                values: new object[] { "[{\"Id\":1,\"Content\":\"T\\u00ECm ki\\u1EBFm Google\",\"Image\":\"Google\"},{\"Id\":2,\"Content\":\"Facebook\",\"Image\":\"Facebook\"},{\"Id\":3,\"Content\":\"Youtube\",\"Image\":\"Youtube\"},{\"Id\":4,\"Content\":\"Tiktok\",\"Image\":\"Tiktok\"},{\"Id\":5,\"Content\":\"B\\u1EA1n b\\u00E8/Gia \\u0111\\u00ECnh\",\"Image\":\"B\\u1EA1n b\\u00E8/Gia \\u0111\\u00ECnh\"},{\"Id\":6,\"Content\":\"Tin t\\u1EE9c/B\\u00E1o ch\\u00ED/Blog\",\"Image\":\"Tin t\\u1EE9c/B\\u00E1o ch\\u00ED/Blog\"},{\"Id\":7,\"Content\":\"Tivi\",\"Image\":\"Tivi\"},{\"Id\":8,\"Content\":\"Kh\\u00E1c....\",\"Image\":\"Kh\\u00E1c....\"}]", new DateTime(2023, 3, 27, 10, 3, 50, 614, DateTimeKind.Local).AddTicks(6078), "FselSource" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                columns: new[] { "AnswerStr", "CreatedDate" },
                values: new object[] { "null", new DateTime(2023, 3, 27, 10, 3, 50, 614, DateTimeKind.Local).AddTicks(7670) });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                columns: new[] { "AnswerStr", "CreatedDate", "Type" },
                values: new object[] { "[{\"Id\":1,\"Content\":\"V\\u0103n h\\u00F3a\",\"Image\":\"V\\u0103n h\\u00F3a\"},{\"Id\":2,\"Content\":\"Du l\\u1ECBch\",\"Image\":\"Du l\\u1ECBch\"},{\"Id\":3,\"Content\":\"K\\u1EBFt b\\u1EA1n v\\u00E0 chia s\\u1EBB\",\"Image\":\"K\\u1EBFt b\\u1EA1n v\\u00E0 chia s\\u1EBB\"},{\"Id\":4,\"Content\":\"H\\u1ECDc t\\u1EADp\",\"Image\":\"H\\u1ECDc t\\u1EADp\"},{\"Id\":5,\"Content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"Image\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\"},{\"Id\":6,\"Content\":\"Kh\\u00E1c....\",\"Image\":\"Kh\\u00E1c....\"}]", new DateTime(2023, 3, 27, 10, 3, 50, 614, DateTimeKind.Local).AddTicks(7397), "ChooseLanguage" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                columns: new[] { "AnswerStr", "CreatedDate", "Type" },
                values: new object[] { "[{\"Id\":1,\"Content\":\"B\\u1EA1n \\u0111\\u00E3 bi\\u1EBFt m\\u1ED9t ch\\u00FAt Ti\\u1EBFng Anh\"},{\"Id\":2,\"Content\":\"\\u0110\\u00E2y l\\u00E0 l\\u1EA7n \\u0111\\u1EA7u b\\u1EA1n h\\u1ECDc Ti\\u1EBFng Anh\"}]", new DateTime(2023, 3, 27, 10, 3, 50, 614, DateTimeKind.Local).AddTicks(7185), "ChooseDirection" });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayOrder", "Icon", "IsDeleted", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("adeaeda6-0e41-4ceb-a3fd-1b195317d776"), "[{\"Id\":1,\"Content\":\"8 - 10 am\"},{\"Id\":2,\"Content\":\"1 - 3 pm\"},{\"Id\":3,\"Content\":\"3 - 5 pm\"},{\"Id\":4,\"Content\":\"5 - 9 pm\"},{\"Id\":5,\"Content\":\"7 - 10 pm\"}]", new DateTime(2023, 3, 27, 10, 3, 50, 614, DateTimeKind.Local).AddTicks(7843), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 5, "addd", false, "Chọn thời gian học tập ", "StudyTime", null, null, null });
        }
    }
}
