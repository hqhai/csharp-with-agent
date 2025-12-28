// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Unit
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByUnitTestQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public EnumTimeCodeType? Type { get; set; }
    }

    public class GetUnitByUnitTestQueryHandler : IRequestHandler<GetUnitByUnitTestQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUserService _userService;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;

        public GetUnitByUnitTestQueryHandler(AuthContext authContext
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IUserService userService
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository)
        {
            _authContext = authContext;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _userService = userService;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetUnitByUnitTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();
            request.Type ??= EnumTimeCodeType.UnitTest;

            var method = await GetStudentAsync();
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var studentId = method.Result!.Id;

            var videoTimeCodeResults = await GetVideoTimeCodeResultsAsync(request, studentId, cancellationToken);
            if (videoTimeCodeResults == null || !videoTimeCodeResults.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var workingTime = videoTimeCodeResults.Sum(x => GetSecond(x));
            var skillScores = videoTimeCodeResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
            .SelectMany(x => x.SkillScores!)
            .GroupBy(x => new { x.Skill, x.SkillId, x.SkillName })
            .Select(x =>
            {
                return new SkillScores
                {
                    Skill = x.Key.Skill,
                    SkillId = x.Key.SkillId,
                    SkillName = x.Key.SkillName,
                    CorrectQuestion = x.Sum(x => x.CorrectQuestion),
                    TokenReceived = x.Sum(x => x.TokenReceived),
                    CountQuestion = x.Sum(x => x.CountQuestion),
                    TotalQuestion = x.Sum(x => x.TotalQuestion),
                    CorrectCount = x.Sum(x => x.CorrectCount),
                    TotalCount = x.Sum(x => x.TotalCount),
                };
            })
            .ToList();

            overallScoreReport.HighestStreak = videoTimeCodeResults.Max(x => x.HighestStreak);
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.WorkingTime = workingTime;
            overallScoreReport.CourseSkills = skillScores.Select(x => x.Skill).ToList();
            overallScoreReport.Skills = skillScores.Select(x => x.SkillName ?? string.Empty).Where(x => string.IsNullOrEmpty(x)).ToList();
            overallScoreReport.CountQuestion = overallScoreReport.SkillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = overallScoreReport.SkillScores.Sum(x => x.TotalQuestion);

            methodResult.Result = overallScoreReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            methodResult.Result = student;
            return methodResult;
        }

        private async Task<IList<VideoTimeCodeResult>> GetVideoTimeCodeResultsAsync(GetUnitByUnitTestQuery request, Guid studentId, CancellationToken cancellationToken)
        {
            var videoTimeCodeResults = await (from baseQ in _lessonResultRepository.ReadQueryable
                                              join vr in _videoResultRepository.ReadQueryable on baseQ.Id equals vr.LessonResultId
                                              join vtcr in _videoTimeCodeResultRepository.ReadQueryable on vr.Id equals vtcr.VideoResultId
                                              join vtc in _videoTimeCodeRepository.ReadQueryable on vtcr.VideoTimeCodeId equals vtc.Id
                                              where baseQ.CourseId == request.CourseId && baseQ.UnitId == request.UnitId && baseQ.StudentId == studentId
                                              && vtc.TimeCodeType == request.Type
                                              select vtcr).ToListAsync(cancellationToken);
            return videoTimeCodeResults;
        }

        private static long GetSecond(VideoTimeCodeResult x)
        {
            TimeSpan timeDiff = x.UpdatedDate.HasValue ? x.UpdatedDate.Value - x.CreatedDate : default;
            return Convert.ToInt64(timeDiff.TotalSeconds);
        }
    }
}
