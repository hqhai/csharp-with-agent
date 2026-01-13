// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ProgramCmd
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Programs;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateProgramCommand : UpdateProgramCommandModel, IRequest<MethodResult<ProgramModel>>
    {
    }

    public class UpdateProgramCommandHandler : IRequestHandler<UpdateProgramCommand, MethodResult<ProgramModel>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ProgramConverter _programConverter;
        private readonly ITestRepository _testRepository;
        private static readonly Regex s_regexCode = new Regex("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

        public UpdateProgramCommandHandler(ICategoryRepository categoryRepository,
                                           IMapper mapper,
                                           ProgramConverter programConverter,
                                           ITestRepository testRepository)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _programConverter = programConverter;
            _testRepository = testRepository;
        }

        public async Task<MethodResult<ProgramModel>> Handle(UpdateProgramCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ProgramModel>();

            #region Validate

            if (string.IsNullOrEmpty(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(request.Name), request.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Code) || (!string.IsNullOrEmpty(request.Code) && !s_regexCode.IsMatch(request.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.CodeNotValid), nameof(request.Code), request.Code);
                return methodResult;
            }

            var checkCode = await _categoryRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.Code == request.Code!.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            var programs = await _categoryRepository.Queryable.Where(x => x.ParentId == category.ParentId).ToListAsync(cancellationToken);
            if (request.IsTestDefault && programs.Any(x => x.Id != request.Id && x.IsTestDefault))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.IsTestDefault), request.IsTestDefault);
                return methodResult;
            }
            if (request.TestMode.HasValue && request.TestMode.Value == EnumTestMode.Default && !programs.Any(x => x.Id != request.Id && x.IsTestDefault))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.TestMode), programs.Any(x => x.IsTestDefault));
                return methodResult;
            }
            if (request.TestOriginalIds != null && request.TestOriginalIds.Any())
            {
                var tests = await _testRepository.Queryable.WhereBulkContains(request.TestOriginalIds, x => x.OriginalId).Where(x => x.VersionStatus == EnumVersionStatus.LastVersion).ToListAsync(cancellationToken);

                if (tests.Count != request.TestOriginalIds.Count)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NotEnoughTests));
                    return methodResult;
                }
            }

            #endregion Validate

            category = await _categoryRepository.Queryable.Include(x => x.CategoryTestBanks)
                                                   .Include(x => x.Levels)
                                                   .ThenInclude(x => x.SkillLevels)
                                                   .ThenInclude(x => x.Skill)
                                                   .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(category), request.Id);
                return methodResult;
            }

            if (request.Levels == null || !request.Levels.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Levels), request.Levels);
                return methodResult;
            }

            _mapper.Map(request, category);
            if (!category.IsValid())
            {
                methodResult.AddErrorBadRequest(category.ErrorMessages);
                return methodResult;
            }
            var incomingIds = request.TestOriginalIds?.Distinct().ToList() ?? new List<Guid>();
            var toRemoves = new List<CategoryTestBank>();
            foreach (var oldBank in category.CategoryTestBanks)
            {
                if (!incomingIds.Contains(oldBank.TestOriginalId))
                {
                    toRemoves.Add(oldBank);
                }
            }
            foreach (var item in toRemoves)
            {
                category.CategoryTestBanks.Remove(item);
            }

            var existingIds = category.CategoryTestBanks.Select(x => x.TestOriginalId).ToHashSet();
            foreach (var id in incomingIds)
            {
                if (!existingIds.Contains(id))
                {
                    category.CategoryTestBanks.Add(new CategoryTestBank
                    {
                        TestOriginalId = id,
                        TestType = EnumTestType.PlacementTest
                    });
                }
            }
            var levelIds = category.Levels.Select(x => x.Id).ToList();
            var levelRequestIds = request.Levels.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
            var duplicateIds = levelIds.Intersect(levelRequestIds).ToList();

            var deleteLevelIds = levelIds.Where(x => !duplicateIds.Contains(x)).ToList();
            if (deleteLevelIds.Any())
            {
                var itemsToRemove = category.Levels
                                            .Where(x => deleteLevelIds.Contains(x.Id))
                                            .ToList();

                foreach (var item in itemsToRemove)
                {
                    category.Levels.Remove(item);
                }
            }

            foreach (var level in request.Levels)
            {
                var levelHandler = await _programConverter.LevelHandler(level, category, cancellationToken);
                if (!levelHandler.IsOK)
                {
                    methodResult.AddErrorBadRequest(levelHandler.ErrorMessages);
                    return methodResult;
                }
            }

            await _categoryRepository.ExecuteTransactionAsync(async () =>
            {
                _categoryRepository.Update(category);
                await _categoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<ProgramModel>(category);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
