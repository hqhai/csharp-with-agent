// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.SubjectConditionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.SubjectConditionRules;
    using Fsel.Course.Domain.Models.CommandModels.SubjectConditions;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateSubjectConditionCommand : IRequest<MethodResult<IList<SubjectConditionModel>>>
    {
        public IList<CreateSubjectConditionCommandModel>? SubjectConditions { get; set; }
    }

    public class CreateSubjectConditionCommandHandler : IRequestHandler<CreateSubjectConditionCommand, MethodResult<IList<SubjectConditionModel>>>
    {
        private readonly ISubjectConditionRepository _subjectConditionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IMapper _mapper;

        public CreateSubjectConditionCommandHandler(ISubjectConditionRepository subjectConditionRepository,
                                                    ICategoryRepository categoryRepository,
                                                    ILevelRepository levelRepository,
                                                    IMapper mapper)
        {
            _subjectConditionRepository = subjectConditionRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SubjectConditionModel>>> Handle(CreateSubjectConditionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.SubjectConditions);
            MethodResult<IList<SubjectConditionModel>> methodResult = new MethodResult<IList<SubjectConditionModel>>();

            List<SubjectCondition> subjectConditions = new List<SubjectCondition>();

            foreach (var item in request.SubjectConditions)
            {
                var subjectConditionHandler = await SubjectConditionHandler(item, subjectConditions, cancellationToken);
                if (!subjectConditionHandler.IsOK)
                {
                    methodResult.AddErrorBadRequest(subjectConditionHandler.ErrorMessages);
                    return methodResult;
                }
            }

            await _subjectConditionRepository.ExecuteTransactionAsync(async () =>
            {
                await _subjectConditionRepository.AddList(subjectConditions);
                await _subjectConditionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<IList<SubjectConditionModel>>(subjectConditions);

                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }

        private async Task<MethodResult<bool>> SubjectConditionHandler(CreateSubjectConditionCommandModel request, IList<SubjectCondition> subjectConditions, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var checkCategory = await _categoryRepository.Queryable.AnyAsync(x => x.ParentId == request.CategoryId && x.Type == EnumTypeCategory.Program, cancellationToken);
            if (!checkCategory)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.NoCategoryProgram), nameof(checkCategory), request.CategoryId);
                return methodResult;
            }

            var checkCategoryAlreadyExist = await _subjectConditionRepository.Queryable.AnyAsync(x => x.CategoryId == request.CategoryId, cancellationToken);
            if (checkCategoryAlreadyExist)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.CategoryAlreadyExist), nameof(checkCategoryAlreadyExist), request.CategoryId);
                return methodResult;
            }

            if (request.Status && (request.SubjectConditionRules == null || !request.SubjectConditionRules.Any()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.NullSubjectConditionRule), nameof(request.SubjectConditionRules));
                return methodResult;
            }

            if (!request.Status && (request.SubjectConditionRules == null || !request.SubjectConditionRules.Any()))
            {
                subjectConditions.Add(_mapper.Map<SubjectCondition>(request));
                return methodResult;
            }

            var subjectCondition = _mapper.Map<SubjectCondition>(request);

            foreach (var item in request.SubjectConditionRules!)
            {
                var subjectConditionRuleHandler = await SubjectConditionRuleHandler(item, subjectCondition, request.Type, cancellationToken);
                if (!subjectConditionRuleHandler.IsOK)
                {
                    methodResult.AddErrorBadRequest(subjectConditionRuleHandler.ErrorMessages);
                    return methodResult;
                }
            }

            if (!subjectCondition.IsValid())
            {
                methodResult.AddErrorBadRequest(subjectCondition.ErrorMessages);
                return methodResult;
            }

            subjectConditions.Add(subjectCondition);

            methodResult.Result = true;
            return methodResult;
        }

        private async Task<MethodResult<bool>> SubjectConditionRuleHandler(CreateSubjectConditionRuleCommandModel request, SubjectCondition subjectCondition, EnumConditionType conditionType, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            IList<Guid> levelIds = (request.ConditionRules?.Where(x => x.LevelIds != null).SelectMany(x => x.LevelIds!) ?? new List<Guid>())
                                   .Union((request.ConditionValues?.Where(x => x.LevelIds != null).SelectMany(x => x.LevelIds!) ?? new List<Guid>())).ToList();

            var checkLevel = await _levelRepository.Queryable.WhereBulkContains(levelIds, x => x.Id).CountAsync(cancellationToken);
            if (checkLevel != levelIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.LevelAlreadyExist), nameof(levelIds));
                return methodResult;
            }

            if (request.ConditionRules == null || !request.ConditionRules.Any() || request.ConditionValues == null || !request.ConditionValues.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.DataNotNull), $"{request.ConditionRules}-{request.ConditionValues}");
                return methodResult;
            }

            if (request.ConditionRules.Any(c => c.Type == EnumSubjectConditionRuleType.Age && !c.FromAge.HasValue))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.AgeNotNull), "Age");
                return methodResult;
            }

            if (request.ConditionRules.Any(c => c.Type == EnumSubjectConditionRuleType.CurrentLevel && (c.LevelIds == null || !c.LevelIds.Any())))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.LevelIdsNotNull), "LevelIds");
                return methodResult;
            }

            if (request.ConditionValues.Any(c => c.Type != EnumSubjectConditionValueType.Maximum && (c.LevelIds == null || !c.LevelIds.Any())))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.LevelIdsNotNull), "LevelIds");
                return methodResult;
            }

            if (conditionType == EnumConditionType.CourseSuggest && !request.ConditionValues.Any(c => c.Type == EnumSubjectConditionValueType.Recommended))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.TypeRecommendedNotNull), nameof(request.ConditionValues), request.ConditionValues);
                return methodResult;
            }

            var subjectConditionRule = _mapper.Map<SubjectConditionRule>(request);
            subjectCondition.SubjectConditionRules.Add(subjectConditionRule);

            methodResult.Result = true;
            return methodResult;
        }
    }
}
