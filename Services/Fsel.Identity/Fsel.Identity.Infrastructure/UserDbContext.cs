// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Identity.Infrastructure
{
    public class UserDbContext : BaseIdentityDbContext<User>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options, IMediator mediator) : base(options, mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            //SeedRoles(builder);

            builder.ApplyConfiguration(new HumanEntityTypeConfiguration());
            builder.ApplyConfiguration(new TeacherEntityTypeConfiguration());
            builder.ApplyConfiguration(new TeacherBankAccountEntityTypeConfiguration());
            builder.ApplyConfiguration(new CSOEntityTypeConfiguration());
            builder.ApplyConfiguration(new ParentEntityTypeConfiguration());
            builder.ApplyConfiguration(new ParentStudentEntityTypeConfiguration());
            builder.ApplyConfiguration(new StudentEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserOtpCodeEntityTypeConfiguration());
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
