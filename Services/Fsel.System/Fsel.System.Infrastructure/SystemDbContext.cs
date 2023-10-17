using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.System.Domain.Entities;
using Fsel.System.Domain.Entities.Configs;
using Fsel.System.Infrastructure.Configs;
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
            SeedQuestBoards(modelBuilder);
            SeedFocusTimeConfig(modelBuilder);
            modelBuilder.ApplyConfiguration(new TeachingCostEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ReferralDiscountConfigConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardConfigConfigConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardConfigConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardStudentConfigConfiguration());
            modelBuilder.ApplyConfiguration(new FeatureAccessTimeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new GameTopicEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GameVocabularyEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GameVocabularyTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FocusTimeConfigEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<FeatureAccessTime> FeatureAccessTimes { get; set; }
        public DbSet<LiveTimeFrame> LiveTimeFrames { get; set; }
        public DbSet<CourseTimeConfig> CourseTimeConfigs { get; set; }
        public DbSet<ForbiddenWord> ForbiddenWords { get; set; }
        public DbSet<TeachingCost> TeachingCosts { get; set; }
        public DbSet<LogAction> LogActions { get; set; }
        public DbSet<ReferralDiscountConfig> ReferralDiscountConfigs { get; set; }
        public DbSet<QuestBoardStudent> QuestBoardStudents { get; set; }
        public DbSet<QuestBoard> QuestBoards { get; set; }
        public DbSet<QuestBoardConfig> QuestBoardConfigs { get; set; }
        public DbSet<GameTopic> GameTopics { get; set; }
        public DbSet<GameVocabulary> GameVocabularies { get; set; }
        public DbSet<GameVocabularyType> GameVocabularyTypes { get; set; }
        public DbSet<FocusTimeConfig> FocusTimeConfigs { get; set; }

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

        private static void SeedQuestBoards(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.QuestBoardFileName);
            var questBoardConfigs = ConvertHelper.DeserializeFromFilePath<IList<QuestBoardConfig>>(path);
            ArgumentNullException.ThrowIfNull(questBoardConfigs);
            builder.Entity<QuestBoardConfig>().HasData(questBoardConfigs);
        }

        private static void SeedFocusTimeConfig(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.FocusTimeFileName);
            var focusTimeConfigs = ConvertHelper.DeserializeFromFilePath<IList<FocusTimeConfig>>(path);
            ArgumentNullException.ThrowIfNull(focusTimeConfigs);
            builder.Entity<FocusTimeConfig>().HasData(focusTimeConfigs);
        }
    }
}
