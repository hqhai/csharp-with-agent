// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.SubjectConditionQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSubjectConditionPreviewQuery : IRequest<MethodResult<IList<SubjectConditionPreviewModel>>>
    {
        public Guid LevelId { get; set; }

        public int Age { get; set; }
    }

    public class GetSubjectConditionPreviewQueryHandler : IRequestHandler<GetSubjectConditionPreviewQuery, MethodResult<IList<SubjectConditionPreviewModel>>>
    {
        private readonly ILevelRepository _levelRepository;
        private readonly ISubjectConditionRuleRepository _subjectConditionRuleRepository;
        private readonly SubjectConditionHelper _subjectConditionHelper;

        public GetSubjectConditionPreviewQueryHandler(ILevelRepository levelRepository,
                                                      ISubjectConditionRuleRepository subjectConditionRuleRepository,
                                                      SubjectConditionHelper subjectConditionHelper)
        {
            _levelRepository = levelRepository;
            _subjectConditionRuleRepository = subjectConditionRuleRepository;
            _subjectConditionHelper = subjectConditionHelper;
        }
        public async Task<MethodResult<IList<SubjectConditionPreviewModel>>> Handle(GetSubjectConditionPreviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SubjectConditionPreviewModel>> methodResult = new MethodResult<IList<SubjectConditionPreviewModel>>();

            var level = await _levelRepository.Queryable.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == request.LevelId, cancellationToken);
            if (level == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(level), request.LevelId);
                return methodResult;
            }

            var conditionRuleCourseSuggests = await _subjectConditionRuleRepository.Queryable
                                                                                   .Where(x => level.Category != null && x.SubjectCondition != null &&
                                                                                          x.SubjectCondition.CategoryId == level.Category.ParentId &&
                                                                                          x.SubjectCondition.Type == EnumConditionType.CourseSuggest)
                                                                                   .AsNoTracking()
                                                                                   .ToListAsync(cancellationToken)
                                                                                   .ConfigureAwait(false);
            if (conditionRuleCourseSuggests == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(conditionRuleCourseSuggests));
                return methodResult;
            }

            var subjectConditionPreviews = await SubjectConditionPreviewHandler(conditionRuleCourseSuggests, request.LevelId, request.Age, cancellationToken);

            methodResult.Result = subjectConditionPreviews.Result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<IList<SubjectConditionPreviewModel>>> SubjectConditionPreviewHandler(IList<SubjectConditionRule> conditionRuleCourseSuggests, Guid levelId, int age, CancellationToken cancellationToken)
        {
            MethodResult<IList<SubjectConditionPreviewModel>> methodResult = new MethodResult<IList<SubjectConditionPreviewModel>>();

            var subjectConditionRules = _subjectConditionHelper.SubjectConditionRuleHandler(conditionRuleCourseSuggests, levelId, age);
            List<SubjectConditionPreviewModel> subjectConditionPreviews = new List<SubjectConditionPreviewModel>();

            var conditionValues = subjectConditionRules.Where(x => x.ConditionRules != null).SelectMany(x => x.ConditionValues!).ToList();

            var levelIds = conditionValues.Where(x => x.LevelIds != null).SelectMany(x => x.LevelIds!).ToList();
            var levelDatas = await _levelRepository.Queryable.Include(x => x.Category).WhereBulkContains(levelIds, x => x.Id).ToListAsync(cancellationToken);

            foreach (var subjectConditionPreview in conditionValues)
            {
                if (subjectConditionPreview.LevelIds == null)
                {
                    continue;
                }

                subjectConditionPreviews.AddRange(subjectConditionPreview.LevelIds.Select(level => new SubjectConditionPreviewModel
                {
                    LevelId = level,
                    LevelName = $"{levelDatas.FirstOrDefault(x => x.Id == level)?.Category?.Name} - {levelDatas.FirstOrDefault(x => x.Id == level)?.Name}",
                    Type = subjectConditionPreview.Type
                }).ToList());
            }

            methodResult.Result = subjectConditionPreviews.OrderByDescending(x => x.Type).ToList();
            return methodResult;
        }
    }
}
