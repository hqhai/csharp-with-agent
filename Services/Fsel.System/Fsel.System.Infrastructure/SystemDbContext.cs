using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Shared.Enums;
using Fsel.System.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.System.Infrastructure
{
    public class SystemDbContext : BaseDbContext
    {
        public SystemDbContext(DbContextOptions<SystemDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            SeedCourselevel(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<LiveTimeFrame> LiveTimeFrames { get; set; }
        public DbSet<CourseTimeConfig> CourseTimeConfigs { get; set; }

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

        private static void SeedCourselevel(ModelBuilder builder)
        {
            builder.Entity<CourseTimeConfig>().HasData
                (
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.A1 },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.A2 },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.B1 },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.B1Plus },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.B2 },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.RFE },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.MS3 },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.MS2 },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.MS1 }
                );
        }
    }
}
