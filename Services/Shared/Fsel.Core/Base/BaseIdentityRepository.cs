// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Core.Applications.InternalEvents;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fsel.Core.Base
{
    public class BaseIdentityRepository<T, TUser> : IRepository<T> where T : Entity
        where TUser : IdentityUser
    {
        protected readonly AuthContext _authContext;

        protected readonly BaseIdentityDbContext<TUser> _dbBaseContext;

        protected readonly DbSet<T> _dbSet;

        private Guid CurrentUserId => _authContext.CurrentUserId;

        private string CurrentFullName => _authContext.CurrentFullName ?? string.Empty;

        public IUnitOfWork UnitOfWork => _dbBaseContext;

        public IQueryable<T> Queryable => _dbSet.Where((T m) => !m.IsDeleted);

        public BaseIdentityRepository(BaseIdentityDbContext<TUser> dbContext, AuthContext authContext)
        {
            _dbBaseContext = dbContext;
            _dbSet = _dbBaseContext.Set<T>();
            _authContext = authContext;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await _dbSet.SingleOrDefaultAsync((T c) => c.Id == id && !c.IsDeleted).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<T?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await _dbSet.SingleOrDefaultAsync((T c) => c.Id == id && !c.IsDeleted).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, int? siteId = null)
        {
            try
            {
                return await _dbSet.Where((T c) => ids.Contains(c.Id) && !c.IsDeleted).ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual T Add(T newEntity)
        {
            try
            {
                newEntity.CreatedDate = DateTime.Now;
                newEntity.CreatedUserId = CurrentUserId;
                newEntity.CreatedFullName = CurrentFullName;
                newEntity.Id = Guid.NewGuid();
                newEntity.AddDomainEvent(new EntityCreatedEvent<T>(newEntity));
                _dbBaseContext.TrackEntity(newEntity);
                return _dbSet.Add(newEntity).Entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task AddList(IEnumerable<T> newEntities)
        {
            try
            {
                foreach (var newEntity in newEntities)
                {
                    newEntity.CreatedDate = DateTime.Now;
                    newEntity.CreatedUserId = CurrentUserId;
                    newEntity.CreatedFullName = CurrentFullName;
                    newEntity.Id = Guid.NewGuid();
                    newEntity.AddDomainEvent(new EntityCreatedEvent<T>(newEntity));
                    _dbBaseContext.TrackEntity(newEntity);
                }
                await _dbSet.AddRangeAsync(newEntities);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual void UpdateList(IEnumerable<T> updateEntities)
        {
            try
            {
                foreach (var updateEntity in updateEntities)
                {
                    updateEntity.UpdatedDate = DateTime.Now;
                    updateEntity.CreatedUserId = CurrentUserId;
                    updateEntity.CreatedFullName = CurrentFullName;
                    updateEntity.AddDomainEvent(new EntityChangedEvent<T>(updateEntity));
                    _dbBaseContext.TrackEntity(updateEntity);
                }
                _dbSet.UpdateRange(updateEntities);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<bool> AnyAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await _dbSet.AnyAsync((T c) => c.Id == id && !c.IsDeleted).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AnyGuidAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await _dbSet.AnyAsync((T c) => c.Id == id && !c.IsDeleted).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool IsIdsInValid(IEnumerable<Guid> ids, int? siteId = null)
        {
            return ids.Any(id => !_dbSet.Any(f => f.Id == id));
        }

        public virtual Task<bool> DeleteAsync(T deleteEntity)
        {
            try
            {
                deleteEntity.IsDeleted = true;
                deleteEntity.DeletedDate = DateTime.Now;
                deleteEntity.CreatedUserId = CurrentUserId;
                deleteEntity.CreatedFullName = CurrentFullName;
                deleteEntity.AddDomainEvent(new EntityDeletedEvent<T>(deleteEntity));
                _dbBaseContext.TrackEntity(deleteEntity);
                return Task.FromResult(result: true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual T Update(T updateEntity)
        {
            try
            {
                updateEntity.UpdatedDate = DateTime.Now;
                updateEntity.CreatedUserId = CurrentUserId;
                updateEntity.CreatedFullName = CurrentFullName;
                updateEntity.AddDomainEvent(new EntityChangedEvent<T>(updateEntity));
                _dbBaseContext.TrackEntity(updateEntity);
                return _dbSet.Update(updateEntity).Entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task ExecuteTransactionAsync(Func<Task<VoidMethodResult>> action)
        {
            if (_dbBaseContext.Database.IsInMemory() || _dbBaseContext.HasActiveTransaction)
            {
                await action().ConfigureAwait(continueOnCapturedContext: false);
                return;
            }

            IExecutionStrategy strategy = _dbBaseContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async delegate
            {
                using IDbContextTransaction? transaction = await _dbBaseContext.BeginTransactionAsync().ConfigureAwait(continueOnCapturedContext: false);
                if (transaction != null)
                {
                    try
                    {
                        if ((await action().ConfigureAwait(continueOnCapturedContext: false))?.IsOK ?? false)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
