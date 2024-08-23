// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Core.Entities;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Infrastructure.Configs;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Identity.Infrastructure
{
    public class UserDbContext : BaseIdentityDbContext<User, Role, Guid, UserClaimEntity, RoleClaimEntity, UserToken>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<UserToken>().HasQueryFilter(e => !e.IsDeleted);

            SeedPlatforms(builder);
            SeedRoles(builder);

            builder.ApplyConfiguration(new HumanEntityTypeConfiguration());
            builder.ApplyConfiguration(new TeacherEntityTypeConfiguration());
            builder.ApplyConfiguration(new TeacherBankAccountEntityTypeConfiguration());
            builder.ApplyConfiguration(new CSOEntityTypeConfiguration());
            builder.ApplyConfiguration(new ParentEntityTypeConfiguration());
            builder.ApplyConfiguration(new ParentStudentEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserOtpCodeEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserSettingEntityTypeConfiguration());
            builder.ApplyConfiguration(new PlatformEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserPlatformEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserCourseSettingEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudenrRankingEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentFocusTimeEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentTrialRegistrationEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentCompetitionEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentCompetitionEventsEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentRankingEventEntityTypeConfiguration());
            builder.ApplyConfiguration(new CompetitionEventsEntityTypeConfiguration());
            base.OnModelCreating(builder);
        }

        #region Db Set

        public override DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Human> Humans { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<CSO> CSOs { get; set; }
        public DbSet<TeacherBankAccount> TeacherBankAccounts { get; set; }
        public DbSet<ParentStudent> ParentStudents { get; set; }
        public DbSet<UserOtpCode> UserOtpCodes { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<UserCourseSetting> UserCourseSettings { get; set; }
        public DbSet<Platform> Platform { get; set; }
        public DbSet<UserPlatform> UserPlatforms { get; set; }
        public DbSet<StudentRanking> StudentRankings { get; set; }
        public DbSet<StudentFocusTime> StudentFocusTimes { get; set; }
        public DbSet<StudentTrialRegistration> StudentTrialRegistrations { get; set; }
        public DbSet<StudentCompetitionSnapShot> StudentCompetitionSnapShots { get; set; }
        public DbSet<StudentCompetitionEvent> StudentCompetitionEvents { get; set; }
        public DbSet<StudentRankingEvent> StudentRankingEvents { get; set; }
        public DbSet<CompetitionEvent> CompetitionEvents { get; set; }

        #endregion Db Set

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

        private static void SeedPlatforms(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.PlatformFileName);
            var platforms = ConvertHelper.DeserializeFromFilePath<IList<Platform>>(path);
            if (platforms != null)
            {
                ArgumentNullException.ThrowIfNull(platforms);
                builder.Entity<Platform>().HasData(platforms);
            }
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.RoleFileName);
            var roles = ConvertHelper.DeserializeFromFilePath<IList<Role>>(path);
            if (roles != null)
            {
                ArgumentNullException.ThrowIfNull(roles);
                builder.Entity<Role>().HasData(roles);
            }
        }
    }
}
