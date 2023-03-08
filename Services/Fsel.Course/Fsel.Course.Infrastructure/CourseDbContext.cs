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

        public DbSet<MockFinalTest> MockFinalTests { get; set; }
        public DbSet<UnitMockFinalTest> UnitMockFinalTests { get; set; }
        public DbSet<PlacementTest> PlacementTests { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Domain.Entities.Course> Courses { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<LessonVideo> LessonVideos { get; set; }
        public DbSet<UnitLesson> UnitLessons { get; set; }
        public DbSet<Domain.Entities.Unit> Units { get; set; }
        public DbSet<CourseUnit> CourseUnits { get; set; }

        #endregion Db Set

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PlacementTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VideoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonVideoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitLessonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UnitMockFinalTestConfiuration());
            modelBuilder.ApplyConfiguration(new UnitTypeConfiuration());
            modelBuilder.ApplyConfiguration(new CourseTypeConfiguration());

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