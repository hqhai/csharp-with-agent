// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
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
        public DbSet<LessonNote> LessonNotes { get; set; }
        public DbSet<QuestionForm> QuestionForms { get; set; }
        public DbSet<LessonInstruction> LessonInstructions { get; set; }
        public DbSet<HomeWorkQuestion> HomeWorkQuestions { get; set; }
        public DbSet<HomeWorkAnswer> HomeWorkAnswers { get; set; }
        public DbSet<HomeWorkResult> HomeWorkResults { get; set; }

        public DbSet<PlacementTestSection> PlacementTestSections { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SectionGroup> SectionGroups { get; set; }
        public DbSet<SectionPart> SectionParts { get; set; }
        public DbSet<SectionQuestion> SectionQuestions { get; set; }
        public DbSet<SectionTimeCode> SectionTimeCodes { get; set; }
        public DbSet<MockTestSection> MockTestSections { get; set; }
        public DbSet<FinalTest> FinalTests { get; set; }
        public DbSet<MockTestResult> MockTestResults { get; set; }
        public DbSet<MockTestAnswer> MockTestAnswer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.ApplyConfiguration(new ClassForumEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseTeacherEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseUnitMockTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExerciseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExerciseQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExtraPracticeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HomeWorkEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonExtraPracticeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonHomeWorkEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonVideoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestEntityTypeConfiguration());
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
            modelBuilder.ApplyConfiguration(new CourseResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VideoTimeCodeAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionFormEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonInstructionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HomeWorkAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HomeWorkResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HomeWorkQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PlacementTestSectionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionGroupEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionPartEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SectionTimeCodeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestSectionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FinalTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MockTestAnswerEntityTypeConfiguration());
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
