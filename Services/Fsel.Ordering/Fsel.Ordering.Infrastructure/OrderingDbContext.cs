// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Ordering.Domain.Entities;
using Fsel.Ordering.Domain.Entities.PackageConfigs;
using Fsel.Ordering.Domain.Enums;
using Fsel.Ordering.Infrastructure.Configs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Ordering.Infrastructure
{
    public class OrderingDbContext : BaseDbContext
    {
        public OrderingDbContext(DbContextOptions<OrderingDbContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            SeedPackages(modelBuilder);

            modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PackageEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VoucherEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VoucherPackageEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserVoucherEnityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<Voucher> Vouchers { get; set; }

        public DbSet<VoucherPackage> VoucherPackages { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<UserReferral> UserReferrals { get; set; }

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

        private static void SeedPackages(ModelBuilder builder)
        {
            builder.Entity<Package>().HasData
                (
                    new Package()
                    {
                        Id = Guid.Parse("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                        Code = EnumPackageCode.BASIC,
                        Price = 1000000,
                        Description = new List<PackageConfig>()
                        {
                            new PackageConfig { Content = "Bài giảng , bài tập tên nền tảng E-learning", Status = true },
                            new PackageConfig { Content = "Truy cập bài tập hướng dẫn, và bài thi Unit", Status = true },
                            new PackageConfig { Content = "Diễn đàn", Status = true },
                            new PackageConfig { Content = "Giảng viên nhận xét", Status = false },
                            new PackageConfig { Content = "Truy cập tiết học trực tuyến cho kỹ năng nói với Giảng viên", Status = false },
                        }
                    },
                    new Package()
                    {
                        Id = Guid.Parse("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                        Code = EnumPackageCode.STANDARD,
                        Price = 3000000,
                        Description = new List<PackageConfig>()
                        {
                            new PackageConfig { Content = "Bài giảng , bài tập tên nền tảng E-learning", Status = true },
                            new PackageConfig { Content = "Truy cập bài tập hướng dẫn, và bài thi Unit", Status = true },
                            new PackageConfig { Content = "Diễn đàn", Status = true },
                            new PackageConfig { Content = "Giảng viên nhận xét", Status = true },
                            new PackageConfig { Content = "Truy cập tiết học trực tuyến cho kỹ năng nói với Giảng viên", Status = false },
                        },
                    },
                    new Package()
                    {
                        Id = Guid.Parse("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                        Code = EnumPackageCode.PREMIUM,
                        Price = 10000000,
                        Description = new List<PackageConfig>()
                        {
                            new PackageConfig { Content = "Bài giảng , bài tập tên nền tảng E-learning", Status = true },
                            new PackageConfig { Content = "Truy cập bài tập hướng dẫn, và bài thi Unit", Status = true },
                            new PackageConfig { Content = "Diễn đàn", Status = true },
                            new PackageConfig { Content = "Giảng viên nhận xét", Status = true },
                            new PackageConfig { Content = "Truy cập tiết học trực tuyến cho kỹ năng nói với Giảng viên", Status = true }
                        }
                    }
                );
            ;
        }
    }
}
