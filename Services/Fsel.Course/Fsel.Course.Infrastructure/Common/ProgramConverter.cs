// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Programs;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Concurrent;
    using System.Text.RegularExpressions;

    public class ProgramConverter
    {
        private readonly ILevelRepository _levelRepository;
        private readonly IMapper _mapper;
        private readonly ISkillRepository _skillRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProgramConverter(ILevelRepository levelRepository,
                                IMapper mapper,
                                ISkillRepository skillRepository,
                                ICategoryRepository categoryRepository)
        {
            _levelRepository = levelRepository;
            _mapper = mapper;
            _skillRepository = skillRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<bool>> LevelHandler(UpdateLevelCommandModel levelRequest, Category category, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(levelRequest);
            ArgumentNullException.ThrowIfNull(category);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            Regex regexName = new Regex("^[A-Za-z0-9 ]$");
            Regex regexCode = new Regex("^[A-Z0-9_]$");

            #region Validate
            if (string.IsNullOrEmpty(levelRequest.Name) || (!string.IsNullOrEmpty(levelRequest.Name) && regexName.IsMatch(levelRequest.Name)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(levelRequest.Name), levelRequest.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(levelRequest.Code) || (!string.IsNullOrEmpty(levelRequest.Code) && regexCode.IsMatch(levelRequest.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.CodeNotValid), nameof(levelRequest.Code), levelRequest.Code);
                return methodResult;
            }

            var checkCode = await _levelRepository.Queryable.AnyAsync(x => (levelRequest.Id.HasValue ? x.Id != levelRequest.Id : !levelRequest.Id.HasValue) && x.Code == levelRequest.Code!.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(levelRequest.Code), levelRequest.Code);
                return methodResult;
            }

            var skillCount = await _skillRepository.Queryable.CountAsync(x => levelRequest.SkillIds != null && levelRequest.SkillIds.Contains(x.Id), cancellationToken);
            if (levelRequest.SkillIds != null && skillCount != levelRequest.SkillIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(levelRequest.SkillIds), levelRequest.SkillIds);
                return methodResult;
            }

            #endregion

            Level level = new Level();

            if (levelRequest.Id.HasValue)
            {
                var checkLevel = category.Levels.FirstOrDefault(x => x.Id == levelRequest.Id);
                if (checkLevel == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(levelRequest), levelRequest.Id);
                    return methodResult;
                }

                level = checkLevel;
            }

            _mapper.Map(levelRequest, level);
            SkillHandler(levelRequest.SkillIds, level);
            if (!level.IsValid())
            {
                methodResult.AddErrorBadRequest(level.ErrorMessages);
                return methodResult;
            }

            if (!levelRequest.Id.HasValue)
            {
                category.Levels.Add(level);
            }

            return methodResult;
        }

        private static VoidMethodResult SkillHandler(IList<Guid>? skillIds, Level level)
        {
            ArgumentNullException.ThrowIfNull(skillIds);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (level.SkillLevels.Any())
            {
                var skillLevelIds = level.SkillLevels.Select(x => x.SkillId).ToList();
                var duplicateIds = skillLevelIds.Intersect(skillIds).ToList();

                skillIds = skillIds.Where(x => !duplicateIds.Contains(x)).ToList();

                var deleteSkillLevelIds = skillLevelIds.Where(x => !duplicateIds.Contains(x)).ToList();
                if (deleteSkillLevelIds.Any())
                {
                    var itemsToRemove = level.SkillLevels
                                             .Where(x => deleteSkillLevelIds.Contains(x.SkillId))
                                             .ToList();

                    foreach (var item in itemsToRemove)
                    {
                        level.SkillLevels.Remove(item);
                    }
                }
            }

            if (skillIds.Any())
            {
                var skillLevels = level.SkillLevels.Any() ? level.SkillLevels.ToList() : new List<SkillLevel>();

                var newSkillLevels = skillIds?.Select(x => new SkillLevel
                {
                    SkillId = x
                }).ToList() ?? new List<SkillLevel>();

                skillLevels.AddRange(newSkillLevels);
                level.SkillLevels = skillLevels;
            }

            return methodResult;
        }

        public async Task AddChildentCategory(IList<CategoryTreeModel> parentCategories, CancellationToken cancellationToken)
        {
            var parentCategoryIds = parentCategories.Select(x => x.Data).ToList();
            var childentCategories = await _categoryRepository.Queryable
                                                              .WhereBulkContains(parentCategoryIds, x => x.ParentId)
                                                              .Where(x => x.Status != EnumStatus.Archive)
                                                              .ToListAsync(cancellationToken);

            if (childentCategories == null || !childentCategories.Any())
            {
                return;
            }

            ConcurrentStack<CategoryTreeModel> competitionEvents = new ConcurrentStack<CategoryTreeModel>();

            Parallel.ForEach(parentCategories, parentCategory =>
            {
                var childentWithEventParents = childentCategories.Where(x => x.ParentId == parentCategory.Data).ToList();
                var parentCategoryChildents = _mapper.Map<IList<CategoryTreeModel>>(childentWithEventParents);
                parentCategory.Children = parentCategoryChildents;

                competitionEvents.PushRange(parentCategoryChildents.ToArray());
            });

            await AddChildentCategory(competitionEvents.ToArray(), cancellationToken);
        }
    }
}
