// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ProgramCmd
{
    using System.Text.RegularExpressions;
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

    public class CreateProgramCommand : CreateProgramCommandModel, IRequest<MethodResult<ProgramModel>>
    {
    }

    public class CreateProgramCommandHandler : IRequestHandler<CreateProgramCommand, MethodResult<ProgramModel>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ProgramConverter _programConverter;
        private readonly ITestRepository _testRepository;
        private static readonly Regex s_regexCode = new Regex("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

        public CreateProgramCommandHandler(ICategoryRepository categoryRepository,
                                           IMapper mapper,
                                           ProgramConverter programConverter,
                                           ITestRepository testRepository)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _programConverter = programConverter;
            _testRepository = testRepository;
        }

        public async Task<MethodResult<ProgramModel>> Handle(CreateProgramCommand request, CancellationToken cancellationToken)
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

            var checkCode = await _categoryRepository.Queryable.AnyAsync(x => x.Code == request.Code!.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentId);
            if (parentCategory == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ParentId), request.ParentId);
                return methodResult;
            }
            var programs = await _categoryRepository.Queryable.Where(x => x.ParentId == request.ParentId).ToListAsync(cancellationToken);
            if (request.IsTestDefault && programs.Any(x => x.IsTestDefault))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.IsTestDefault), request.IsTestDefault);
                return methodResult;
            }
            if (request.TestMode.HasValue && request.TestMode.Value == EnumTestMode.Default && !programs.Any(x => x.IsTestDefault))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.TestMode), programs.Any(x => x.IsTestDefault));
                return methodResult;
            }
            if (request.TestOriginalIds != null && request.TestOriginalIds.Any())
            {
                var tests = await _testRepository.Queryable.WhereBulkContains(request.TestOriginalIds, x => x.Id).Where(x => x.VersionStatus == EnumVersionStatus.LastVersion).ToListAsync(cancellationToken);
                if (tests.Count != request.TestOriginalIds.Distinct().Count())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NotEnoughTests));
                    return methodResult;
                }
            }

            #endregion Validate

            var category = _mapper.Map<Category>(request);
            category.Type = Shared.Enums.EnumTypeCategory.Program;
            if (request.TestOriginalIds != null && request.TestOriginalIds.Any())
            {
                category.CategoryTestBanks = request.TestOriginalIds.Distinct().Select(x => new CategoryTestBank
                {
                    TestOriginalId = x,
                    TestType = EnumTestType.PlacementTest,
                }).ToList();
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
                _categoryRepository.Add(category);
                await _categoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<ProgramModel>(category);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
