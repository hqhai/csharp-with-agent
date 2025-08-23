// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IEntities;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class EntityVersionUpdater<TEntity> : IVersionEntityUpdater<TEntity> where TEntity : Entity, IVersionEntity
    {
        private IRepository<TEntity> _repository;

        public EntityVersionUpdater(IRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public async Task UpdateEntity(TEntity entity, TEntity newVersionEntity, Func<IRepository<TEntity>, TEntity, Task<bool>> isNewVersionRequired, Func<TEntity, TEntity, Task>? updateEntityAction, Func<TEntity, TEntity, Task>? beforeUpdateAction = null, Func<TEntity, TEntity, Task>? afterUpdateAction = null)
        {
            TEntity entity2 = entity;
            TEntity newVersionEntity2 = newVersionEntity;
            Func<TEntity, TEntity, Task> updateEntityAction2 = updateEntityAction;
            ArgumentNullException.ThrowIfNull(entity2, "entity");
            ArgumentNullException.ThrowIfNull(newVersionEntity2, "newVersionEntity");
            ArgumentNullException.ThrowIfNull(isNewVersionRequired, "isNewVersionRequired");
            ArgumentNullException.ThrowIfNull(updateEntityAction2, "updateEntityAction");
            if (beforeUpdateAction != null)
            {
                await beforeUpdateAction(entity2, newVersionEntity2);
            }

            if (!(await isNewVersionRequired(_repository, entity2).ConfigureAwait(continueOnCapturedContext: false)))
            {
                await _repository.ExecuteTransactionAsync(async delegate
                {
                    await updateEntityAction2(entity2, newVersionEntity2);
                    _repository.Update(entity2);
                    await _repository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(continueOnCapturedContext: false);
                    return new VoidMethodResult();
                });
            }
            else
            {
                TEntity latestVersionEntity = entity2;
                if (entity2.VersionStatus != EnumVersionStatus.LastVersion)
                {
                    DetachAllEntities(entity2);
                    latestVersionEntity = await _repository.Queryable.Where((TEntity x) => x.OriginalId == entity2.OriginalId && (int)x.VersionStatus == 1).FirstOrDefaultAsync();
                }

                ArgumentNullException.ThrowIfNull(latestVersionEntity, "latestVersionEntity");
                await _repository.ExecuteTransactionAsync(async delegate
                {
                    latestVersionEntity.VersionStatus = EnumVersionStatus.OldVersion;
                    newVersionEntity2.VersionStatus = EnumVersionStatus.LastVersion;
                    newVersionEntity2.Version = latestVersionEntity.Version + 1;
                    newVersionEntity2.OriginalId = latestVersionEntity.OriginalId;
                    _repository.Update(latestVersionEntity);
                    _repository.Add(newVersionEntity2);
                    await _repository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(continueOnCapturedContext: false);
                    return new VoidMethodResult();
                });
            }

            if (afterUpdateAction != null)
            {
                await afterUpdateAction(entity2, newVersionEntity2);
            }
        }

        private void DetachAllEntities(object entity)
        {
            EntityEntry entityEntry = _repository.DbContext.Entry(entity);
            if (entityEntry.State == EntityState.Detached)
            {
                return;
            }

            entityEntry.State = EntityState.Detached;
            foreach (NavigationEntry navigation in entityEntry.Navigations)
            {
                if (navigation.CurrentValue is IEnumerable<object> enumerable)
                {
                    foreach (object item in enumerable)
                    {
                        DetachAllEntities(item);
                    }
                }
                else if (navigation.CurrentValue != null)
                {
                    DetachAllEntities(navigation.CurrentValue);
                }
            }
        }
    }
}
