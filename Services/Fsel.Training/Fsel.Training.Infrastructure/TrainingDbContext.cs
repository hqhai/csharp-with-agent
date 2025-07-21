// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure
{
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Infrastructure.Configs;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class TrainingReadDbContext : BaseTrainingDbContext
    {
        protected override string Connection => Settings.ReadOnlyConnection;

        public TrainingReadDbContext(DbContextOptions<TrainingDbContext> options, IMediator mediator, AuthContext authContext)
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

    public class TrainingDbContext : BaseTrainingDbContext
    {
        public TrainingDbContext(DbContextOptions<TrainingDbContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }
    }

    public class BaseTrainingDbContext : BaseDbContext
    {
        protected virtual string Connection => Settings.DefaultConnection;

        public BaseTrainingDbContext(DbContextOptions options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.ApplyConfiguration(new ClassEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassStudentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassLiveCalendarEntityTypeConfiguaration());
            modelBuilder.ApplyConfiguration(new TeacherFreeTimeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TeacherFreeDateEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassLiveWorkFlowEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassLiveWorkFlowPlanEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TeacherFreeTimeLiveEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassStudent> ClassStudents { get; set; }
        public DbSet<ClassLiveCalendar> ClassLiveCalendars { get; set; }
        public DbSet<TeacherFreeTime> TeacherFreeTimes { get; set; }
        public DbSet<TeacherFreeDate> TeacherFreeDates { get; set; }
        public DbSet<ClassLiveWorkFlow> ClassLiveWorkFlows { get; set; }
        public DbSet<ClassLiveWorkFlowPlan> ClassLiveWorkFlowPlans { get; set; }
        public DbSet<TeacherFreeTimeLive> TeacherFreeTimeLives { get; set; }

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
