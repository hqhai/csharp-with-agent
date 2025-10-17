// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;

    public class GetExamPracticePartsQuery : IRequest<MethodResult<ExamPracticeConfigModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetExamPracticePartsQueryHandler : IRequestHandler<GetExamPracticePartsQuery, MethodResult<ExamPracticeConfigModel>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private const int DefaultQuestion = 1;

        public GetExamPracticePartsQueryHandler(IExamPracticeRepository examPracticeRepository,
            IExamPracticeSectionRepository examPracticeSectionRepository)
        {
            _examPracticeRepository = examPracticeRepository;
            _examPracticeSectionRepository = examPracticeSectionRepository;
        }

        public async Task<MethodResult<ExamPracticeConfigModel>> Handle(GetExamPracticePartsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeConfigModel>();
            var examPractice = await _examPracticeRepository.Queryable.Include(x => x.ExamPracticeSections).AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), request.Id);
                return methodResult;
            }
            if (examPractice.Type == EnumExamPracticeType.IELTS && examPractice.SubType != EnumExamPracticeSubType.SkillMockTest)
            {
                return methodResult;
            }
            if (examPractice.Type == EnumExamPracticeType.IELTS && examPractice.SubType != EnumExamPracticeSubType.SkillMockTest)
            {
                return methodResult;
            }
            var examPracticeSection = examPractice.ExamPracticeSections.FirstOrDefault();
            if (examPracticeSection == null)
            {
                return methodResult;
            }
            var examPracticeConfig = new ExamPracticeConfigModel();
            var listSkill = new List<EnumCourseSkill> { EnumCourseSkill.Reading, EnumCourseSkill.Writing };
            if (examPractice.Type != EnumExamPracticeType.ExamPractice && examPracticeSection.CourseSkill.HasValue && listSkill.Any(x => x == examPracticeSection.CourseSkill.Value))
            {
                var examPracticeSections = await _examPracticeSectionRepository.Queryable.Where(x => x.ParentExamPracticeSectionId == examPracticeSection.Id)
                                                .OrderBy(x => x.DisplayOrder)
                                                .Select(x => new ExamPracticePartModel
                                                {
                                                    ExamPracticeSectionId = x.Id,
                                                    TotalQuestion = examPracticeSection.CourseSkill == EnumCourseSkill.Reading ? x.Questions.Count : DefaultQuestion
                                                })
                                                .ToListAsync(cancellationToken);

                examPracticeConfig.ExamPracticeParts = examPracticeSections;
            }

            examPracticeConfig.PracticeTimeLimitRules = examPractice.Type.GetEnumPracticeTimeLimits(examPractice.SubType, examPracticeSection.CourseSkill);
            methodResult.Result = examPracticeConfig;
            return methodResult;
        }
    }
}
