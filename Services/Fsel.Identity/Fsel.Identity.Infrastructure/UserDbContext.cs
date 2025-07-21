// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Core.Entities;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi;
using Fsel.Identity.Infrastructure.Configs;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Identity.Infrastructure
{
    public class UserReadDbContext : BaseUserDbContext
    {
        protected override string Connection => Settings.ReadOnlyConnection;

        public UserReadDbContext(DbContextOptions<UserReadDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }
    }

    public class UserDbContext : BaseUserDbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            base.OnModelCreating(builder);
            SeedMenus(builder);
            SeedPlatforms(builder);
            SeedRoles(builder);
            SeedPermissions(builder);
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

        private static void SeedPermissions(ModelBuilder builder)
        {
            var pathPermissionGroup = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.PermissionGroupName);
            var permissionGroups = ConvertHelper.DeserializeFromFilePath<IList<PermissionGroup>>(pathPermissionGroup);

            ArgumentNullException.ThrowIfNull(permissionGroups);
            builder.Entity<PermissionGroup>().HasData(permissionGroups);

            var pathPermission = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.PermissionName);
            var permissions = ConvertHelper.DeserializeFromFilePath<IList<Permission>>(pathPermission);

            ArgumentNullException.ThrowIfNull(permissions);
            builder.Entity<Permission>().HasData(permissions);

            var pathRoleClaim = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.RoleClaimName);
            var roleClaims = ConvertHelper.DeserializeFromFilePath<IList<RoleClaim>>(pathRoleClaim);

            ArgumentNullException.ThrowIfNull(roleClaims);
            builder.Entity<RoleClaim>().HasData(roleClaims);
        }

        private static void SeedMenus(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.MenuName);
            var menus = ConvertHelper.DeserializeFromFilePath<IList<Menu>>(path);
            ArgumentNullException.ThrowIfNull(menus);
            if (menus != null)
            {
                builder.Entity<Menu>().HasData(menus);
            }
        }
    }

    public class BaseUserDbContext : BaseIdentityDbContext<User, Role, Guid, UserClaimEntity, RoleClaimEntity, UserRole, UserLoginEntity, UserToken>
    {
        protected virtual string Connection => Settings.DefaultConnection;

        public BaseUserDbContext(DbContextOptions options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            //Dùng khi tạo migration, comment lại sau khi tạo xong
            builder.Ignore<OverallStudentModel>();
            builder.Ignore<NumberStudentLearnOnSystemModel>();
            builder.Ignore<SummaryDataOnCityModel>();

            builder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<UserToken>().HasQueryFilter(e => !e.IsDeleted);
            builder.HasSequence<int>(SqlSettings.Sequence.UserSequence).StartsAt(100000).IncrementsBy(1);

            base.OnModelCreating(builder);
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
            builder.ApplyConfiguration(new EventManagerEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserReferralEntityTypeConfiguration());
            builder.ApplyConfiguration(new EventRegistrationEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserDeletionEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentDailyStreakEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserSchoolEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserTokenEntityTypeConfiguration());
            builder.ApplyConfiguration(new PermissionEntityTypeConfiguration());
            builder.ApplyConfiguration(new RoleClaimEntityTypeConfiguration());
            builder.ApplyConfiguration(new MenuEntityTypeConfiguration());
            builder.ApplyConfiguration(new PermissionGroupEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserGroupMemberShipEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentEditHistoryEntityTypeConfiguration());
        }

        #region Db Set

        public override DbSet<User> Users { get; set; }
        public override DbSet<UserToken> UserTokens { get; set; }
        public override DbSet<Role> Roles { get; set; }
        public override DbSet<UserRole> UserRoles { get; set; }
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
        public DbSet<EventManager> EventManagers { get; set; }
        public DbSet<UserReferral> UserReferrals { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<UserDeletion> UserDeletions { get; set; }
        public DbSet<UserSchool> UserSchools { get; set; }
        public DbSet<SchoolImportHistory> SchoolImportHistorys { get; set; }
        public DbSet<StudentEventLearningRecord> StudentEventLearningRecords { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserGroupMemberShip> UserGroupMemberShips { get; set; }
        public DbSet<StudentEditHistory> StudentEditHistories { get; set; }
        public DbSet<Menu> Menus { get; set; }

        #endregion Db Set

        #region report

        public DbSet<OverallStudentModel> OverallStudentResults { get; set; }

        public DbSet<NumberStudentLearnOnSystemModel> NumberStudentLearnOnSystemResults { get; set; }

        public DbSet<SummaryDataOnCityModel> SummaryDataOnCityResults { get; set; }

        #endregion report

        public DbSet<RoleClaim> RoleClaims { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<Permission> Permissions { get; set; }

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
