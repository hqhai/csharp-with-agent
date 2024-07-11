// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure
{
    using Fsel.System.Domain.Entities;
    using Fsel.System.Infrastructure.ValueSettings;
    using Microsoft.EntityFrameworkCore;

    public class CrmDbContext : DbContext
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
        {
        }

        protected CrmDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=10.0.0.28;Initial Catalog=Web_CRM_Rs_Formation_Data;User Id=sa;Password=Fsela@2023!@#$;TrustServerCertificate=true;");
        }
    }
}
