using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Interaction.Domain.Entities;
using Fsel.Interaction.Infrastructure.Configs;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Interaction.Infrastructure
{
    public class InteractionDbContext : BaseDbContext
    {
        public InteractionDbContext(DbContextOptions<InteractionDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            SeedSurveyQuestions(modelBuilder);

            modelBuilder.ApplyConfiguration(new SurveyQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerSurveyEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new InteractionActionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PostEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PostTagEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<CustomerSurvey> CustomerSurveys { get; set; }
        public DbSet<InteractionAction> InteractionActions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<TopicTag> TopicTags { get; set; }

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

        private static void SeedSurveyQuestions(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SurveyQuestionSettings.SurveyQuestionFileName);
            var surveyQuestions = ConvertHelper.DeserializeFromFilePath<IList<SurveyQuestion>>(path);
            ArgumentNullException.ThrowIfNull(surveyQuestions);
            builder.Entity<SurveyQuestion>().HasData(surveyQuestions.ToArray());
        }
    }
}
