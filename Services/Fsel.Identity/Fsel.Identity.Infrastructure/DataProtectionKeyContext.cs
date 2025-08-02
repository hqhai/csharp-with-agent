using Fsel.Core.Base;
using Fsel.Core.Infrastructure.Tenants;
using MediatR;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Infrastructure
{
    public class DataProtectionKeyContext : BaseDbContext, IDataProtectionKeyContext
    {
        public DataProtectionKeyContext(DbContextOptions<DataProtectionKeyContext> options, IMediator mediator, AuthContext authContext) : base(options, mediator, authContext)
        {
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    }
}
