using Fsel.Core.Base.Interfaces;
using Fsel.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
