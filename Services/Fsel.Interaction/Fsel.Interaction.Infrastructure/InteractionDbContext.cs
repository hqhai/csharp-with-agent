// Copyright (c) Atlantic. All rights reserved.

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
            SeedSurveyQuestBoard(modelBuilder);

            modelBuilder.ApplyConfiguration(new SurveyQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerSurveyEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new InteractionActionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CommentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PostEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PostTagEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentReviewDetailEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentReviewEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SupportQuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SupportTicketEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FlagEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SurveyQuestionTranslationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerSurveyGroupEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SurveyConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserSurveyAssignmentEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<SurveyQuestionTranslation> SurveyQuestionTranslations { get; set; }
        public DbSet<CustomerSurvey> CustomerSurveys { get; set; }
        public DbSet<InteractionAction> InteractionActions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<TopicTag> TopicTags { get; set; }
        public DbSet<StudentReview> StudentReviews { get; set; }
        public DbSet<StudentReviewDetail> StudentReviewDetails { get; set; }
        public DbSet<SupportQuestion> SupportQuestions { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportCategory> SupportCategorys { get; set; }
        public DbSet<Flag> Flags { get; set; }
        public DbSet<CustomerSurveyGroup> CustomerSurveyGroups { get; set; }
        public DbSet<SurveyConfig> SurveyConfigs { get; set; }
        public DbSet<UserSurveyAssignment> UserSurveyAssignments { get; set; }

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

        /*private static void SeedSurveyQuestions(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SurveyQuestionFileName);
            var surveyQuestions = ConvertHelper.DeserializeFromFilePath<IList<SurveyQuestion>>(path);
            ArgumentNullException.ThrowIfNull(surveyQuestions);
            builder.Entity<SurveyQuestion>().HasData(surveyQuestions);
        }*/

        private static void SeedSurveyQuestions(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SurveyQuestionFileName);
            var surveyQuestions = ConvertHelper.DeserializeFromFilePath<IList<SurveyQuestion>>(path);
            ArgumentNullException.ThrowIfNull(surveyQuestions);

            var surveyQuestionTranslations = surveyQuestions.SelectMany(x => x.Translations).ToList();
            surveyQuestions.ForEach(x => x.Translations.Clear());

            builder.Entity<SurveyQuestion>().HasData(surveyQuestions);
            builder.Entity<SurveyQuestionTranslation>().HasData(surveyQuestionTranslations);
        }

        private static void SeedSurveyQuestBoard(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SurveyQuestBoardFileName);
            var entities = ConvertHelper.DeserializeFromFilePath<IList<SurveyConfig>>(path);
            ArgumentNullException.ThrowIfNull(entities);
            builder.Entity<SurveyConfig>().HasData(entities);
        }
    }
}
