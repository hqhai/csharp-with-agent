// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using CacheServices;
    using Domain.Entities;
    using Domain.Enums;
    using Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using Shared.Enums;

    public interface ICategoryService
    {
        Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Category?> GetProgramContainPtBySelectedProject(Guid projectId, CancellationToken cancellationToken = default);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryCachingService _categoryCachingService;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryCachingService categoryCachingService, ICategoryRepository categoryRepository)
        {
            _categoryCachingService = categoryCachingService;
            _categoryRepository = categoryRepository;
        }

        public async Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var categoryFound = await _categoryCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var category = await _categoryRepository.ReadQueryable
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (category == null)
                {
                    return null;
                }

                await LoadChildren(category);

                return category;
            }, token: cancellationToken);

            return categoryFound;
        }

        public async Task<Category?> GetProgramContainPtBySelectedProject(Guid projectId, CancellationToken cancellationToken = default)
        {
            var categoryFound = await GetCategoryAsync(projectId, cancellationToken);
            if (categoryFound == null)
            {
                return null;
            }

            var programs = categoryFound.Categorys.Where(x => x.Type == EnumTypeCategory.Program).ToList();

            var programFound = programs.FirstOrDefault(x => x.TestMode == EnumTestMode.Default);
            if (programFound != null)
            {
                return programFound;
            }

            programFound = programs.FirstOrDefault(x => x.TestMode == EnumTestMode.Custom);
            return programFound;
        }


        private async Task LoadChildren(Category category)
        {
            category.Categorys = await _categoryRepository.ReadQueryable.Where(x => x.ParentId == category.Id).ToListAsync();

            foreach (var child in category.Categorys)
            {
                await LoadChildren(child);
            }
        }
    }
}
