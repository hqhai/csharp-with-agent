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
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
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

            var examPracticeResult = await GetExamPracticeResultAsync(request.ExamPracticeResultId, cancellationToken);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult), request.ExamPracticeResultId);
                return methodResult;
            }

            var examPracticeSectionResult = await GetExamPracticeSectionResultAsync(request, examPracticeResult.CreatedDate, cancellationToken);
            if (examPracticeSectionResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSectionResult));
                return methodResult;
            }

            if (!IsSectionResultDone(examPracticeSectionResult))
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusNotDone), nameof(examPracticeSectionResult.Status));
                return methodResult;
            }

            var courseSkill = examPracticeSectionResult.ExamPracticeSection?.CourseSkill;
            if (IsSkillScoreReportable(courseSkill, examPracticeSectionResult.SkillScores))
            {
                var examPracticeSectionResultReport = GetSectionResultReport(examPracticeSectionResult, examPracticeResult);
                methodResult.Result = examPracticeSectionResultReport;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<ExamPracticeResult?> GetExamPracticeResultAsync(Guid examPracticeResultId, CancellationToken cancellationToken)
        {
            return await _examPracticeResultRepository.Queryable.AsNoTracking()
                .Include(x => x.ExamPractice)
                .FirstOrDefaultAsync(x => x.Id == examPracticeResultId, cancellationToken);
        }

        private async Task<ExamPracticeSectionResult?> GetExamPracticeSectionResultAsync(GetExamPracticeSectionResultReportQuery request, DateTime createdDate, CancellationToken cancellationToken)
        {
            return await _examPracticeSectionResultRepository.Queryable.AsNoTracking()
                .Include(x => x.ExamPracticeSection)
                .Where(x => x.CreatedDate >= createdDate)
                .FirstOrDefaultAsync(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId && x.ExamPracticeResultId == request.ExamPracticeResultId, cancellationToken);
        }

        private static bool IsSectionResultDone(ExamPracticeSectionResult sectionResult)
        {
            return sectionResult.Status == EnumResultStatus.Done;
        }

        private static bool IsSkillScoreReportable(EnumCourseSkill? courseSkill, IList<SkillScores>? skillScores)
        {
            return (courseSkill == EnumCourseSkill.Reading || courseSkill == EnumCourseSkill.Listening) && skillScores != null;
        }

        private ExamPracticeSectionResultModel GetSectionResultReport(ExamPracticeSectionResult examPracticeSectionResult, ExamPracticeResult examPracticeResult)
        {
            var report = _mapper.Map<ExamPracticeSectionResultModel>(examPracticeSectionResult);
            var scores = examPracticeSectionResult.SkillScores?.Select(x => x.Scores).FirstOrDefault() ?? default;
            report.BandScoresReport = GetBandScoresReport(examPracticeSectionResult, examPracticeResult, scores);

            double executionTime = default;
            if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice
                && examPracticeResult.Config?.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
            {
                executionTime = examPracticeResult.Config?.ExecutionTime ?? default;
                report.RemainingTime = executionTime - examPracticeSectionResult.WorkingTime > 0 ? executionTime - examPracticeSectionResult.WorkingTime : default;
            }
            if (examPracticeResult.ExamPractice?.Type == EnumExamPracticeType.IELTS
                && examPracticeResult.ExamPractice != null && examPracticeResult.ExamPractice.CourseLevel.HasValue)
            {
                (report.IsCheckScoreColor, report.TargetBandScore) = examPracticeResult.ExamPractice.CourseLevel.Value.CheckScoreColor(scores);
            }
            return report;
        }

        private BandScoresReport GetBandScoresReport(ExamPracticeSectionResult examPracticeSectionResult, ExamPracticeResult examPracticeResult, double scores)
        {
            var isVstep = examPracticeResult.ExamPractice?.Type == EnumExamPracticeType.Vstep;

            var bandScores = GetBandScoreConfigs(examPracticeSectionResult, examPracticeResult);
            var bandScore = isVstep ? GetVstepBandScore(bandScores, scores) : GetBandScore(bandScores, scores);
            var bandScoreReport = _mapper.Map<BandScoresReport>(bandScore);
            BandScores? bandScoreStudent;
            if (isVstep)
            {
                bandScoreStudent = bandScores?.FirstOrDefault(x => x.MaxInclusive >= scores && x.MinInclusive <= scores);
            }
            else
            {
                bandScoreStudent = bandScores?.FirstOrDefault(x => x.Scores == scores);
            }
            if (bandScoreStudent != null)
            {
                bandScoreReport.ScoresStudent = isVstep ? scores : bandScoreStudent.Scores;
                bandScoreReport.CorrectAnswerStudents = bandScoreStudent.CorrectAnswers;
            }
            return bandScoreReport;
        }

        private static IList<BandScores>? GetBandScoreConfigs(ExamPracticeSectionResult examPracticeSectionResult, ExamPracticeResult examPracticeResult)
        {
            var isVstep = examPracticeResult.ExamPractice?.Type == EnumExamPracticeType.Vstep;
            var resource = isVstep ? ResourceSettings.ExamBandScores : ResourceSettings.BandScoreFileName;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resource);
            var bandScores = ConvertHelper.DeserializeFromFilePath<IList<BandScores>>(path);
            if (isVstep)
            {
                bandScores = bandScores?.Where(x => x.CourseSkill == examPracticeSectionResult.ExamPracticeSection!.CourseSkill).OrderByDescending(x => x.MaxInclusive).ToList();
            }
            else
            {
                bandScores = bandScores?.Where(x => x.CourseSkill == examPracticeSectionResult.ExamPracticeSection!.CourseSkill).OrderByDescending(x => x.Scores).ToList();
            }

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

        private static BandScores? GetVstepBandScore(IList<BandScores>? bandScores, double scores)
        {
            return bandScores?.FirstOrDefault(x => x.MinInclusive <= scores && x.MaxInclusive >= scores);
        }
    }
}
