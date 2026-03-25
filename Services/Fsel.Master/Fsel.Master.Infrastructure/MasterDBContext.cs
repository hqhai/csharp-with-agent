using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Master.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Master.Infrastructure
{
    public class MasterReadDbContext : MasterBaseDBContext
    {
        protected override string Connection => Settings.ReadOnlyConnection;

        public MasterReadDbContext(DbContextOptions<MasterReadDbContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    public class MasterDBContext : MasterBaseDBContext
    {
        public MasterDBContext(DbContextOptions<MasterDBContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }
    }

    public class MasterBaseDBContext : BaseDbContext
    {
        protected virtual string Connection => Settings.DefaultConnection;

        public MasterBaseDBContext(DbContextOptions options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.Entity<PlacementTestReport>()
                        .Property(x => x.Status)
                        .HasConversion<string>();

            modelBuilder.Entity<StudentLearningProgress>()
                        .Property(x => x.CurrentProgressStatus)
                        .HasConversion<string>();

            modelBuilder.Entity<StudentWeeklyLearningProgress>()
                        .Property(e => e.ProgressStatus)
                              .HasMaxLength(20)
                              .HasConversion(
                                   v => v.ToString(),
                                   v => v.EnumParse<EnumCurrentProgressStatus>());

            modelBuilder.Entity<CourseResult>()
                        .Property(x => x.Status)
                        .HasConversion<string>();

            modelBuilder.Entity<CourseResult>()
                        .Property(x => x.WorkingStatus)
                        .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<StudentLearningProgress> StudentLearningProgresses { get; set; }
        public DbSet<StudentWeeklyLearningProgress> StudentWeeklyLearningProgresses { get; set; }
        public DbSet<StudentProfileReport> StudentProfileReports { get; set; }
        public DbSet<PlacementTestReport> PlacementTestReports { get; set; }
        public DbSet<StudentCompetitionEvent> StudentCompetitionEvents { get; set; }
        public DbSet<CompetitionEvent> CompetitionEvents { get; set; }
        public DbSet<Program> Programs { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<CourseResult> CourseResults { get; set; }
        public DbSet<Course> Courses { get; set; }

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
