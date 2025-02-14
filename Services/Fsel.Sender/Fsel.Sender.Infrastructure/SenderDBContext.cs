using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Sender.Domain.Entities;
using Fsel.Sender.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Sender.Infrastructure
{
    public class SenderDBContext : BaseDbContext
    {
        public SenderDBContext(DbContextOptions<SenderDBContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.ApplyConfiguration(new MessageHistoryEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<MessageHistory> MessageHistories { get; set; }

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
