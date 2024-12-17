// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure
{
    using Fsel.System.Domain.Entities;
    using Fsel.System.Infrastructure.Configs;
    using Fsel.System.Infrastructure.Configs.Crm;
    using Microsoft.EntityFrameworkCore;

    public class CrmDbContext : DbContext
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.ApplyConfiguration(new CrmLocationEntityTypeConfigConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<CrmLocation> Locations { get; set; }
    }
}
