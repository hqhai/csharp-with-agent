using Fsel.Notification.Domain.Entities;
using Fsel.Notification.Infrastructure.Configs;
using Fsel.Shared.Constants;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Notification.Infrastructure
{
    public class NotificationsDBContext : BaseDbContext
    {
        public NotificationsDBContext(DbContextOptions<NotificationsDBContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
            ChangeTracker.LazyLoadingEnabled = true;
        }

        public DbSet<NotificationMessage> NotificationMessages { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<NotificationRemind> NotificationReminds { get; set; }
        public DbSet<NotificationTypeTranslation> NotificationTypeTranslations { get; set; }
        public DbSet<NotificationMessageTranslation> NotificationMessageTranslations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            base.OnModelCreating(modelBuilder);
            SeedNotificationType(modelBuilder);
            modelBuilder.ApplyConfiguration(new NotificationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationTypeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationRemindEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationTypeTranslationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationMessageTranslationEntityTypeConfiguration());
        }

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

        private static void SeedNotificationType(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.NotificationType);
            var notificationType = ConvertHelper.DeserializeFromFilePath<IList<NotificationType>>(path);
            ArgumentNullException.ThrowIfNull(notificationType);

            var packageTranslations = notificationType.SelectMany(x => x.Translations).ToList();
            notificationType.ForEach(x => x.Translations.Clear());

            builder.Entity<NotificationType>().HasData(notificationType);
            builder.Entity<NotificationTypeTranslation>().HasData(packageTranslations);
        }
    }
}
