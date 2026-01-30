using System;
using System.Threading;
using System.Threading.Tasks;
using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.System.Domain.Entities;
using Fsel.System.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.System.Infrastructure
{
    public class PostgreDbContext : BaseDbContext
    {
        public PostgreDbContext(DbContextOptions<PostgreDbContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new DictionaryAIEntityTypeConfiguration());
        }

        public DbSet<DictionaryAI> DictionaryAIs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile(Settings.SettingFileName)
                    .Build();
                optionsBuilder.UseNpgsql(
                    configuration.GetConnectionString("PostgreConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MaxBatchSize(100);
                    });
            }
        }
    }
}
