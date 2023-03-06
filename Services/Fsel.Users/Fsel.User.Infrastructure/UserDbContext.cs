using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.User.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.User.Infrastructure
{
    public class UserDbContext : IdentityDbContext<Account>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        #region Db Set

        public DbSet<Account> Accounts { get; set; }

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