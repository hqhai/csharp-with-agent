using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE INDEX IX_CourseResults_CreatedUserId_WithInclude
                ON [CourseResults] ([CreatedUserId])
                INCLUDE ([Status], [CourseId])");

            migrationBuilder.Sql(@"CREATE INDEX IX_LessonResults_CreatedUserId_WithInclude
                ON [LessonResults] ([CreatedUserId])
                INCLUDE ([UpdatedUserId], [DeletedUserId], [CreatedFullName], [UpdatedFullName], [DeletedFullName], [CreatedDate], [UpdatedDate], [DeletedDate], [IsDeleted], [Percent], [Status], [SummaryNote], [CourseId], [UnitId], [LessonId], [StudentId], [UnitLessonId], [SkillScoresStr], [CorrectCount], [CorrectTotal])");

            migrationBuilder.Sql(@"CREATE INDEX IX_PlacementTestResults_CreatedUserId_WithInclude
                ON [PlacementTestResults] ([CreatedUserId])
                INCLUDE ([UpdatedUserId], [DeletedUserId], [CreatedFullName], [UpdatedFullName], [DeletedFullName], [CreatedDate], [UpdatedDate], [DeletedDate], [IsDeleted], [Percent], [CorrectCount], [CorrectTotal], [Status], [Level], [SkillScoresStr], [StudentId], [CountQuestion], [TotalQuestion], [PlacementTestId])");

            migrationBuilder.Sql(@"CREATE INDEX IX_TimeCodeExercises_IsDeleted_WithInclude
                ON [TimeCodeExercises] ([IsDeleted])
                INCLUDE ([ExerciseId], [VideoTimeCodeId])");

            migrationBuilder.Sql(@"CREATE INDEX IX_UnitResults_CreatedUserId_WithInclude
                ON [UnitResults] ([CreatedUserId])
                INCLUDE ([UpdatedUserId], [DeletedUserId], [CreatedFullName], [UpdatedFullName], [DeletedFullName], [CreatedDate], [UpdatedDate], [DeletedDate], [IsDeleted], [Percent], [Status], [SkillScoresStr], [CorrectCount], [CorrectTotal], [CourseId], [UnitId], [StudentId], [CourseUnitMockTestId], [CompletionDate], [ProcessDate])");

            migrationBuilder.Sql(@"CREATE INDEX IX_VideoTimeCodeAnswers_QuestionId_VideoResultId_WithInclude
                ON [VideoTimeCodeAnswers] ([QuestionId], [VideoResultId])
                INCLUDE ([CorrectCount])");

            migrationBuilder.Sql(@"CREATE INDEX IX_VideoTimeCodeResults_Status_StudentId_WithInclude
                ON [VideoTimeCodeResults] ([Status], [StudentId])
                INCLUDE ([VideoResultId], [VideoTimeCodeId], [CorrectCount], [CorrectTotal], [WorkingTime])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_CourseResults_CreatedUserId_WithInclude ON [CourseResults]");

            migrationBuilder.Sql("DROP INDEX IX_LessonResults_CreatedUserId_WithInclude ON [LessonResults]");

            migrationBuilder.Sql("DROP INDEX IX_PlacementTestResults_CreatedUserId_WithInclude ON [PlacementTestResults]");

            migrationBuilder.Sql("DROP INDEX IX_TimeCodeExercises_IsDeleted_WithInclude ON [TimeCodeExercises]");

            migrationBuilder.Sql("DROP INDEX IX_UnitResults_CreatedUserId_WithInclude ON [UnitResults]");

            migrationBuilder.Sql("DROP INDEX IX_VideoTimeCodeAnswers_QuestionId_VideoResultId_WithInclude ON [VideoTimeCodeAnswers]");

            migrationBuilder.Sql("DROP INDEX IX_VideoTimeCodeResults_Status_StudentId_WithInclude ON [VideoTimeCodeResults]");
        }
    }
}
