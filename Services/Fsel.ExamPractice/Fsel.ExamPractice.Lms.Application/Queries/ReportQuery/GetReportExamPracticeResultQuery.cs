// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.ExamPractice.Lms.Application.Queries.ReportQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;

    public class GetReportExamPracticeResultQuery : IRequest<MethodResult<ExamPracticeResultReportModel>>
    {
        public Guid ExamPracticeResultId { get; set; }
    }

    public class GetReportExamPracticeResultQueryHandler : IRequestHandler<GetReportExamPracticeResultQuery, MethodResult<ExamPracticeResultReportModel>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IMapper _mapper;
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;

        public GetReportExamPracticeResultQueryHandler(IExamPracticeRepository examPracticeRepository,
            IExamPracticeResultRepository examPracticeResultRepository,
            IMapper mapper,
            IExamPracticeAnswerRepository examPracticeAnswerRepository)
        {
            _examPracticeRepository = examPracticeRepository;
            _examPracticeResultRepository = examPracticeResultRepository;
            _mapper = mapper;
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
        }

        public async Task<MethodResult<ExamPracticeResultReportModel>> Handle(GetReportExamPracticeResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeResultReportModel>();
            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(request.ExamPracticeResultId);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult), request.ExamPracticeResultId);
                return methodResult;
            }
            var examPractice = await _examPracticeRepository.Queryable.AsNoTracking().Include(x => x.ExamPracticeSections)
                                                            .FirstOrDefaultAsync(x => x.Id == examPracticeResult.ExamPracticeId, cancellationToken);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), examPracticeResult.ExamPracticeId);
                return methodResult;
            }
            var examPracticeResultReport = _mapper.Map<ExamPracticeResultReportModel>(examPracticeResult);
            if (examPractice.CourseLevel.HasValue)
            {
                (examPracticeResultReport.IsCheckScoreColor, examPracticeResultReport.TargetBandScore) = examPractice.CourseLevel.Value.CheckScoreColor(examPracticeResultReport.Score ?? default);
            }
            examPracticeResultReport.IsTeacherGraded = await IsTeacherGraded(examPracticeResult, examPractice.ExamPracticeSections.Where(x => x.CourseSkill.HasValue).Select(x => x.CourseSkill.GetValueOrDefault()).ToList());
            methodResult.Result = examPracticeResultReport;
            return methodResult;
        }

        public async Task<bool> IsTeacherGraded(ExamPracticeResult examPracticeResult, IList<EnumCourseSkill>? courseSkills)
        {
            ArgumentNullException.ThrowIfNull(examPracticeResult);
            var isTeacherGradedSkill = courseSkills?.Any(x => x == EnumCourseSkill.Speaking);
            var isAIGraded = courseSkills?.Any(x => x == EnumCourseSkill.Writing);
            if (isTeacherGradedSkill.HasValue && isTeacherGradedSkill.Value)
            {
                return examPracticeResult.ExamPracticeScores.Any();
            }
            if (isAIGraded.HasValue && isAIGraded.Value)
            {
                var gradingAlFeedbacks = await _examPracticeAnswerRepository.Queryable.AsNoTracking()
                                                                             .Where(x => x.ExamPracticeResultId == examPracticeResult.Id)
                                                                             .Select(x => x.GradingAlFeedback)
                                                                             .ToListAsync();
                return gradingAlFeedbacks.All(x => !string.IsNullOrEmpty(x));
            }
            return true;
        }
    }
}
