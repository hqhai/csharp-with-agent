// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Entities;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.EntityFrameworkCore;

    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : Entity
    {
        private readonly UserDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(UserDbContext context)
        {
            if (context == null)
            {
                ArgumentNullException.ThrowIfNull(context);
            }
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<PagingItemsModel<EntityModel>> SearchCreatedUsersInfoAsync(BaseQueryModel baseQuery)
        {
            ArgumentNullException.ThrowIfNull(baseQuery);

            var query = _dbSet
                .Select(x => new EntityModel()
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    DeletedDate = x.DeletedDate,
                    DeletedFullName = x.DeletedFullName,
                    DeletedUserId = x.DeletedUserId,
                    IsDeleted = x.IsDeleted,
                    UpdatedDate = x.UpdatedDate,
                    UpdatedFullName = x.UpdatedFullName,
                    UpdatedUserId = x.UpdatedUserId
                });

            if (!string.IsNullOrEmpty(baseQuery.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.CreatedFullName) && p.CreatedFullName.Contains(baseQuery.Keyword));
            }

            var results = await query.ToListAsync(CancellationToken.None);

            results = results.DistinctBy(x => x.CreatedUserId).ToList();

            int totalItem = results.Count;

            var lists = results
                    .ApplySortAndPaging(baseQuery)
                    .ToList();

            return new PagingItemsModel<EntityModel>(lists, baseQuery, totalItem);
        }
    }
}
