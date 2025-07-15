using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropUniqueIndexOnTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX [IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount] ON [dbo].[ClassForumDetailResults];");
            migrationBuilder.Sql("DROP INDEX [IX_ClassForumResults_LessonResultId_ClassForumId_StudentId] ON [dbo].[ClassForumResults];");
            migrationBuilder.Sql("DROP INDEX [IX_CourseResults_CourseId_StudentId] ON [dbo].[CourseResults];");
            migrationBuilder.Sql("DROP INDEX [IX_Courses_ParentCourseId_Priority] ON [dbo].[Courses];");
            migrationBuilder.Sql("DROP INDEX [IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId] ON [dbo].[FinalTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_FinalTestResults_CourseId_FinalTestId_StudentId] ON [dbo].[FinalTestResults];");
            migrationBuilder.Sql("DROP INDEX [IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId] ON [dbo].[HomeWorkAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId] ON [dbo].[HomeWorkResults];");
            migrationBuilder.Sql("DROP INDEX [IX_LessonResults_CourseId_UnitId_LessonId_StudentId] ON [dbo].[LessonResults];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId] ON [dbo].[MockTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[MockTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId] ON [dbo].[MockTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestResults_CourseId_MockTestId_StudentId] ON [dbo].[MockTestResults];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId] ON [dbo].[MockTestResults];");
            migrationBuilder.Sql("DROP INDEX [IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[PlacementTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_PlacementTestGroupResults_StudentId] ON [dbo].[PlacementTestGroupResults];");
            migrationBuilder.Sql("DROP INDEX [IX_PlacementTestResults_PlacementTestId_StudentId] ON [dbo].[PlacementTestResults];");
            migrationBuilder.Sql("DROP INDEX [IX_QuestionShuffles_StudentId_QuestionId] ON [dbo].[QuestionShuffles];");
            migrationBuilder.Sql("DROP INDEX [IX_SectionGroupResults_SectionGroupId_FinalTestResultId] ON [dbo].[SectionGroupResults];");
            migrationBuilder.Sql("DROP INDEX [IX_SectionGroupResults_SectionGroupId_MockTestResultId] ON [dbo].[SectionGroupResults];");
            migrationBuilder.Sql("DROP INDEX [IX_SectionGroupResults_SectionGroupId_PlacementTestResultId] ON [dbo].[SectionGroupResults];");
            migrationBuilder.Sql("DROP INDEX [IX_UnitResults_CourseId_UnitId_StudentId] ON [dbo].[UnitResults];");
            migrationBuilder.Sql("DROP INDEX [IX_VideoResults_LessonResultId_VideoId_StudentId] ON [dbo].[VideoResults];");
            migrationBuilder.Sql("DROP INDEX [IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId] ON [dbo].[VideoTimeCodeAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId] ON [dbo].[VideoTimeCodeResults];");

            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount] ON [dbo].[ClassForumDetailResults] ([ClassForumResultId], [SubmissionCount]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_ClassForumResults_LessonResultId_ClassForumId_StudentId] ON [dbo].[ClassForumResults] ([LessonResultId], [ClassForumId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_CourseResults_CourseId_StudentId] ON [dbo].[CourseResults] ([CourseId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_Courses_ParentCourseId_Priority] ON [dbo].[Courses] ([ParentCourseId], [Priority]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId] ON [dbo].[FinalTestAnswers] ([FinalTestResultId], [SectionQuestionId], [SectionGroupResultId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_FinalTestResults_CourseId_FinalTestId_StudentId] ON [dbo].[FinalTestResults] ([CourseId], [FinalTestId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId] ON [dbo].[HomeWorkAnswers] ([HomeWorkQuestionId], [HomeWorkResultId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId] ON [dbo].[HomeWorkResults] ([LessonResultId], [HomeWorkId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_LessonResults_CourseId_UnitId_LessonId_StudentId] ON [dbo].[LessonResults] ([CourseId], [UnitId], [LessonId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId] ON [dbo].[MockTestAnswers] ([MockTestResultId], [SectionGroupResultId], [SectionId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[MockTestAnswers] ([MockTestResultId], [SectionGroupResultId], [SectionQuestionId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId] ON [dbo].[MockTestAnswers] ([MockTestResultId], [SectionGroupResultId], [SectionTimeCodeId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_MockTestResults_CourseId_MockTestId_StudentId] ON [dbo].[MockTestResults] ([CourseId], [MockTestId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId] ON [dbo].[MockTestResults] ([CourseId], [MockTestId], [UnitId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[PlacementTestAnswers] ([PlacementTestResultId], [SectionGroupResultId], [SectionQuestionId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_PlacementTestGroupResults_StudentId] ON [dbo].[PlacementTestGroupResults] ([StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_PlacementTestResults_PlacementTestId_StudentId] ON [dbo].[PlacementTestResults] ([PlacementTestId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_QuestionShuffles_StudentId_QuestionId] ON [dbo].[QuestionShuffles] ([StudentId], [QuestionId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_FinalTestResultId] ON [dbo].[SectionGroupResults] ([SectionGroupId], [FinalTestResultId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_MockTestResultId] ON [dbo].[SectionGroupResults] ([SectionGroupId], [MockTestResultId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_PlacementTestResultId] ON [dbo].[SectionGroupResults] ([SectionGroupId], [PlacementTestResultId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_UnitResults_CourseId_UnitId_StudentId] ON [dbo].[UnitResults] ([CourseId], [UnitId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_VideoResults_LessonResultId_VideoId_StudentId] ON [dbo].[VideoResults] ([LessonResultId], [VideoId], [StudentId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId] ON [dbo].[VideoTimeCodeAnswers] ([VideoResultId], [QuestionId], [VideoTimeCodeResultId]);");
            migrationBuilder.Sql("CREATE NONCLUSTERED INDEX [IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId] ON [dbo].[VideoTimeCodeResults] ([VideoResultId], [VideoTimeCodeId], [StudentId]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX [IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount] ON [dbo].[ClassForumDetailResults];");
            migrationBuilder.Sql("DROP INDEX [IX_ClassForumResults_LessonResultId_ClassForumId_StudentId] ON [dbo].[ClassForumResults];");
            migrationBuilder.Sql("DROP INDEX [IX_CourseResults_CourseId_StudentId] ON [dbo].[CourseResults];");
            migrationBuilder.Sql("DROP INDEX [IX_Courses_ParentCourseId_Priority] ON [dbo].[Courses];");
            migrationBuilder.Sql("DROP INDEX [IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId] ON [dbo].[FinalTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_FinalTestResults_CourseId_FinalTestId_StudentId] ON [dbo].[FinalTestResults];");
            migrationBuilder.Sql("DROP INDEX [IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId] ON [dbo].[HomeWorkAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId] ON [dbo].[HomeWorkResults];");
            migrationBuilder.Sql("DROP INDEX [IX_LessonResults_CourseId_UnitId_LessonId_StudentId] ON [dbo].[LessonResults];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId] ON [dbo].[MockTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[MockTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId] ON [dbo].[MockTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestResults_CourseId_MockTestId_StudentId] ON [dbo].[MockTestResults];");
            migrationBuilder.Sql("DROP INDEX [IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId] ON [dbo].[MockTestResults];");
            migrationBuilder.Sql("DROP INDEX [IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[PlacementTestAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_PlacementTestGroupResults_StudentId] ON [dbo].[PlacementTestGroupResults];");
            migrationBuilder.Sql("DROP INDEX [IX_PlacementTestResults_PlacementTestId_StudentId] ON [dbo].[PlacementTestResults];");
            migrationBuilder.Sql("DROP INDEX [IX_QuestionShuffles_StudentId_QuestionId] ON [dbo].[QuestionShuffles];");
            migrationBuilder.Sql("DROP INDEX [IX_SectionGroupResults_SectionGroupId_FinalTestResultId] ON [dbo].[SectionGroupResults];");
            migrationBuilder.Sql("DROP INDEX [IX_SectionGroupResults_SectionGroupId_MockTestResultId] ON [dbo].[SectionGroupResults];");
            migrationBuilder.Sql("DROP INDEX [IX_SectionGroupResults_SectionGroupId_PlacementTestResultId] ON [dbo].[SectionGroupResults];");
            migrationBuilder.Sql("DROP INDEX [IX_UnitResults_CourseId_UnitId_StudentId] ON [dbo].[UnitResults];");
            migrationBuilder.Sql("DROP INDEX [IX_VideoResults_LessonResultId_VideoId_StudentId] ON [dbo].[VideoResults];");
            migrationBuilder.Sql("DROP INDEX [IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId] ON [dbo].[VideoTimeCodeAnswers];");
            migrationBuilder.Sql("DROP INDEX [IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId] ON [dbo].[VideoTimeCodeResults];");

            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount] ON [dbo].[ClassForumDetailResults] ([ClassForumResultId], [SubmissionCount]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_ClassForumResults_LessonResultId_ClassForumId_StudentId] ON [dbo].[ClassForumResults] ([LessonResultId], [ClassForumId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_CourseResults_CourseId_StudentId] ON [dbo].[CourseResults] ([CourseId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_Courses_ParentCourseId_Priority] ON [dbo].[Courses] ([ParentCourseId], [Priority]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId] ON [dbo].[FinalTestAnswers] ([FinalTestResultId], [SectionQuestionId], [SectionGroupResultId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_FinalTestResults_CourseId_FinalTestId_StudentId] ON [dbo].[FinalTestResults] ([CourseId], [FinalTestId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId] ON [dbo].[HomeWorkAnswers] ([HomeWorkQuestionId], [HomeWorkResultId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId] ON [dbo].[HomeWorkResults] ([LessonResultId], [HomeWorkId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_LessonResults_CourseId_UnitId_LessonId_StudentId] ON [dbo].[LessonResults] ([CourseId], [UnitId], [LessonId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId] ON [dbo].[MockTestAnswers] ([MockTestResultId], [SectionGroupResultId], [SectionId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[MockTestAnswers] ([MockTestResultId], [SectionGroupResultId], [SectionQuestionId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId] ON [dbo].[MockTestAnswers] ([MockTestResultId], [SectionGroupResultId], [SectionTimeCodeId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestResults_CourseId_MockTestId_StudentId] ON [dbo].[MockTestResults] ([CourseId], [MockTestId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId] ON [dbo].[MockTestResults] ([CourseId], [MockTestId], [UnitId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[PlacementTestAnswers] ([PlacementTestResultId], [SectionGroupResultId], [SectionQuestionId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_PlacementTestGroupResults_StudentId] ON [dbo].[PlacementTestGroupResults] ([StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_PlacementTestResults_PlacementTestId_StudentId] ON [dbo].[PlacementTestResults] ([PlacementTestId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_QuestionShuffles_StudentId_QuestionId] ON [dbo].[QuestionShuffles] ([StudentId], [QuestionId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_FinalTestResultId] ON [dbo].[SectionGroupResults] ([SectionGroupId], [FinalTestResultId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_MockTestResultId] ON [dbo].[SectionGroupResults] ([SectionGroupId], [MockTestResultId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_PlacementTestResultId] ON [dbo].[SectionGroupResults] ([SectionGroupId], [PlacementTestResultId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_UnitResults_CourseId_UnitId_StudentId] ON [dbo].[UnitResults] ([CourseId], [UnitId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_VideoResults_LessonResultId_VideoId_StudentId] ON [dbo].[VideoResults] ([LessonResultId], [VideoId], [StudentId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId] ON [dbo].[VideoTimeCodeAnswers] ([VideoResultId], [QuestionId], [VideoTimeCodeResultId]);");
            migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId] ON [dbo].[VideoTimeCodeResults] ([VideoResultId], [VideoTimeCodeId], [StudentId]);");
        }
    }
}
