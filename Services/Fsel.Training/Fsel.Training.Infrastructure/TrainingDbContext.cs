// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure
{
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Training.Infrastructure.Configs;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Fsel.Training.Doman.Entities;

    public class TrainingDbContext : BaseDbContext
    {
        public TrainingDbContext(DbContextOptions<TrainingDbContext> options, IMediator mediator) : base(options, mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.ApplyConfiguration(new ClassEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClassStudentEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassStudent> ClassStudents { get; set; }

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
