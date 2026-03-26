// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Master.Identity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fsel.Master.Identity.Infrastructure
{
    public class UserMasterReadDbContext : UserMasterBaseDBContext
    {
        protected override string Connection => Settings.ReadOnlyConnection;

        public UserMasterReadDbContext(DbContextOptions<UserMasterReadDbContext> options, IMediator mediator, AuthContext authContext)
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

    public class UserMasterDBContext : UserMasterBaseDBContext
    {
        public UserMasterDBContext(DbContextOptions<UserMasterDBContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }
    }

    public class UserMasterBaseDBContext : BaseIdentityDbContext<MasterUser, MasterRole, Guid, MasterUserClaim, MasterUserRole, MasterUserLogin, MasterRoleClaim, MasterUserToken>
    {
        protected virtual string Connection => Settings.DefaultConnection;

        public UserMasterBaseDBContext(DbContextOptions options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            base.OnModelCreating(builder);

            builder.Entity<MasterRole>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<MasterUser>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<MasterUserToken>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<MasterUserToken>().HasIndex(x => new { x.IsDeleted, x.RefreshToken });
        }

        #region Auth DbSets

        public DbSet<MasterUserToken> MasterUserTokens { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; }

        #endregion Identity DbSets

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
