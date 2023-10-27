// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fsel.Core.Base.Interfaces
{
    public interface IBaseDbContext : IUnitOfWork, IDisposable
    {
        IDbContextTransaction? GetCurrentTransaction();

        Task<IDbContextTransaction?> BeginTransactionAsync();

        Task CommitTransactionAsync(IDbContextTransaction? transaction);

        void RollbackTransaction();

        void TrackEntity(Entity entity);
    }
}
