using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure
{
    public class CourseDbContext : BaseDbContext
    {
        public CourseDbContext(DbContextOptions<CourseDbContext> options, IMediator mediator) : base(options, mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PlacementTestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LessonEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        #region Db Set
        public DbSet<PlacementTest> PlacementTests { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        #endregion

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
