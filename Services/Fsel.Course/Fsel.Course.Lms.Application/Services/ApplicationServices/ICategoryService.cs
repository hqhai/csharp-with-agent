// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using CacheServices;
    using Domain.Entities;
    using Domain.Enums;
    using Domain.IRepositories;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Microsoft.EntityFrameworkCore;

    public interface ICategoryService
    {
        Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Category?> GetProgramContainPtBySelectedProject(Guid projectId, CancellationToken cancellationToken = default);

        Task<IList<SkillScores>> GetDefaultSkillScoresAsync(Guid? programId, CancellationToken cancellationToken = default);

        Task<Level?> LoadPreviousOrMinLevelAsync(Guid programId, Guid levelId);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryCachingService _categoryCachingService;
        private readonly IProgramSkillScoresCachingService _programSkillScoresCachingService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISkillLevelRepository _skillLevelRepository;
        private readonly ILevelRepository _levelRepository;

        public CategoryService(
            ICategoryCachingService categoryCachingService,
            IProgramSkillScoresCachingService programSkillScoresCachingService,
            ICategoryRepository categoryRepository,
            ISkillLevelRepository skillLevelRepository,
            ILevelRepository levelRepository)
        {
            _categoryCachingService = categoryCachingService;
            _programSkillScoresCachingService = programSkillScoresCachingService;
            _categoryRepository = categoryRepository;
            _skillLevelRepository = skillLevelRepository;
            _levelRepository = levelRepository;
        }

        public async Task<IList<SkillScores>> GetDefaultSkillScoresAsync(Guid? programId, CancellationToken cancellationToken = default)
        {
            if (programId == null)
            {
                return new List<SkillScores>();
            }
            return await _programSkillScoresCachingService.GetOrSetAsync(programId.Value.ToString(), async (ctx, _) =>
            {
                var skills = await _skillLevelRepository.GetDefaultSkillsByProgramIdAsync(programId);
                return skills.Select(x => new SkillScores
                {
                    SkillId = x.Id,
                    SkillName = x.Name,
                    SkillFilePath = x.FilePath,
                }).ToList();
            }, token: cancellationToken);
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
            var programFound = await _categoryRepository.ReadQueryable.Where(x => x.Id == projectId)
                .FirstOrDefaultAsync(cancellationToken);

            if (programFound == null)
            {
                return null;
            }

            if (programFound.TestMode is EnumTestMode.Not or EnumTestMode.Custom)
            {
                return programFound;
            }

            if (programFound.ParentId == null)
            {
                return null;
            }

            var parent = await _categoryCachingService.GetOrSetAsync(programFound.ParentId.Value.ToString(), async (ctx, _) =>
            {
                return await _categoryRepository.ReadQueryable
                    .Where(x => x.Id == programFound.ParentId.Value)
                    .Include(x => x.Categorys)
                    .FirstOrDefaultAsync(cancellationToken);
            }, token: cancellationToken);

            return parent?.Categorys?.Where(x => x.TestMode == EnumTestMode.Custom && x.IsTestDefault).FirstOrDefault();
        }

        private async Task LoadChildren(Category category)
        {
            category.Categorys = await _categoryRepository.ReadQueryable.Where(x => x.ParentId == category.Id).ToListAsync();

            foreach (var child in category.Categorys)
            {
                await LoadChildren(child);
            }
        }

        public async Task<Level?> LoadPreviousOrMinLevelAsync(Guid programId, Guid levelId)
        {
            var currentLevel = await _levelRepository.ReadQueryable
                .Where(x => x.ProgramId == programId && x.Id == levelId)
                .FirstOrDefaultAsync();

            if (currentLevel == null)
            {
                return await LoadMinLevelAsync(programId);
            }
            var emailLevel = await _levelRepository.ReadQueryable
                .Where(x => x.ProgramId == programId
                         && x.LevelOrder < currentLevel.LevelOrder)
                .OrderByDescending(x => x.LevelOrder)
                .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .FirstOrDefaultAsync();

            return emailLevel ?? currentLevel;
        }

        public async Task<Level?> LoadMinLevelAsync(Guid programId)
        {
            return await _levelRepository.ReadQueryable
                .Where(x => x.ProgramId == programId)
                .OrderBy(x => x.LevelOrder)
                .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .FirstOrDefaultAsync();
        }
    }
}
