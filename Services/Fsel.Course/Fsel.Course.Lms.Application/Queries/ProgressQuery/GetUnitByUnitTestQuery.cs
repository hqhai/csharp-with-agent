// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
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
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUserService _userService;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;

        public GetUnitByUnitTestQueryHandler(AuthContext authContext
            , IUnitRepository unitRepository
            , ILessonRepository lessonRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IUserService userService
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository)
        {
            _authContext = authContext;
            _unitRepository = unitRepository;
            _lessonRepository = lessonRepository;
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
            if (!request.Type.HasValue)
            {
                request.Type = EnumTimeCodeType.UnitTest;
            }
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
            var studentId = student.Id;

            var videoTimeCodeResults = await GetVideoTimeCodeResultsAsync(request, studentId, cancellationToken);
            if (videoTimeCodeResults == null || !videoTimeCodeResults.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var workingTime = videoTimeCodeResults.Sum(x => GetSecond(x));
            var skillScores = videoTimeCodeResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).ToList();
            skillScores = skillScores.GroupBy(x => x!.Skill).Select(x =>
            {
                return new SkillScores
                {
                    Skill = x.Key,
                    CountQuestion = x.Sum(x => x.CountQuestion),
                    TotalQuestion = x.Sum(x => x.TotalQuestion),
                    CorrectCount = x.Sum(x => x.CorrectCount),
                    TotalCount = x.Sum(x => x.TotalCount),
                };
            }).ToList();
            overallScoreReport.HighestStreak = videoTimeCodeResults.Max(x => x.HighestStreak);
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.WorkingTime = workingTime;
            overallScoreReport.CourseSkills = skillScores.Select(x => x.Skill).ToList();
            overallScoreReport.CountQuestion = overallScoreReport.SkillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = overallScoreReport.SkillScores.Sum(x => x.TotalQuestion);

            methodResult.Result = overallScoreReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<VideoTimeCodeResult>> GetVideoTimeCodeResultsAsync(GetUnitByUnitTestQuery request, Guid studentId, CancellationToken cancellationToken)
        {
            var lessonResultIds = await GetLessonResultIdsAsync(request, studentId);
            if (lessonResultIds == null || !lessonResultIds.Any())
            {
                return new List<VideoTimeCodeResult>();
            }
            var videoIds = await GetVideoIdsAsync(request);
            if (videoIds == null || !videoIds.Any())
            {
                return new List<VideoTimeCodeResult>();
            }
            var videoResultIds = await _videoResultRepository.Queryable.WhereBulkContains(lessonResultIds, x => x.LessonResultId)
                                                             .Where(x => x.StudentId == studentId)
                                                             .Select(x => x.Id)
                                                             .ToListAsync(cancellationToken);
            if (videoResultIds == null || !videoResultIds.Any())
            {
                return new List<VideoTimeCodeResult>();
            }
            var videoTimeCodeIds = await _videoTimeCodeRepository.Queryable.WhereBulkContains(videoIds, x => x.VideoId).Where(x => x.TimeCodeType == request.Type)
                                                                         .Select(x => x.Id)
                                                                         .ToListAsync(cancellationToken);
            if (videoTimeCodeIds == null || !videoTimeCodeIds.Any())
            {
                return new List<VideoTimeCodeResult>();
            }

            return await _videoTimeCodeResultRepository.ReadQueryable.WhereBulkContains(videoResultIds, x => x.VideoResultId)
                                                        .WhereBulkContains(videoTimeCodeIds, x => x.VideoTimeCodeId)
                                                        .ToListAsync(cancellationToken);
        }

        private async Task<IList<Guid>> GetLessonIdsAsync(GetUnitByUnitTestQuery request)
        {
            return await _unitRepository.ReadQueryable.Include(x => x.UnitLessons)
                                                  .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId && x.UnitId == request.UnitId))
                                                  .SelectMany(x => x.UnitLessons)
                                                  .OrderBy(x => x.DisplayOrder)
                                                  .Select(x => x.LessonId)
                                                  .ToListAsync();
        }

        private async Task<List<Guid>> GetVideoIdsAsync(GetUnitByUnitTestQuery request)
        {
            var lessonIds = await GetLessonIdsAsync(request);
            if (!lessonIds.Any())
            {
                return new List<Guid>();
            }

            return await _lessonRepository.ReadQueryable.Include(x => x.LessonVideos)
                                                  .WhereBulkContains(lessonIds, x => x.Id)
                                                  .SelectMany(x => x.LessonVideos)
                                                  .Select(x => x.VideoId)
                                                  .ToListAsync();
        }

        private async Task<IList<Guid>> GetLessonResultIdsAsync(GetUnitByUnitTestQuery request, Guid studentId)
        {
            return await _lessonResultRepository.ReadQueryable.Where(x => x.StudentId == studentId && x.UnitId == request.UnitId && x.CourseId == request.CourseId).Select(x => x.Id).ToListAsync();
        }

        private static long GetSecond(VideoTimeCodeResult x)
        {
            TimeSpan timeDiff = x.UpdatedDate.HasValue ? x.UpdatedDate.Value - x.CreatedDate : default;
            return Convert.ToInt64(timeDiff.TotalSeconds);
        }
    }
}
