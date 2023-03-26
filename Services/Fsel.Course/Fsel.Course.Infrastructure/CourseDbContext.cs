using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

/*using Course = Fsel.Course.Domain.Entities.Course;*/

namespace Fsel.Course.Infrastructure
{
    public class CourseDbContext : BaseDbContext
    {
        public CourseDbContext(DbContextOptions<CourseDbContext> options, IMediator mediator) : base(options, mediator)
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
        public DbSet<CourseStudent> CourseStudents { get; set; }
        public DbSet<CourseClass> CourseClasses { get; set; }
        public DbSet<UnitStudent> UnitStudents { get; set; }
        public DbSet<LessonStudent> LessonStudents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ClassForumEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseStudentEntityTypeConfiguration());
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
            modelBuilder.ApplyConfiguration(new LessonStudentEntityTypeConfiguraion());
            modelBuilder.ApplyConfiguration(new UnitStudentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CourseClassEntityTypeConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
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