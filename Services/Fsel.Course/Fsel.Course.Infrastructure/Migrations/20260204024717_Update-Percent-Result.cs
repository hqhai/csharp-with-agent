using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePercentResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "VideoTimeCodeResults",
                type: "float",
                nullable: false,
                computedColumnSql: "CASE WHEN ([CorrectTotal] + [CorrectTotalUngraded]) > 0 THEN ROUND(([CorrectCount] + [CorrectCountUngraded] * 100.0) / ([CorrectTotal] + [CorrectTotalUngraded]), 0) ELSE 0 END",
                stored: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "VideoResults",
                type: "float",
                nullable: false,
                computedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END",
                stored: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "TestSectionResult",
                type: "float",
                nullable: false,
                computedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END",
                stored: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "TestResult",
                type: "float",
                nullable: false,
                computedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END",
                stored: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "HomeWorkResults",
                type: "float",
                nullable: false,
                computedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END",
                stored: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "HomeWorkExtraPracticeResults",
                type: "float",
                nullable: false,
                computedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END",
                stored: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "ClassForumResults",
                type: "float",
                nullable: false,
                computedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END",
                stored: true,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "VideoTimeCodeResults",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComputedColumnSql: "CASE WHEN ([CorrectTotal] + [CorrectTotalUngraded]) > 0 THEN ROUND(([CorrectCount] + [CorrectCountUngraded] * 100.0) / ([CorrectTotal] + [CorrectTotalUngraded]), 0) ELSE 0 END");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "VideoResults",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComputedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "TestSectionResult",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComputedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "TestResult",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComputedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "HomeWorkResults",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComputedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "HomeWorkExtraPracticeResults",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComputedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END");

            migrationBuilder.AlterColumn<double>(
                name: "Percent",
                table: "ClassForumResults",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldComputedColumnSql: "CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END");
        }
    }
}
