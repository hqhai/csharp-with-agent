using System.Diagnostics;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fsel.Core.Base
{
    public class BaseDbContext : DbContext, IBaseDbContext
    {
        private readonly IMediator _mediator;

        private IDbContextTransaction? _currentTransaction;

        private readonly IList<Entity> _trackEntities;

        public bool HasActiveTransaction => _currentTransaction != null;

        public IDbContextTransaction? GetCurrentTransaction()
        {
            return _currentTransaction;
        }

        public BaseDbContext(DbContextOptions options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _trackEntities = new List<Entity>();
            Debug.WriteLine("BaseDbContext::ctor ->" + GetHashCode());
        }

        public async Task<Guid> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            UpdateSoftDataAsync();

            IExecutionStrategy strategy = Database.CreateExecutionStrategy();

            if (Database.IsInMemory())
            {
                await base.SaveChangesAsync(cancellationToken);
                await DispatchDomainEventsAsync();
                return Guid.NewGuid();
            }

            if (_currentTransaction != null)
            {
                await base.SaveChangesAsync(cancellationToken);
                await DispatchDomainEventsAsync();
                return _currentTransaction.TransactionId;
            }

            return await strategy.ExecuteAsync(async delegate
            {
                IDbContextTransaction? dbContextTransaction = await BeginTransactionAsync().ConfigureAwait(continueOnCapturedContext: false);
                try
                {
                    await base.SaveChangesAsync(cancellationToken);
                    await DispatchDomainEventsAsync();
                    await CommitTransactionAsync(dbContextTransaction).ConfigureAwait(continueOnCapturedContext: false);
                    return dbContextTransaction?.TransactionId ?? Guid.Empty;
                }
                catch (Exception)
                {
                    RollbackTransaction();
                    throw;
                }
            });
        }

        private void UpdateSoftDataAsync()
        {
            var entryMain = ChangeTracker.Entries().FirstOrDefault();

            if (entryMain != null && entryMain.State == EntityState.Modified && bool.TryParse(entryMain.CurrentValues[nameof(Entity.IsDeleted)]?.ToString(), out bool isDeleted) && isDeleted)
            {
                foreach (var entry in ChangeTracker.Entries())
                {
                    entry.CurrentValues[nameof(Entity.IsDeleted)] = entryMain.CurrentValues[nameof(Entity.IsDeleted)];
                    entry.CurrentValues[nameof(Entity.DeletedDate)] = entryMain.CurrentValues[nameof(Entity.DeletedDate)];
                    entry.CurrentValues[nameof(Entity.DeletedUserId)] = entryMain.CurrentValues[nameof(Entity.DeletedUserId)];
                    entry.CurrentValues[nameof(Entity.DeletedFullName)] = entryMain.CurrentValues[nameof(Entity.DeletedFullName)];
                }
            }
        }

        private async Task DispatchDomainEventsAsync()
        {
            IEnumerable<Entity> domainEntities = _trackEntities.Where((Entity x) => x.DomainEvents != null && x.DomainEvents.Any());
            List<INotification> domainEvents = domainEntities.SelectMany((Entity x) => x.DomainEvents).ToList();
            domainEntities.ToList().ForEach(delegate (Entity entity)
            {
                entity.ClearDomainEvents();
            });
            IEnumerable<Task> tasks = ((IEnumerable<INotification>)domainEvents).Select((Func<INotification, Task>)async delegate (INotification domainEvent)
            {
                Console.WriteLine($"Dispatching InternalEvent: {domainEvent.GetType()}");
                await _mediator.Publish(domainEvent);
                Console.WriteLine($"Dispatched InternalEvent: {domainEvent.GetType()}");
            });
            await Task.WhenAll(tasks);
        }

        public async Task<IDbContextTransaction?> BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return null;
            }

            _currentTransaction = await Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction? transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != _currentTransaction)
            {
                throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");
            }

            try
            {
                await SaveChangesAsync();
                transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void TrackEntity(Entity entity)
        {
            _trackEntities.Add(entity);
        }
    }
}
