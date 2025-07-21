// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Ordering.Domain.Entities;
using Fsel.Ordering.Infrastructure.Configs;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Ordering.Infrastructure
{
    public class OrderingReadDbContext : BaseOrderingDbContext
    {
        protected override string Connection => Settings.ReadOnlyConnection;

        public OrderingReadDbContext(DbContextOptions<OrderingReadDbContext> options, IMediator mediator, AuthContext authContext)
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

    public class OrderingDbContext : BaseOrderingDbContext
    {
        public OrderingDbContext(DbContextOptions<OrderingDbContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            base.OnModelCreating(modelBuilder);
            SeedPackages(modelBuilder);
            SeedEvents(modelBuilder);
            SeedProducts(modelBuilder);
        }

        private static void SeedPackages(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.PackageFileName);
            var packages = ConvertHelper.DeserializeFromFilePath<IList<Package>>(path);
            ArgumentNullException.ThrowIfNull(packages);

            var packageTranslations = packages.SelectMany(x => x.Translations).ToList();
            packages.ForEach(x => x.Translations.Clear());

            builder.Entity<Package>().HasData(packages);
            builder.Entity<PackageTranslation>().HasData(packageTranslations);
        }

        private static void SeedEvents(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.EventFileName);
            var events = ConvertHelper.DeserializeFromFilePath<IList<Event>>(path);
            ArgumentNullException.ThrowIfNull(events);

            var eventTranslations = events.SelectMany(x => x.Translations).ToList();
            events.ForEach(x => x.Translations.Clear());
            var packageEvents = events.SelectMany(x => x.PackageEvents).ToList();
            events.ForEach(x => x.PackageEvents.Clear());

            builder.Entity<Event>().HasData(events);
            builder.Entity<EventTranslation>().HasData(eventTranslations);
            builder.Entity<PackageEvent>().HasData(packageEvents);
        }

        private static void SeedProducts(ModelBuilder builder)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ProductFileName);
            var products = ConvertHelper.DeserializeFromFilePath<IList<Product>>(path);
            ArgumentNullException.ThrowIfNull(products);

            var productTranslations = products.SelectMany(x => x.Translations).ToList();
            products.ForEach(x => x.Translations.Clear());
            ArgumentNullException.ThrowIfNull(productTranslations);

            builder.Entity<Product>().HasData(products);
            builder.Entity<ProductTranslation>().HasData(productTranslations);
        }
    }

    public class BaseOrderingDbContext : BaseDbContext
    {
        protected virtual string Connection => Settings.DefaultConnection;

        public BaseOrderingDbContext(DbContextOptions options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PackageEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VoucherEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VoucherPackageEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserVoucherEnityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new OrderTransactionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EventEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PackageEventEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EventTranslationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PackageTranslationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProductTranslationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProductEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<Voucher> Vouchers { get; set; }

        public DbSet<VoucherPackage> VoucherPackages { get; set; }

        public DbSet<UserVoucher> UserVouchers { get; set; }

        public DbSet<UserReferral> UserReferrals { get; set; }
        public DbSet<OrderTransaction> OrderTransactions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<PackageEvent> PackageEvents { get; set; }
        public DbSet<EventTranslation> EventTranslations { get; set; }
        public DbSet<PackageTranslation> PackageTranslations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductTranslation> ProductTranslations { get; set; }
        public DbSet<UserVoucherLock> UserVoucherLocks { get; set; }

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
