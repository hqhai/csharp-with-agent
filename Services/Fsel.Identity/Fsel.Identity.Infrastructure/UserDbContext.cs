using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Core.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Infrastructure.Configs;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Identity.Infrastructure
{
    public class UserDbContext : BaseIdentityDbContext<User>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options, IMediator mediator) : base(options, mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HumanEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ParentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ParentStudentEntityTypeConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        #region Db Set

        public override DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Human> Humans { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<RoleClaim> RoleClaims { get; set; }
        public DbSet<UserClaim> UserClaims { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<ParentStudent> ParentStudents { get; set; }

        #endregion Db Set

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

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData
                (
                    new IdentityRole() { Name = EnumRole.MasterAdmin.ToString(), NormalizedName = EnumRole.MasterAdmin.ToString() },
                    new IdentityRole() { Name = EnumRole.Admin.ToString(), NormalizedName = EnumRole.Admin.ToString() },
                    new IdentityRole() { Name = EnumRole.CSO.ToString(), NormalizedName = EnumRole.CSO.ToString() }

                );
        }
    }
}
