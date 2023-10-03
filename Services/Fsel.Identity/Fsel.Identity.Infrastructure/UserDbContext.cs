// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Infrastructure.Configs;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Identity.Infrastructure
{
    public class UserDbContext : BaseIdentityDbContext<User, Role, string, IdentityUserClaim<string>, IdentityRoleClaim<string>, UserToken>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            SeedPlatforms(builder);

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

            builder.ApplyConfiguration(new StudenrRankingEntityTypeConfiguration());
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
        public DbSet<Platform> Platform { get; set; }
        public DbSet<UserPlatform> UserPlatforms { get; set; }
        public DbSet<StudentRanking> StudentRankings { get; set; }

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

        //private static void SeedRoles(ModelBuilder builder)
        //{
        //    builder.Entity<Role>().HasData
        //        (
        //            new Role() { Name = EnumRole.MasterAdmin.ToString(), NormalizedName = EnumRole.MasterAdmin.ToString() },
        //            new Role() { Name = EnumRole.Admin.ToString(), NormalizedName = EnumRole.Admin.ToString() },
        //            new Role() { Name = EnumRole.CSO.ToString(), NormalizedName = EnumRole.CSO.ToString() },
        //            new Role() { Name = EnumRole.Teacher.ToString(), NormalizedName = EnumRole.Teacher.ToString() },
        //            new Role() { Name = EnumRole.Parent.ToString(), NormalizedName = EnumRole.Parent.ToString() },
        //            new Role() { Name = EnumRole.Student.ToString(), NormalizedName = EnumRole.Student.ToString() }
        //        );
        //}
    }
}
