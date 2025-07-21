using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Shared.Constants;
using Fsel.System.Domain.Entities;
using Fsel.System.Domain.Entities.BlindBoxs;
using Fsel.System.Domain.Entities.ChatBot;
using Fsel.System.Domain.Entities.Chatbots;
using Fsel.System.Domain.Entities.Configs;
using Fsel.System.Domain.Entities.DailyQuiz;
using Fsel.System.Domain.Entities.QuestBoards;
using Fsel.System.Infrastructure.Configs;
using Fsel.System.Infrastructure.Configs.BlindBoxs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.System.Infrastructure
{
    public class SystemReadDbContext : BaseSystemDbContext
    {
        protected override string Connection => Settings.ReadOnlyConnection;

        public SystemReadDbContext(DbContextOptions<SystemReadDbContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    public class SystemDbContext : BaseSystemDbContext
    {
        public SystemDbContext(DbContextOptions<SystemDbContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ArgumentNullException.ThrowIfNull(modelBuilder);
            SeedQuestBoards(modelBuilder);
            SeedQuestBoardOveralls(modelBuilder);
            SeedFocusTimeConfig(modelBuilder);
            SeedApprovalTimeConfig(modelBuilder);
            SeedTokenConfig(modelBuilder);
            SeedTechieConfig(modelBuilder);
            SeedTechieActionsConfig(modelBuilder);
            SeedDisplayOrderConfig(modelBuilder);
            SeedBlindBox(modelBuilder);
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

        private static void SeedTechieConfig(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.TechieFileName);
            var techies = ConvertHelper.DeserializeFromFilePath<IList<Techie>>(path);
            ArgumentNullException.ThrowIfNull(techies);
            builder.Entity<Techie>().HasData(techies);
        }

        private static void SeedApprovalTimeConfig(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ApprovalTimeFileName);
            var approvalTimeConfigs = ConvertHelper.DeserializeFromFilePath<IList<ApprovalTimeConfig>>(path);
            ArgumentNullException.ThrowIfNull(approvalTimeConfigs);
            builder.Entity<ApprovalTimeConfig>().HasData(approvalTimeConfigs);
        }

        private static void SeedTechieActionsConfig(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.TechieActionFileName);
            var techieActions = ConvertHelper.DeserializeFromFilePath<IList<TechieAction>>(path);
            ArgumentNullException.ThrowIfNull(techieActions);

            var packageTranslations = techieActions.SelectMany(x => x.Translations).ToList();
            techieActions.ForEach(x => x.Translations.Clear());

            builder.Entity<TechieAction>().HasData(techieActions);
            builder.Entity<TechieActionTranslation>().HasData(packageTranslations);
        }

        private static void SeedDisplayOrderConfig(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.DisplayOrderConfig);
            var displayOrderConfigs = ConvertHelper.DeserializeFromFilePath<IList<DisplayOrderConfig>>(path);
            ArgumentNullException.ThrowIfNull(displayOrderConfigs);
            builder.Entity<DisplayOrderConfig>().HasData(displayOrderConfigs);
        }

        private static void SeedBlindBox(ModelBuilder builder)
        {
            var blindBoxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.BlindBox);
            var blindBoxChestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.BlindBoxChest);
            var blindBoxChestConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.BlindBoxChestConfig);
            var blindBoxes = ConvertHelper.DeserializeFromFilePath<IList<BlindBox>>(blindBoxPath);
            var blindBoxChests = ConvertHelper.DeserializeFromFilePath<IList<BlindBoxChest>>(blindBoxChestPath);
            var blindBoxChestConfigs = ConvertHelper.DeserializeFromFilePath<IList<BlindBoxChestConfig>>(blindBoxChestConfigPath);
            ArgumentNullException.ThrowIfNull(blindBoxes);
            ArgumentNullException.ThrowIfNull(blindBoxChests);
            ArgumentNullException.ThrowIfNull(blindBoxChestConfigs);

            builder.Entity<BlindBox>().HasData(blindBoxes);
            builder.Entity<BlindBoxChest>().HasData(blindBoxChests);
            builder.Entity<BlindBoxChestConfig>().HasData(blindBoxChestConfigs);
        }
    }

    public class BaseSystemDbContext : BaseDbContext
    {
        protected virtual string Connection => Settings.DefaultConnection;

        public BaseSystemDbContext(DbContextOptions options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
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
            modelBuilder.ApplyConfiguration(new LuckyTicketEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new FselRatingEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CourseTargetConfigEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new CourseSuggestConfigEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new BannerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BannerScopeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BannerImageEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BannerSettingEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BannerStudentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TokenHistoryTranslationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DisplayOrderConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BlindBoxChestConfigEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new BlindBoxChestEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new BlindBoxHistoryEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new BlindBoxUserEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new DailyQuizHistoryEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new DailyQuizAnswerEntityTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new DictionaryEntityTypeConfiguration());
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
        public DbSet<ChatbotConfig> ChatbotConfigs { get; set; }
        public DbSet<ChatbotSkillConfig> ChatbotSkillConfigs { get; set; }
        public DbSet<ChatbotTokenConfigs> ChatbotTokenConfigs { get; set; }
        public DbSet<ChatBot> ChatBots { get; set; }
        public DbSet<Techie> Techie { get; set; }
        public DbSet<TechieAction> TechieActions { get; set; }
        public DbSet<StudentTechie> StudentTechies { get; set; }
        public DbSet<UserConfig> UserConfigs { get; set; }
        public DbSet<LuckyTicket> LuckyTickets { get; set; }
        public DbSet<CourseTargetConfig> CourseTargetConfigs { get; set; }
        public DbSet<CourseSuggestConfig> CourseSuggestConfigs { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<BannerSetting> BannerSettings { get; set; }
        public DbSet<BannerScope> BannerScopes { get; set; }
        public DbSet<BannerImage> BannerImages { get; set; }
        public DbSet<BannerStudent> BannerStudents { get; set; }
        public DbSet<TokenHistory> TokenHistories { get; set; }
        public DbSet<TokenHistoryTranslation> TokenHistoryTranslations { get; set; }
        public DbSet<FselRating> FselRatings { get; set; }
        public DbSet<BlindBox> BlindBoxes { get; set; }
        public DbSet<BlindBoxChest> BlindBoxChests { get; set; }
        public DbSet<BlindBoxChestConfig> BlindBoxChestConfigs { get; set; }
        public DbSet<BlindBoxHistory> BlindBoxHistories { get; set; }
        public DbSet<BlindBoxUser> BlindBoxUsers { get; set; }
        public DbSet<DailyQuizQuestion> DailyQuizQuestions { get; set; }
        public DbSet<DailyQuizAnswer> DailyQuizAnswers { get; set; }
        public DbSet<DailyQuizHistory> DailyQuizHistories { get; set; }
        public DbSet<DailyQuizWinner> DailyQuizWinners { get; set; }
        public DbSet<Dictionary> Dictionaries { get; set; }
        public DbSet<UnknownWord> UnknownWords { get; set; }

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
