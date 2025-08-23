// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.IEntities;

    public interface IVersionEntityUpdater<TEntity> where TEntity : Entity, IVersionEntity
    {
        Task UpdateEntity(TEntity entity, TEntity newVersionEntity, Func<IRepository<TEntity>, TEntity, Task<bool>> isNewVersionRequired, Func<TEntity, TEntity, Task> updateEntityAction, Func<TEntity, TEntity, Task>? beforeUpdateAction = null, Func<TEntity, TEntity, Task>? afterUpdateAction = null);
    }
}
