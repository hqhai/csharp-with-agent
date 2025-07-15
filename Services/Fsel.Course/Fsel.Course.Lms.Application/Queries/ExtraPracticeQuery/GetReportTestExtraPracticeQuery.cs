// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReportTestExtraPracticeQuery : IRequest<MethodResult<ExtraPracticeResultModel>>
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid ExtraPracticeResultId { get; set; }
    }

    public class GetReportTestExtraPracticeQueryHandler : IRequestHandler<GetReportTestExtraPracticeQuery, MethodResult<ExtraPracticeResultModel>>
    {
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IMapper _mapper;

        public GetReportTestExtraPracticeQueryHandler(IExtraPracticeResultRepository extraPracticeResultRepository, IMapper mapper)
        {
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExtraPracticeResultModel>> Handle(GetReportTestExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeResultModel> methodResult = new MethodResult<ExtraPracticeResultModel>();

            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeResultId, cancellationToken: cancellationToken);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ExtraPracticeResultId), request.ExtraPracticeResultId);
                return methodResult;
            }
            if (extraPracticeResult.SkillScores != null && extraPracticeResult.Status == EnumResultStatus.Done)
            {
                var skillScores = extraPracticeResult.SkillScores.GroupBy(x => x.Skill).Select(x => new SkillScores { Skill = x.Key, CorrectCount = x.Sum(x => x.CorrectCount), TotalCount = x.Sum(x => x.TotalCount) }).ToList();
                foreach (var skillScore in skillScores)
                {
                    skillScore.Scores = ((int)skillScore.CorrectCount).GetIeltsScore(skillScore.Skill);
                    if (skillScore.Skill == EnumCourseSkill.Writing || skillScore.Skill == EnumCourseSkill.Speaking)
                    {
                        skillScore.TotalCount = 36;
                    }
                }

                await _extraPracticeResultRepository.BulkUpdateList(new List<ExtraPracticeResult> { extraPracticeResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeId, c.StudentId };
                });

                await _extraPracticeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeResultNotStatusDone), nameof(request.ExtraPracticeResultId), request.ExtraPracticeResultId);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<ExtraPracticeResultModel>(extraPracticeResult);
            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }
    }
}
