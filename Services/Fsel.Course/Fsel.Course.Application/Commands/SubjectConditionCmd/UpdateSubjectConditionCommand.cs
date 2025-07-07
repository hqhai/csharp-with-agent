// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.SubjectConditionCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.SubjectConditionRules;
    using Fsel.Course.Domain.Models.CommandModels.SubjectConditions;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateSubjectConditionCommand : IRequest<MethodResult<IList<SubjectConditionModel>>>
    {
        public IList<UpdateSubjectConditionCommandModel>? SubjectConditions { get; set; }
    }

    public class UpdateSubjectConditionCommandHandler : IRequestHandler<UpdateSubjectConditionCommand, MethodResult<IList<SubjectConditionModel>>>
    {
        private readonly ISubjectConditionRepository _subjectConditionRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IMapper _mapper;

        public UpdateSubjectConditionCommandHandler(ISubjectConditionRepository subjectConditionRepository,
                                                    ILevelRepository levelRepository,
                                                    IMapper mapper)
        {
            _subjectConditionRepository = subjectConditionRepository;
            _levelRepository = levelRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SubjectConditionModel>>> Handle(UpdateSubjectConditionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.SubjectConditions);
            MethodResult<IList<SubjectConditionModel>> methodResult = new MethodResult<IList<SubjectConditionModel>>();

            var subjectConditionIds = request.SubjectConditions.Select(x => x.Id).ToList();
            if (subjectConditionIds == null || !subjectConditionIds.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(subjectConditionIds));
                return methodResult;
            }

            var subjectConditions = await _subjectConditionRepository.Queryable
                                                                     .Include(x => x.SubjectConditionRules)
                                                                     .WhereBulkContains(subjectConditionIds, c => c.Id)
                                                                     .ToListAsync(cancellationToken);
            if (subjectConditions == null || !subjectConditions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(subjectConditions));
                return methodResult;
            }

            foreach (var item in request.SubjectConditions)
            {
                var subjectCondition = subjectConditions.FirstOrDefault(x => x.Id == item.Id);
                if (subjectCondition == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(subjectCondition));
                    return methodResult;
                }

                var subjectConditionHandler = await SubjectConditionHandler(item, subjectCondition, cancellationToken);
                if (!subjectConditionHandler.IsOK)
                {
                    methodResult.AddErrorBadRequest(subjectConditionHandler.ErrorMessages);
                    return methodResult;
                }
            }

            await _subjectConditionRepository.ExecuteTransactionAsync(async () =>
            {
                _subjectConditionRepository.UpdateList(subjectConditions);
                await _subjectConditionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<IList<SubjectConditionModel>>(subjectConditions);

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }

        private async Task<MethodResult<bool>> SubjectConditionHandler(UpdateSubjectConditionCommandModel request, SubjectCondition subjectCondition, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (request.Status && (request.SubjectConditionRules == null || !request.SubjectConditionRules.Any()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSubjectConditionErrorCode.NullSubjectConditionRule), nameof(request.SubjectConditionRules));
                return methodResult;
            }

            _mapper.Map(request, subjectCondition);

            if (!subjectCondition.Status)
            {
                methodResult.Result = true;
                return methodResult;
            }

            foreach (var item in request.SubjectConditionRules!)
            {
                var subjectConditionRuleHandler = await SubjectConditionRuleHandler(item, subjectCondition.SubjectConditionRules, subjectCondition.Type, cancellationToken);
                if (!subjectConditionRuleHandler.IsOK)
                {
                    methodResult.AddErrorBadRequest(subjectConditionRuleHandler.ErrorMessages);
                    return methodResult;
                }
            }

            DeleteSubjectConditionRules(subjectCondition.SubjectConditionRules, request.SubjectConditionRules);

            methodResult.Result = true;
            return methodResult;
        }

        private async Task<MethodResult<bool>> SubjectConditionRuleHandler(UpdateSubjectConditionRuleCommandModel item, ICollection<SubjectConditionRule> subjectConditionRules, EnumConditionType conditionType, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();
            SubjectConditionRule subjectConditionRule = new SubjectConditionRule();

            if (item.Id.HasValue)
            {
                var subjectConditionRuleHasValue = subjectConditionRules.FirstOrDefault(x => x.Id == item.Id);
                if (subjectConditionRuleHasValue == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(subjectConditionRule));
                    return methodResult;
                }
                subjectConditionRule = subjectConditionRuleHasValue;
            }

            var subjectConditionRuleHandler = await SubjectConditionRuleMapValue(item, subjectConditionRule, conditionType, cancellationToken);
            if (!subjectConditionRuleHandler.IsOK)
            {
                methodResult.AddErrorBadRequest(subjectConditionRuleHandler.ErrorMessages);
                return methodResult;
            }

            if (item.Id.HasValue)
            {
                _mapper.Map(item, subjectConditionRule);
            }
            else
            {
                subjectConditionRules.Add(subjectConditionRule);
            }

            return methodResult;
        }

        private async Task<MethodResult<bool>> SubjectConditionRuleMapValue(UpdateSubjectConditionRuleCommandModel request, SubjectConditionRule subjectConditionRule, EnumConditionType conditionType, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            IList<Guid> levelIds = (subjectConditionRule.ConditionRules?.Where(x => x.LevelIds != null).SelectMany(x => x.LevelIds!) ?? new List<Guid>())
                                   .Union(subjectConditionRule.ConditionValues?.Where(x => x.LevelIds != null).SelectMany(x => x.LevelIds!) ?? new List<Guid>()).ToList();

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

            _mapper.Map(request, subjectConditionRule);
            methodResult.Result = true;
            return methodResult;
        }

        private static void DeleteSubjectConditionRules(ICollection<SubjectConditionRule> subjectConditionRules, IList<UpdateSubjectConditionRuleCommandModel> requestSubjectConditionRules)
        {
            var subjectConditionRuleIds = requestSubjectConditionRules.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
            var subjectConditionRuleDeletes = subjectConditionRules.Where(x => !subjectConditionRuleIds.Contains(x.Id) && x.Id != Guid.Empty).ToList();

            foreach (var item in subjectConditionRuleDeletes)
            {
                subjectConditionRules.Remove(item);
            }
        }
    }
}
