// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.EntityModels.ExportEventModels;
using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
using Fsel.Course.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Course.Infrastructure
{
    public class CourseDbContext : BaseDbContext
    {
        public CourseDbContext(DbContextOptions<CourseDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        public DbSet<MockTest> MockTests { get; set; }
        public DbSet<UnitSkillMockTest> UnitSkillMockTests { get; set; }
        public DbSet<PlacementTest> PlacementTests { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Domain.Entities.Course> Courses { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<LessonVideo> LessonVideos { get; set; }
        public DbSet<LessonHomeWork> LessonHomeWorks { get; set; }
        public DbSet<LessonExtraPractice> LessonExtraPractices { get; set; }
        public DbSet<ExtraPractice> ExtraPractices { get; set; }
        public DbSet<ExtraPracticeChapter> ExtraPracticeChapters { get; set; }
        public DbSet<ExtraPracticeExercise> ExtraPracticeExercises { get; set; }
        public DbSet<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; }
        public DbSet<ExtraPracticeResult> ExtraPracticeResults { get; set; }
        public DbSet<ExtraPracticeExerciseResult> ExtraPracticeExerciseResults { get; set; }
        public DbSet<HomeWork> HomeWorks { get; set; }
        public DbSet<ClassForum> ClassForums { get; set; }
        public DbSet<UnitLesson> UnitLessons { get; set; }
        public DbSet<Domain.Entities.Unit> Units { get; set; }
        public DbSet<CourseUnitMockTest> CourseUnitMockTests { get; set; }
        public DbSet<VideoTimeCode> VideoTimeCodes { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<TimeCodeExercise> TimeCodeExercises { get; set; }
        public DbSet<ExerciseQuestion> ExerciseQuestions { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<CourseTeacher> CourseTeachers { get; set; }
        public DbSet<VideoResult> VideoResults { get; set; }
        public DbSet<LessonResult> LessonResults { get; set; }
        public DbSet<UnitResult> UnitResults { get; set; }
        public DbSet<CourseResult> CourseResults { get; set; }
        public DbSet<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; }
        public DbSet<VideoTimeCodeResult> VideoTimeCodeResults { get; set; }
        public DbSet<LessonNote> LessonNotes { get; set; }
        public DbSet<QuestionForm> QuestionForms { get; set; }
        public DbSet<LessonInstruction> LessonInstructions { get; set; }
        public DbSet<HomeWorkQuestion> HomeWorkQuestions { get; set; }
        public DbSet<HomeWorkAnswer> HomeWorkAnswers { get; set; }
        public DbSet<HomeWorkResult> HomeWorkResults { get; set; }

        public DbSet<PlacementTestGroupResult> PlacementTestGroupResults { get; set; }
        public DbSet<PlacementTestSection> PlacementTestSections { get; set; }
        public DbSet<PlacementTestAnswer> PlacementTestAnswers { get; set; }
        public DbSet<PlacementTestResult> PlacementTestResults { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SectionGroup> SectionGroups { get; set; }
        public DbSet<SectionGroupResult> SectionGroupResults { get; set; }
        public DbSet<SectionPart> SectionParts { get; set; }
        public DbSet<SectionQuestion> SectionQuestions { get; set; }
        public DbSet<SectionTimeCode> SectionTimeCodes { get; set; }
        public DbSet<MockTestSection> MockTestSections { get; set; }
        public DbSet<MockTestScore> MockTestScores { get; set; }

        public DbSet<FinalTest> FinalTests { get; set; }
        public DbSet<MockTestResult> MockTestResults { get; set; }
        public DbSet<MockTestAnswer> MockTestAnswers { get; set; }
        public DbSet<FinalTestSection> FinalTestSections { get; set; }
        public DbSet<FinalTestAnswer> FinalTestAnswers { get; set; }
        public DbSet<FinalTestResult> FinalTestResults { get; set; }
        public DbSet<ClassForumResult> ClassForumResults { get; set; }
        public DbSet<ClassForumScore> ClassForumScores { get; set; }
        public DbSet<ClassForumResultFile> ClassForumResultFiles { get; set; }
        public DbSet<ClassForumFile> ClassForumFiles { get; set; }
        public DbSet<StudentFeedback> StudentFeedbacks { get; set; }
        public DbSet<ClassForumResultRandom> ClassForumResultRandoms { get; set; }
        public DbSet<MockTestAISetting> MockTestAISettings { get; set; }
        public DbSet<ClassForumDetailResult> ClassForumDetailResults { get; set; }
        public DbSet<MockTestAICriteriaSetting> MockTestAICriteriaSettings { get; set; }
        public DbSet<ProsodyScore> ProsodyScore { get; set; }
        public DbSet<CurriculumConfig> CurriculumConfigs { get; set; }
        public DbSet<CurriculumStudent> CurriculumStudents { get; set; }
        public DbSet<QuestionExplanationError> QuestionExplanationErrors { get; set; }
        public DbSet<QuestionExplanationLog> QuestionExplanationLogs { get; set; }
        public DbSet<QuestionShuffle> QuestionShuffles { get; set; }
        public DbSet<ClassForumDetailResultHistory> ClassForumDetailResultHistories { get; set; }
        public DbSet<WeeklyReport> WeeklyReports { get; set; }
        public DbSet<VideoSubFilePath> VideoSubFilePaths { get; set; }
        public DbSet<Topic> Topics { get; set; }

        #region Report

        public DbSet<TotalEvaluateInputResultModel> TotalEvaluateInputResults { get; set; }
        public DbSet<TotalDetailEvaluateInputResultModel> TotalDetailEvaluateInputResults { get; set; }
        public DbSet<PercentEvaluateInputResultModel> PercentEvaluateInputResults { get; set; }
        public DbSet<LevelEvaluateInputResultModel> LevelEvaluateInputResults { get; set; }
        public DbSet<SchoolSummaryModel> SchoolSummarys { get; set; }
        public DbSet<TotalLearningProgressModel> TotalLearningProgress { get; set; }
        public DbSet<AverageLearningProgressModel> AverageLearningProgress { get; set; }
        public DbSet<UnitDoneLearningProgressIeltsModel> UnitDoneLearningProgressIelts { get; set; }
        public DbSet<UnitDoneLearningProgressAcademicModel> UnitDoneLearningProgressAcademics { get; set; }
        public DbSet<LessonDoneLearningProgressIeltsModel> LessonDoneLearningProgressIelts { get; set; }
        public DbSet<LessonDoneLearningProgressAcademicModel> LessonDoneLearningProgressAcademics { get; set; }
        public DbSet<TotalLearningModel> TotalLearnings { get; set; }
        public DbSet<RateLearningModel> RateLearnings { get; set; }
        public DbSet<TotalLearningQualityModel> TotalLearningQualitys { get; set; }
        public DbSet<TotalDetailLearningQualityModel> TotalDetailLearningQualitys { get; set; }
        public DbSet<SchoolInfoModel> SchoolInfos { get; set; }
        public DbSet<SchoolInfoFilterModel> SchoolInfoFilters { get; set; }
        public DbSet<DistrictInfoModel> DistrictInfos { get; set; }
        public DbSet<ExportSummaryReportCommandModel> ExportSummaryReports { get; set; }
        public DbSet<CourseCompleteReportModel> CourseCompleteReports { get; set; }
        public DbSet<ReportLearningProcessModel> ReportLearningProcesses { get; set; }
        public DbSet<ReportLearningResultModel> ReportLearningResults { get; set; }
        public DbSet<ExportStudentEventModel> ExportStudentEvents { get; set; }
        public DbSet<ExportDistrictEventModel> ExportDistrictEvents { get; set; }
        public DbSet<ExportSchoolEventModel> ExportSchoolEvents { get; set; }
        public DbSet<HomeWorkConfig> HomeWorkConfigs { get; set; }

        #endregion Report

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.ApplyConfiguration(new ClassForumEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseTeacherEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseUnitMockTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExerciseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExerciseQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExtraPracticeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExtraPracticeChapterEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExtraPracticeAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExtraPracticeResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExtraPracticeExerciseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExtraPracticeExerciseResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CurriculumEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CurriculumStudentTypeConfiguration());

            modelBuilder.ApplyConfiguration(new HomeWorkEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonExtraPracticeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonHomeWorkEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonVideoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PlacementTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TimeCodeExerciseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitLessonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitSkillMockTestEntityTestConfiguration());
            modelBuilder.ApplyConfiguration(new VideoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VideoTimeCodeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonResultEntityTypeConfiguraion());
            modelBuilder.ApplyConfiguration(new VideoResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VideoTimeCodeAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VideoTimeCodeResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionFormEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonInstructionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HomeWorkAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HomeWorkResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HomeWorkQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonNoteEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitResultEntityTypeConfiguraion());

            modelBuilder.ApplyConfiguration(new PlacementTestGroupResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PlacementTestSectionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PlacementTestAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PlacementTestResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionGroupEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionGroupResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionPartEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionTimeCodeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestSectionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FinalTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FinalTestAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FinalTestSectionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FinalTestResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassForumResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassForumScoreEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassForumFileEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassForumResultFileEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentFeedbackEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassForumResultRandomEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestAISettingTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestScoreEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestAICriteriaSettingTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassForumDetailResultTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProsodyScoreEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionExplanationErrorEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionExplanationLogEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionShuffleEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassForumDetailResultHistoryTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TopicEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VideoSubFilePathEntityTypeConfiguration());

            modelBuilder.Ignore<TotalEvaluateInputResultModel>();
            modelBuilder.Ignore<TotalDetailEvaluateInputResultModel>();
            modelBuilder.Ignore<PercentEvaluateInputResultModel>();
            modelBuilder.Ignore<LevelEvaluateInputResultModel>();
            modelBuilder.Ignore<SchoolSummaryModel>();
            modelBuilder.Ignore<TotalLearningProgressModel>();
            modelBuilder.Ignore<AverageLearningProgressModel>();
            modelBuilder.Ignore<UnitDoneLearningProgressIeltsModel>();
            modelBuilder.Ignore<UnitDoneLearningProgressAcademicModel>();
            modelBuilder.Ignore<LessonDoneLearningProgressIeltsModel>();
            modelBuilder.Ignore<LessonDoneLearningProgressAcademicModel>();
            modelBuilder.Ignore<TotalLearningModel>();
            modelBuilder.Ignore<RateLearningModel>();
            modelBuilder.Ignore<TotalLearningQualityModel>();
            modelBuilder.Ignore<TotalDetailLearningQualityModel>();
            modelBuilder.Ignore<SchoolInfoModel>();
            modelBuilder.Ignore<CourseCompleteReportModel>();
            modelBuilder.Ignore<SchoolInfoFilterModel>();
            modelBuilder.Ignore<DistrictInfoModel>();
            modelBuilder.Ignore<ReportLearningProcessModel>();
            modelBuilder.Ignore<ReportLearningResultModel>();
            modelBuilder.Ignore<ExportSchoolEventModel>();
            modelBuilder.Ignore<ExportStudentEventModel>();
            modelBuilder.Ignore<ExportSummaryReportCommandModel>();
            modelBuilder.Ignore<ExportDistrictEventModel>();

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile(Settings.SettingFileName)
                    .Build();
                optionsBuilder.UseSqlServer(
                    configuration.GetConnectionString(Settings.DefaultConnection),
                    options => options.MigrationsAssembly(GetType().Assembly.GetName().Name));
            }
        }
    }
}