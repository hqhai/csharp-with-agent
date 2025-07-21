using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Drop_Index_Unique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DROP INDEX [IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount] ON [dbo].[ClassForumDetailResults];
            DROP INDEX [IX_ClassForumResults_LessonResultId_ClassForumId_StudentId] ON [dbo].[ClassForumResults];
            DROP INDEX [IX_CourseResults_CourseId_StudentId] ON [dbo].[CourseResults];
            DROP INDEX [IX_Courses_ParentCourseId_Priority] ON [dbo].[Courses];
            DROP INDEX [IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId] ON [dbo].[FinalTestAnswers];
            DROP INDEX [IX_FinalTestResults_CourseId_FinalTestId_StudentId] ON [dbo].[FinalTestResults];
            DROP INDEX [IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId] ON [dbo].[HomeWorkAnswers];
            DROP INDEX [IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId] ON [dbo].[HomeWorkResults];
            DROP INDEX [IX_LessonResults_CourseId_UnitId_LessonId_StudentId] ON [dbo].[LessonResults];
            DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId] ON [dbo].[MockTestAnswers];
            DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[MockTestAnswers];
            DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId] ON [dbo].[MockTestAnswers];
            DROP INDEX [IX_MockTestResults_CourseId_MockTestId_StudentId] ON [dbo].[MockTestResults];
            DROP INDEX [IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId] ON [dbo].[MockTestResults];
            DROP INDEX [IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[PlacementTestAnswers];
            DROP INDEX [IX_PlacementTestGroupResults_StudentId] ON [dbo].[PlacementTestGroupResults];
            DROP INDEX [IX_PlacementTestResults_PlacementTestId_StudentId] ON [dbo].[PlacementTestResults];
            DROP INDEX [IX_QuestionShuffles_StudentId_QuestionId] ON [dbo].[QuestionShuffles];
            DROP INDEX [IX_SectionGroupResults_SectionGroupId_FinalTestResultId] ON [dbo].[SectionGroupResults];
            DROP INDEX [IX_SectionGroupResults_SectionGroupId_MockTestResultId] ON [dbo].[SectionGroupResults];
            DROP INDEX [IX_SectionGroupResults_SectionGroupId_PlacementTestResultId] ON [dbo].[SectionGroupResults];
            DROP INDEX [IX_UnitResults_CourseId_UnitId_StudentId] ON [dbo].[UnitResults];
            DROP INDEX [IX_VideoResults_LessonResultId_VideoId_StudentId] ON [dbo].[VideoResults];
            DROP INDEX [IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId] ON [dbo].[VideoTimeCodeAnswers];
            DROP INDEX [IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId] ON [dbo].[VideoTimeCodeResults];

            CREATE NONCLUSTERED INDEX [IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount] ON [dbo].[ClassForumDetailResults] (ClassForumResultId, SubmissionCount) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_ClassForumResults_LessonResultId_ClassForumId_StudentId] ON [dbo].[ClassForumResults] (LessonResultId, ClassForumId, StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_CourseResults_CourseId_StudentId] ON [dbo].[CourseResults] (CourseId, StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_Courses_ParentCourseId_Priority] ON [dbo].[Courses] (ParentCourseId, Priority) WHERE ([ParentCourseId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId] ON [dbo].[FinalTestAnswers] (FinalTestResultId, SectionQuestionId, SectionGroupResultId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_FinalTestResults_CourseId_FinalTestId_StudentId] ON [dbo].[FinalTestResults] (CourseId, FinalTestId, StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId] ON [dbo].[HomeWorkAnswers] (HomeWorkQuestionId, HomeWorkResultId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId] ON [dbo].[HomeWorkResults] (LessonResultId, HomeWorkId, StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_LessonResults_CourseId_UnitId_LessonId_StudentId] ON [dbo].[LessonResults] (CourseId, UnitId, LessonId, StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId] ON [dbo].[MockTestAnswers] (MockTestResultId, SectionGroupResultId, SectionId) WHERE ([SectionId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[MockTestAnswers] (MockTestResultId, SectionGroupResultId, SectionQuestionId) WHERE ([SectionQuestionId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId] ON [dbo].[MockTestAnswers] (MockTestResultId, SectionGroupResultId, SectionTimeCodeId) WHERE ([SectionTimeCodeId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_MockTestResults_CourseId_MockTestId_StudentId] ON [dbo].[MockTestResults] (CourseId, MockTestId, StudentId) WHERE ([UnitId] IS NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId] ON [dbo].[MockTestResults] (CourseId, MockTestId, UnitId, StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[PlacementTestAnswers] (PlacementTestResultId, SectionGroupResultId, SectionQuestionId) WHERE ([SectionGroupResultId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_PlacementTestGroupResults_StudentId] ON [dbo].[PlacementTestGroupResults] (StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_PlacementTestResults_PlacementTestId_StudentId] ON [dbo].[PlacementTestResults] (PlacementTestId, StudentId) WHERE ([PlacementTestId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_QuestionShuffles_StudentId_QuestionId] ON [dbo].[QuestionShuffles] (StudentId, QuestionId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_FinalTestResultId] ON [dbo].[SectionGroupResults] (SectionGroupId, FinalTestResultId) WHERE ([FinalTestResultId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_MockTestResultId] ON [dbo].[SectionGroupResults] (SectionGroupId, MockTestResultId) WHERE ([MockTestResultId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_PlacementTestResultId] ON [dbo].[SectionGroupResults] (SectionGroupId, PlacementTestResultId) WHERE ([PlacementTestResultId] IS NOT NULL AND [IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_UnitResults_CourseId_UnitId_StudentId] ON [dbo].[UnitResults] (CourseId, UnitId, StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_VideoResults_LessonResultId_VideoId_StudentId] ON [dbo].[VideoResults] (LessonResultId, VideoId, StudentId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId] ON [dbo].[VideoTimeCodeAnswers] (VideoResultId, QuestionId, VideoTimeCodeResultId) WHERE ([IsDeleted]=(0));
            CREATE NONCLUSTERED INDEX [IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId] ON [dbo].[VideoTimeCodeResults] (VideoResultId, VideoTimeCodeId, StudentId) WHERE ([IsDeleted]=(0));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX [IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount] ON [dbo].[ClassForumDetailResults];
                DROP INDEX [IX_ClassForumResults_LessonResultId_ClassForumId_StudentId] ON [dbo].[ClassForumResults];
                DROP INDEX [IX_CourseResults_CourseId_StudentId] ON [dbo].[CourseResults];
                DROP INDEX [IX_Courses_ParentCourseId_Priority] ON [dbo].[Courses];
                DROP INDEX [IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId] ON [dbo].[FinalTestAnswers];
                DROP INDEX [IX_FinalTestResults_CourseId_FinalTestId_StudentId] ON [dbo].[FinalTestResults];
                DROP INDEX [IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId] ON [dbo].[HomeWorkAnswers];
                DROP INDEX [IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId] ON [dbo].[HomeWorkResults];
                DROP INDEX [IX_LessonResults_CourseId_UnitId_LessonId_StudentId] ON [dbo].[LessonResults];
                DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId] ON [dbo].[MockTestAnswers];
                DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[MockTestAnswers];
                DROP INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId] ON [dbo].[MockTestAnswers];
                DROP INDEX [IX_MockTestResults_CourseId_MockTestId_StudentId] ON [dbo].[MockTestResults];
                DROP INDEX [IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId] ON [dbo].[MockTestResults];
                DROP INDEX [IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[PlacementTestAnswers];
                DROP INDEX [IX_PlacementTestGroupResults_StudentId] ON [dbo].[PlacementTestGroupResults];
                DROP INDEX [IX_PlacementTestResults_PlacementTestId_StudentId] ON [dbo].[PlacementTestResults];
                DROP INDEX [IX_QuestionShuffles_StudentId_QuestionId] ON [dbo].[QuestionShuffles];
                DROP INDEX [IX_SectionGroupResults_SectionGroupId_FinalTestResultId] ON [dbo].[SectionGroupResults];
                DROP INDEX [IX_SectionGroupResults_SectionGroupId_MockTestResultId] ON [dbo].[SectionGroupResults];
                DROP INDEX [IX_SectionGroupResults_SectionGroupId_PlacementTestResultId] ON [dbo].[SectionGroupResults];
                DROP INDEX [IX_UnitResults_CourseId_UnitId_StudentId] ON [dbo].[UnitResults];
                DROP INDEX [IX_VideoResults_LessonResultId_VideoId_StudentId] ON [dbo].[VideoResults];
                DROP INDEX [IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId] ON [dbo].[VideoTimeCodeAnswers];
                DROP INDEX [IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId] ON [dbo].[VideoTimeCodeResults];

                CREATE UNIQUE NONCLUSTERED INDEX [IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount] ON [dbo].[ClassForumDetailResults] (ClassForumResultId, SubmissionCount) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_ClassForumResults_LessonResultId_ClassForumId_StudentId] ON [dbo].[ClassForumResults] (LessonResultId, ClassForumId, StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_CourseResults_CourseId_StudentId] ON [dbo].[CourseResults] (CourseId, StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_Courses_ParentCourseId_Priority] ON [dbo].[Courses] (ParentCourseId, Priority) WHERE ([ParentCourseId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId] ON [dbo].[FinalTestAnswers] (FinalTestResultId, SectionQuestionId, SectionGroupResultId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_FinalTestResults_CourseId_FinalTestId_StudentId] ON [dbo].[FinalTestResults] (CourseId, FinalTestId, StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId] ON [dbo].[HomeWorkAnswers] (HomeWorkQuestionId, HomeWorkResultId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId] ON [dbo].[HomeWorkResults] (LessonResultId, HomeWorkId, StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_LessonResults_CourseId_UnitId_LessonId_StudentId] ON [dbo].[LessonResults] (CourseId, UnitId, LessonId, StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId] ON [dbo].[MockTestAnswers] (MockTestResultId, SectionGroupResultId, SectionId) WHERE ([SectionId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[MockTestAnswers] (MockTestResultId, SectionGroupResultId, SectionQuestionId) WHERE ([SectionQuestionId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId] ON [dbo].[MockTestAnswers] (MockTestResultId, SectionGroupResultId, SectionTimeCodeId) WHERE ([SectionTimeCodeId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestResults_CourseId_MockTestId_StudentId] ON [dbo].[MockTestResults] (CourseId, MockTestId, StudentId) WHERE ([UnitId] IS NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId] ON [dbo].[MockTestResults] (CourseId, MockTestId, UnitId, StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId] ON [dbo].[PlacementTestAnswers] (PlacementTestResultId, SectionGroupResultId, SectionQuestionId) WHERE ([SectionGroupResultId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_PlacementTestGroupResults_StudentId] ON [dbo].[PlacementTestGroupResults] (StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_PlacementTestResults_PlacementTestId_StudentId] ON [dbo].[PlacementTestResults] (PlacementTestId, StudentId) WHERE ([PlacementTestId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_QuestionShuffles_StudentId_QuestionId] ON [dbo].[QuestionShuffles] (StudentId, QuestionId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_FinalTestResultId] ON [dbo].[SectionGroupResults] (SectionGroupId, FinalTestResultId) WHERE ([FinalTestResultId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_MockTestResultId] ON [dbo].[SectionGroupResults] (SectionGroupId, MockTestResultId) WHERE ([MockTestResultId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_SectionGroupResults_SectionGroupId_PlacementTestResultId] ON [dbo].[SectionGroupResults] (SectionGroupId, PlacementTestResultId) WHERE ([PlacementTestResultId] IS NOT NULL AND [IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_UnitResults_CourseId_UnitId_StudentId] ON [dbo].[UnitResults] (CourseId, UnitId, StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_VideoResults_LessonResultId_VideoId_StudentId] ON [dbo].[VideoResults] (LessonResultId, VideoId, StudentId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId] ON [dbo].[VideoTimeCodeAnswers] (VideoResultId, QuestionId, VideoTimeCodeResultId) WHERE ([IsDeleted]=(0));
                CREATE UNIQUE NONCLUSTERED INDEX [IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId] ON [dbo].[VideoTimeCodeResults] (VideoResultId, VideoTimeCodeId, StudentId) WHERE ([IsDeleted]=(0));
            ");
        }
    }
}
