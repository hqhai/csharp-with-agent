using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Shared.Constants;
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
            //SeedQuestBoards(modelBuilder);
            modelBuilder.ApplyConfiguration(new TeachingCostEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ReferralDiscountConfigConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardConfigConfigConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardConfigConfiguration());
            modelBuilder.ApplyConfiguration(new QuestBoardStudentConfigConfiguration());
            modelBuilder.ApplyConfiguration(new FeatureAccessTimeConfigConfiguration());
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
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, QuestBoardSettings.QuestBoardFileName);
            var questBoardConfigs = ConvertHelper.DeserializeFromFilePath<IList<QuestBoardConfig>>(path);
            ArgumentNullException.ThrowIfNull(questBoardConfigs);
            builder.Entity<QuestBoardConfig>().HasData(questBoardConfigs);
        }

        private static void SeedCourselevel(ModelBuilder builder)
        {
            /*builder.Entity<CourseTimeConfig>().HasData
                (
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.A1, Id = Guid.Parse("9ceb5cf5-271c-4c53-8d2d-3d273740fccd") },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.A2, Id = Guid.Parse("07312932-5caf-4e01-a670-6cd4aa8650da") },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.B1, Id = Guid.Parse("8dd3c007-775f-4ef6-ad10-efc404c5a2be") },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.B1Plus, Id = Guid.Parse("a0904fa2-fce2-432a-bb85-a3f87c1344b4") },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.B2, Id = Guid.Parse("18e6bccf-1c88-4886-bc71-bec2c556f913") },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.RFE, Id = Guid.Parse("85bc760a-be1e-497d-bf62-2126c6178479") },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.MS3, Id = Guid.Parse("2045c855-e5a8-4818-8bf7-d477d02c1b02") },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.MS2, Id = Guid.Parse("382bae8a-55aa-4da0-8287-71182cb17a7c") },
                    new CourseTimeConfig() { CourseLevel = EnumCourseLevel.MS1, Id = Guid.Parse("e0a504cf-ceb2-415a-ad73-cec5429f0e07") }
                );*/
        }
    }
}
