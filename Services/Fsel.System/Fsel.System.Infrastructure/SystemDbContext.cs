using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Shared.Constants;
using Fsel.System.Domain.Entities;
using Fsel.System.Domain.Entities.ChatBot;
using Fsel.System.Domain.Entities.Chatbots;
using Fsel.System.Domain.Entities.Configs;
using Fsel.System.Domain.Entities.QuestBoards;
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
            SeedQuestBoardOveralls(modelBuilder);
            SeedFocusTimeConfig(modelBuilder);
            SeedApprovalTimeConfig(modelBuilder);
            SeedTokenConfig(modelBuilder);
            modelBuilder.ApplyConfiguration(new TeachingCostEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ReferralDiscountConfigConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardConfigConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardStudentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardOverallEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardOverallStudentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FeatureAccessTimeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new GameTopicEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GameVocabularyEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GameVocabularyTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FocusTimeConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GameVocabularyPlatformEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalTimeEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new TokenConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LocationEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new SchoolEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ChatbotConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ChatbotSkillConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ChatbotTokenConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ErrorReportEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TokenHistoryEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ChatBotEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TechieEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TechieActionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentTechieEntityTypeConfiguration());
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
        public DbSet<QuestBoardOverall> QuestBoardOveralls { get; set; }
        public DbSet<QuestBoardOverallStudent> QuestBoardOverallStudents { get; set; }
        public DbSet<GameTopic> GameTopics { get; set; }
        public DbSet<GameVocabulary> GameVocabularies { get; set; }
        public DbSet<GameVocabularyType> GameVocabularyTypes { get; set; }
        public DbSet<FocusTimeConfig> FocusTimeConfigs { get; set; }
        public DbSet<GameVocabularyPlatform> GameVocabularyPlatforms { get; set; }
        public DbSet<ApprovalTimeConfig> ApprovalTimeConfigs { get; set; }
        public DbSet<TokenConfig> TokenConfigs { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<ErrorReport> ErrorReports { get; set; }
        public DbSet<TokenHistory> TokenHistories { get; set; }
        public DbSet<ChatbotConfig> ChatbotConfigs { get; set; }
        public DbSet<ChatbotSkillConfig> ChatbotSkillConfigs { get; set; }
        public DbSet<ChatbotTokenConfigs> ChatbotTokenConfigs { get; set; }
        public DbSet<ChatBot> ChatBots { get; set; }
        public DbSet<Techie> Techie { get; set; }
        public DbSet<TechieAction> TechieActions { get; set; }
        public DbSet<StudentTechie> StudentTechies { get; set; }

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
            var questBoards = ConvertHelper.DeserializeFromFilePath<IList<QuestBoard>>(path);
            ArgumentNullException.ThrowIfNull(questBoards);
            builder.Entity<QuestBoard>().HasData(questBoards);
        }

        private static void SeedQuestBoardOveralls(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.QuestBoardOverallFileName);
            var questBoardOveralls = ConvertHelper.DeserializeFromFilePath<IList<QuestBoardOverall>>(path);
            ArgumentNullException.ThrowIfNull(questBoardOveralls);
            builder.Entity<QuestBoardOverall>().HasData(questBoardOveralls);
        }

        private static void SeedFocusTimeConfig(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.FocusTimeFileName);
            var focusTimeConfigs = ConvertHelper.DeserializeFromFilePath<IList<FocusTimeConfig>>(path);
            ArgumentNullException.ThrowIfNull(focusTimeConfigs);
            builder.Entity<FocusTimeConfig>().HasData(focusTimeConfigs);
        }

        private static void SeedTokenConfig(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.TokenConfig);
            var tokenConfigs = ConvertHelper.DeserializeFromFilePath<IList<TokenConfig>>(path);
            Console.WriteLine(path.Serialize());
            Console.WriteLine(tokenConfigs.Serialize());
            ArgumentNullException.ThrowIfNull(tokenConfigs);
            builder.Entity<TokenConfig>().HasData(tokenConfigs);
        }

        private static void SeedApprovalTimeConfig(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ApprovalTimeFileName);
            var approvalTimeConfigs = ConvertHelper.DeserializeFromFilePath<IList<ApprovalTimeConfig>>(path);
            ArgumentNullException.ThrowIfNull(approvalTimeConfigs);
            builder.Entity<ApprovalTimeConfig>().HasData(approvalTimeConfigs);
        }
    }
}
