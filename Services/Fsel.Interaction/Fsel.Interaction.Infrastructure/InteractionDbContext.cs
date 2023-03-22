using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Interaction.Domain.Entities;
using Fsel.Interaction.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Interaction.Infrastructure
{
    public class InteractionDbContext : BaseDbContext
    {
        public InteractionDbContext(DbContextOptions<InteractionDbContext> options, IMediator mediator) : base(options, mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SurveyQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerSurveyEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<CustomerSurvey> CustomerSurveys { get; set; }

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
    }
}
