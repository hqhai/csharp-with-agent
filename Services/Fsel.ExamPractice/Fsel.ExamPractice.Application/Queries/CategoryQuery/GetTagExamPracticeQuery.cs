// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Queries.CategoryQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.ExamPractice.Infrastructure.Common.EnumHelper;

    public class GetTagExamPracticeQuery : IRequest<MethodResult<IList<ExamPracticeTagModel>>>
    {
        public EnumExamPracticeType Type { get; set; }
    }

    public class GetTagExamPracticeQueryHandler : IRequestHandler<GetTagExamPracticeQuery, MethodResult<IList<ExamPracticeTagModel>>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;

        public GetTagExamPracticeQueryHandler(IExamPracticeRepository examPracticeRepository)
        {
            _examPracticeRepository = examPracticeRepository;
        }

        public async Task<MethodResult<IList<ExamPracticeTagModel>>> Handle(GetTagExamPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ExamPracticeTagModel>> methodResult = new MethodResult<IList<ExamPracticeTagModel>>();

            var tags = await GetDataTags(request.Type);

            methodResult.Result = request.Type.GetTags().Where(x => tags.Any(y => y.SubType == x.SubType && y.CourseSkill == x.CourseSkill)).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<ExamPracticeTagModel>> GetDataTags(EnumExamPracticeType practiceType)
        {
            var pairs = await _examPracticeRepository.Queryable.Where(x => x.Type == practiceType)
                                                    .Where(x => x.Status == EnumExamPracticeStatus.Active && x.VersionStatus == EnumVersionStatus.LastVersion)
                                                    .Where(x => !x.IsArchive)
                                                    .Select(x => new
                                                    {
                                                        SubType = x.SubType,
                                                        Skills = x.ExamPracticeSections.Select(s => s.CourseSkill)
                                                    })
                                                    .ToListAsync();
            return pairs.Select(g =>
            {
                var isSkillTest = g.SubType == EnumExamPracticeSubType.SkillMockTest || g.SubType == EnumExamPracticeSubType.SingleVstepSkill;

                return new ExamPracticeTagModel
                {
                    SubType = g.SubType,
                    CourseSkill = isSkillTest ? g.Skills.FirstOrDefault() : null,
                    Description = g.SubType.GetDescription()
                };
            }).Distinct().ToList();
        }
    }
}
