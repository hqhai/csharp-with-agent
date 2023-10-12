namespace Fsel.Cms.PlanetDefender.Infrastructure
{
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Infrastructure.Configs;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
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
            SeedZMatter(modelBuilder);
            modelBuilder.ApplyConfiguration(new StudentGameInfoEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<StudentGameInfo> StudentGameInfos { get; set; }
        public DbSet<QuestBank> QuestBanks { get; set; }
        public DbSet<ZMatter> ZMatters { get; set; }

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
            builder.Entity<ZMatter>().HasData
                (
                    new ZMatter() { Id = Guid.Parse("187c1ce2-23ad-4acc-ba89-649a437c1099"), IsActive = false, Name = "Wind Blade", Code = "WB001", Description = "A sword without a costume? Oh no, look, there's a sharp wind around it!", Usage = "Push 1 meteor, reset meteor duration to maximum",  },
                    new ZMatter() { Id = Guid.Parse("9faaead9-d4de-4aa8-8523-5432fa7f313f"), IsActive = false, Name = "Stop Watch", Code = "SW001", Description = "What's the use of a broken watch?", Usage = "Freeze time within X seconds" },
                    new ZMatter() { Id = Guid.Parse("baecdd2b-39c3-41a5-8240-1a48e37b4f33"), IsActive = false, Name = "Shield", Code = "SH001", Description = "This shield is so beautiful! Wish it was here forever", Usage = "Quantum shield, helps the spacecraft block 1 damage" },
                    new ZMatter() { Id = Guid.Parse("54c17216-a173-4ed6-a6e8-ff7062494705"), IsActive = false, Name = "Supply Kit", Code = "SP001", Description = "The rescue ship is here!", Usage = "Use immediately restores 10% of maximum health (Full will restore shield)" },
                    new ZMatter() { Id = Guid.Parse("0cf0c6b3-1312-473d-bd2a-15dfa88d6052"), IsActive = false, Name = "Power Up", Code = "PU001", Description = "This power...It's strange", Usage = "Restores 20% rage" },
                    new ZMatter() { Id = Guid.Parse("fdc945ba-248e-4db2-96cb-c7d55a7de8c8"), IsActive = false, Name = "Magnetic", Code = "MG001", Description = "With the force of fate, these meteorites need a little help", Usage = "Creates a link between 2 meteorites, answering 1 meteorite correctly will destroy both meteorites (If 1 meteorite answers incorrectly, you can still answer the remaining question to destroy both)" },
                    new ZMatter() { Id = Guid.Parse("192b995e-9cfa-45ee-9f6f-f6fff5803059"), IsActive = false, Name = "Gum Bomp", Code = "GB001", Description = "What kind of bomb sticks like glue?", Usage = "Place the bomb in one location, when the meteorite sticks it will explode, causing the meteorite to stand still for 2 seconds" },
                    new ZMatter() { Id = Guid.Parse("12b08f82-9b0b-4a7a-92d2-10e35b9fea4e"), IsActive = false, Name = "Hacker ID", Code = "HI001", Description = "Whose card is this?", Usage = "Only use when answering a question, immediately display the answer and answer" }
                );
        }
    }
}
