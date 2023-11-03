namespace Fsel.Cms.PlanetDefender.Infrastructure
{
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Infrastructure.Configs;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class CmsPlanetDefenderDbContext : BaseDbContext
    {
        public CmsPlanetDefenderDbContext(DbContextOptions<CmsPlanetDefenderDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            SeedWheelOfBuff(modelBuilder);
            SeedZMatter(modelBuilder);
            SeedGameplayRuleConfigs(modelBuilder);
            SeedSpaceShip(modelBuilder);
            StudentTagName(modelBuilder);

            modelBuilder.ApplyConfiguration(new StudentGameInfoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new WheelOfBuffEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GameplayTimeConfigEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GameHistoryEntityTypeConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<StudentGameInfo> StudentGameInfos { get; set; }
        public DbSet<ZMatter> ZMatters { get; set; }
        public DbSet<GameplayTimeConfig> GameplayTimeConfigs { get; set; }
        public DbSet<GameplayRuleConfig> GameplayRuleConfigs { get; set; }
        public DbSet<WheelOfBuff> WheelOfBuffs { get; set; }
        public DbSet<SpaceShip> SpaceShips { get; set; }
        public DbSet<GameHistory> GameHistories { get; set; }
        public DbSet<AvatarImage> AvatarImages { get; set; }
        public DbSet<StudentSpaceShip> StudentSpaceShips { get; set; }
        public DbSet<StudentTagName> StudentTagNames { get; set; }

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

        private static void SeedZMatter(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ZMatter);
            var zMatters = ConvertHelper.DeserializeFromFilePath<IList<ZMatter>>(path);
            ArgumentNullException.ThrowIfNull(zMatters);
            builder.Entity<ZMatter>().HasData(zMatters);
        }

        private static void SeedSpaceShip(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SpaceShip);
            var spaceShips = ConvertHelper.DeserializeFromFilePath<IList<SpaceShip>>(path);
            ArgumentNullException.ThrowIfNull(spaceShips);
            builder.Entity<SpaceShip>().HasData(spaceShips);
        }

        private static void SeedGameplayRuleConfigs(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.GameplayRuleConfig);
            var gameplayRuleConfigs = ConvertHelper.DeserializeFromFilePath<IList<GameplayRuleConfig>>(path);
            ArgumentNullException.ThrowIfNull(gameplayRuleConfigs);
            builder.Entity<GameplayRuleConfig>().HasData(gameplayRuleConfigs);
        }

        private static void SeedWheelOfBuff(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.WheelOfBuffType);
            var wheelOfBuffConfigs = ConvertHelper.DeserializeFromFilePath<IList<WheelOfBuff>>(path);
            ArgumentNullException.ThrowIfNull(wheelOfBuffConfigs);
            builder.Entity<WheelOfBuff>().HasData(wheelOfBuffConfigs);
        }

        private static void StudentTagName(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.StudentTagName);
            var studentTagNames = ConvertHelper.DeserializeFromFilePath<IList<StudentTagName>>(path);
            ArgumentNullException.ThrowIfNull(studentTagNames);
            builder.Entity<StudentTagName>().HasData(studentTagNames);
        }
    }
}
