// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.ReportQuery
{
    using System.IO;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Entities.BandScoresConfigs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetExamPracticeSectionResultReportQuery : IRequest<MethodResult<ExamPracticeSectionResultModel>>
    {
        public Guid ExamPracticeResultId { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
    }

    public class GetExamPracticeSectionResultReportQueryHandler : IRequestHandler<GetExamPracticeSectionResultReportQuery, MethodResult<ExamPracticeSectionResultModel>>
    {
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;
        private readonly IMapper _mapper;

        public GetExamPracticeSectionResultReportQueryHandler(
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository,
            IMapper mapper)
        {
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExamPracticeSectionResultModel>> Handle(GetExamPracticeSectionResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeSectionResultModel>();

            var examPracticeResult = await _examPracticeResultRepository.Queryable.Include(x => x.ExamPractice).FirstOrDefaultAsync(x => x.Id == request.ExamPracticeResultId, cancellationToken);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult));
                return methodResult;
            }
            var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable.Include(x => x.ExamPracticeSection)
                                            .Where(x => x.CreatedDate >= examPracticeResult.CreatedDate)
                                            .FirstOrDefaultAsync(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId && x.ExamPracticeResultId == request.ExamPracticeResultId, cancellationToken);
            if (examPracticeSectionResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSectionResult));
                return methodResult;
            }
            if (examPracticeSectionResult.Status != EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusNotDone), nameof(examPracticeSectionResult.Status));
                return methodResult;
            }
            var courseSkill = examPracticeSectionResult.ExamPracticeSection!.CourseSkill;
            if ((courseSkill == EnumCourseSkill.Reading || courseSkill == EnumCourseSkill.Listening) && examPracticeSectionResult.SkillScores != null)
            {
                var examPracticeSectionResultReport = _mapper.Map<ExamPracticeSectionResultModel>(examPracticeSectionResult);
                double executionTime = default;
                if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config?.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
                {
                    executionTime = examPracticeResult.Config?.ExecutionTime ?? default;
                    examPracticeSectionResultReport.RemainingTime = executionTime - examPracticeSectionResult.WorkingTime > 0 ? executionTime - examPracticeSectionResult.WorkingTime : default;
                }

                var scores = examPracticeSectionResult.SkillScores.Select(x => x.Scores).FirstOrDefault();
                examPracticeSectionResultReport.BandScoresReport = GetBandScoresReport(examPracticeSectionResult, scores);
                if (examPracticeResult.ExamPractice != null && examPracticeResult.ExamPractice.CourseLevel.HasValue)
                {
                    (examPracticeSectionResultReport.IsCheckScoreColor, examPracticeSectionResultReport.TargetBandScore) = examPracticeResult.ExamPractice.CourseLevel.Value.CheckScoreColor(scores);
                }
                methodResult.Result = examPracticeSectionResultReport;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private BandScoresReport GetBandScoresReport(ExamPracticeSectionResult examPracticeSectionResult, double scores)
        {
            var bandScores = GetBandScoreConfigs(examPracticeSectionResult);
            var bandScore = GetBandScore(bandScores, scores);
            var bandScoreReport = _mapper.Map<BandScoresReport>(bandScore);
            var bandScoreStudent = bandScores?.FirstOrDefault(x => x.Scores == scores);
            if (bandScoreStudent != null)
            {
                bandScoreReport.ScoresStudent = bandScoreStudent.Scores;
                bandScoreReport.CorrectAnswerStudents = bandScoreStudent.CorrectAnswers;
            }
            return bandScoreReport;
        }

        private static IList<BandScores>? GetBandScoreConfigs(ExamPracticeSectionResult examPracticeSectionResult)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.BandScoreFileName);
            var bandScores = ConvertHelper.DeserializeFromFilePath<IList<BandScores>>(path);
            bandScores = bandScores?.Where(x => x.CourseSkill == examPracticeSectionResult.ExamPracticeSection!.CourseSkill).OrderByDescending(x => x.Scores).ToList();
            return bandScores;
        }

        private static BandScores? GetBandScore(IList<BandScores>? bandScores, double scores)
        {
            if (scores > 0)
            {
                return bandScores?.FirstOrDefault(x => x.Scores < scores);
            }
            return bandScores?.FirstOrDefault(x => x.Scores == scores);
        }
    }
}
