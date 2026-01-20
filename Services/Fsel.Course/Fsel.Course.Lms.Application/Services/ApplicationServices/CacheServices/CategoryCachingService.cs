// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using System.Collections.Generic;
    using Domain.IRepositories;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Shared.Enums;

    public interface ICategoryCachingService : IEntityCachingService<Category>
    {
        Task<IList<Category>> GetAll(CancellationToken cancellationToken = default);
    }

    public class CategoryCachingService : EntityCachingService<Category>, ICategoryCachingService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryCachingService(ICacheService<Category> cacheService, ICategoryRepository categoryRepository) : base(cacheService)
        {
            _categoryRepository = categoryRepository;
        }

        public override List<string> Tags => new List<string> { "CourseService", "Entity", nameof(Category), };

        public override string Prefix => $"CourseService:Entity:{nameof(Category)}";

        public async Task<IList<Category>> GetAll(CancellationToken cancellationToken = default)
        {
            var subjects = await GetOrSetAsync("all", async (ctx, _) =>
            {
                var categories = await _categoryRepository.ReadQueryable
                    .Where(x => x.Type == EnumTypeCategory.Subject
                                && x.Status == EnumStatus.Active
                                && x.ParentId == null)
                    .ToListAsync(cancellationToken);
                foreach (var category in categories)
                {
                    await LoadChildCategory(category);
                }

                return categories;
            }, token: cancellationToken);
            return subjects;
        }

        private async Task LoadChildCategory(Category category)
        {
            category.Categorys = await _categoryRepository.ReadQueryable
                .Include(x => x.Levels)
                .Where(x => x.ParentId == category.Id && x.Status == EnumStatus.Active)
                .ToListAsync();
            foreach (var child in category.Categorys)
            {
                await LoadChildCategory(child);
            }
        }
    }
}
