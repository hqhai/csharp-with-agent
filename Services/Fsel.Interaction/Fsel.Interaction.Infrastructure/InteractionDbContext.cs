using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Core.Base;
using Fsel.Interaction.Domain.Entities;
using Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.SurverQuestionSources;
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
            ArgumentNullException.ThrowIfNull(modelBuilder);
            SeedSurveyQuestions(modelBuilder);

            modelBuilder.ApplyConfiguration(new SurveyQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerSurveyEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<CustomerSurvey> CustomerSurveys { get; set; }

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
            builder.Entity<SurveyQuestion>().HasData
            (
                new SurveyQuestion()
                {
                    Id = Guid.NewGuid(),
                    Question = "Bạn biết đến Fsel từ đâu?",
                    Description = "addd",
                    Icon = "addd",
                    DisplayOrder = 1,
                    Type = EnumSurveyQuestion.FselSource,
                    Answers = FselSourceQuestionSource.FselSourceQuestion
                },
                new SurveyQuestion()
                {
                    Id = Guid.NewGuid(),
                    Question = "Chọn hướng đi của bạn",
                    Description = "addd",
                    Icon = "addd",
                    DisplayOrder = 1,
                    Type = EnumSurveyQuestion.ChooseDirection,
                    Answers = ChooseDirectionQuestionSourse.ChooseDirectionQuestion
                },
                new SurveyQuestion()
                {
                    Id = Guid.NewGuid(),
                    Question = "Tại sao bạn học ngoại ngữ",
                    Description = "addd",
                    Icon = "addd",
                    DisplayOrder = 1,
                    Type = EnumSurveyQuestion.ChooseLanguage,
                    Answers = ChooseLanguageQuestionSourse.ChooseLanguageQuestion
                },
                new SurveyQuestion()
                {
                    Id = Guid.NewGuid(),
                    Question = "Chọn thời gian học tập ",
                    Description = "addd",
                    Icon = "addd",
                    DisplayOrder = 1,
                    Type = EnumSurveyQuestion.StudyTime,
                    Answers = StudyTimeQuestionSourse.StudyTimeQuestion
                },
                new SurveyQuestion()
                {
                    Id = Guid.NewGuid(),
                    Question = "Xác định độ tuổi và giới tính",
                    Description = "addd",
                    Icon = "addd",
                    DisplayOrder = 1,
                    Type = EnumSurveyQuestion.AgeGender,
                    Answers = AgeGenderQuestionSourse.AgeGenderQuestion
                }
            );
        }
    }
}
