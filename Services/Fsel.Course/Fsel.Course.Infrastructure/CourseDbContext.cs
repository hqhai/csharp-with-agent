using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Unit = Fsel.Course.Domain.Entities.Unit;

/*using Course = Fsel.Course.Domain.Entities.Course;*/

namespace Fsel.Course.Infrastructure
{
    public class CourseDbContext : BaseDbContext
    {
        public CourseDbContext(DbContextOptions<CourseDbContext> options, IMediator mediator) : base(options, mediator)
        {
        }

        #region Db Set

        public DbSet<MockTest> MockTests { get; set; }
        public DbSet<MockFinalTest> UnitMockFinalTests { get; set; }
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

        public DbSet<Excercise> Excercises { get; set; }

        public DbSet<TimeCodeExcercise> TimeCodeExcercises { get; set; }

        public DbSet<ExcerciseQuestion> ExcerciseQuestions { get; set; }

        public DbSet<Question> Questions { get; set; }

        #endregion Db Set

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ClassForumEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseUnitMockTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExcerciseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExcerciseQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExtraPracticeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HomeWorkEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonExtraPracticeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonHomeWorkEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonVideoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PlacementTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TimeCodeExcerciseEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitLessonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitMockFinalEntityTestConfiguration());
            modelBuilder.ApplyConfiguration(new VideoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VideoTimeCodeEntityTypeConfiguration());

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

        //public class UserContextDesignFactory : IDesignTimeDbContextFactory<CourseDbContext>
        //{
        //    public CourseDbContext CreateDbContext(string[] args)
        //    {
        //        var optionsBuilder = new DbContextOptionsBuilder<CourseDbContext>();

        //        optionsBuilder.UseSqlServer(
        //            "Data Source=183.91.11.235;Initial Catalog=env-dev.course-service;User ID=sa;Password=FselTech@2023",
        //            options => options.MigrationsAssembly(GetType().Assembly.GetName().Name));
        //        return new CourseDbContext(optionsBuilder.Options, );
        //    }
        //}
    }
}