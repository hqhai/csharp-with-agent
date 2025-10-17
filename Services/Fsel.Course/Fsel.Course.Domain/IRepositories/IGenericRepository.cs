// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Entities;
    using Fsel.Shared.Models.ShareModels;

    public interface IGenericRepository<TEntity> where TEntity : Entity
    {
        Task<PagingItemsModel<EntityModel>> SearchCreatedUsersInfoAsync(BaseQueryModel baseQuery);
    }
}
