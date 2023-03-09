using Fsel.Common.ActionResults;
using Fsel.Core.Applications.InternalEvents;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;

namespace Fsel.Core.Base
{
    public class BaseRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly BaseDbContext _dbBaseContext;

        protected readonly DbSet<T> _dbSet;

        //public int CurrentUserId => _authContext.CurrentUserId;

        //public string CurrentUsername => _authContext.CurrentUsername;

        public IUnitOfWork UnitOfWork => _dbBaseContext;

        public IQueryable<T> Queryable => _dbSet.Where((T m) => !m.IsDeleted);

        public BaseRepository(BaseDbContext dbContext)
        {
            _dbBaseContext = dbContext;
            _dbSet = _dbBaseContext.Set<T>();
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

        public virtual T Add(T newEntity)
        {
            try
            {
                newEntity.CreatedDate = DateTime.Now;

                //newEntity.CreatedUserId = _authContext.CurrentUserId;
                //newEntity.CreatedUserName = _authContext.CurrentUsername;
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
                //deleteEntity.DeletedUserId = _authContext.CurrentUserId;
                //deleteEntity.DeletedUserName = _authContext.CurrentUsername;
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
                //updateEntity.UpdatedUserId = _authContext.CurrentUserId;
                //updateEntity.UpdatedUserName = _authContext.CurrentUsername;
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
            }).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}