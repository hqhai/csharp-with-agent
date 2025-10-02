using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.ExamPractice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_ExamPracticeScore_Enum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Criteria",
                table: "ExamPracticeScores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"
            UPDATE dbo.ExamPracticeScores
            SET Criteria = CASE Criteria
                WHEN 0 THEN 'FluencyAndCoherence'
                WHEN 1 THEN 'LexicalResource'
                WHEN 2 THEN 'GrammaticalRangeAndAccuracy'
                WHEN 3 THEN 'Pronunciation'
                WHEN 4 THEN 'DiscourseManagement'
                WHEN 5 THEN 'Vocabulary'
                ELSE Criteria   -- nếu có dữ liệu ngoài dải, tạm giữ nguyên (có thể đổi thành giá trị mặc định nếu muốn)
            END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Criteria",
                table: "ExamPracticeScores",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.Sql(@"
            UPDATE dbo.ExamPracticeScores
            SET Criteria = CASE
                WHEN Criteria = N'FluencyAndCoherence'          THEN N'0'
                WHEN Criteria = N'LexicalResource'              THEN N'1'
                WHEN Criteria = N'GrammaticalRangeAndAccuracy'  THEN N'2'
                WHEN Criteria = N'Pronunciation'                THEN N'3'
                WHEN Criteria = N'DiscourseManagement'          THEN N'4'
                WHEN Criteria = N'Vocabulary'                   THEN N'5'
                ELSE Criteria
            END;");
        }
    }
}
