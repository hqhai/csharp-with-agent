// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgessQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByUnitTestQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid UnitId { get; set; }
    }

    public class GetUnitByUnitTestQueryHandler : IRequestHandler<GetUnitByUnitTestQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUnitRepository _unitRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUserService _userService;

        public GetUnitByUnitTestQueryHandler(AuthContext authContext
            , IUnitRepository unitRepository
            , IVideoRepository videoRepository
            , IVideoResultRepository videoResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _unitRepository = unitRepository;
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetUnitByUnitTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;
            var units = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                              .ThenInclude(x => x.Lesson)
                              .ThenInclude(x => x!.LessonVideos)
                              .Where(x => x.Id == request.UnitId)
                              .ToListAsync(cancellationToken);
            var videoIds = units.SelectMany(x => x.UnitLessons).Select(x => x.Lesson).SelectMany(x => x!.LessonVideos).Select(x => x!.VideoId).ToList();
            var videoResultIds = await _videoResultRepository.Queryable.Where(x => x.StudentId == studentId && videoIds.Contains(x.VideoId)).Select(x => x.Id).ToListAsync(cancellationToken);
            var videos = await _videoRepository.Queryable.Include(x => x.VideoTimeCodes)
                                                        .ThenInclude(x => x.VideoTimeCodeAnswers)
                                                        .Include(x => x.VideoTimeCodes)
                                                        .ThenInclude(x => x.TimeCodeExercises)
                                                        .ThenInclude(x => x.Exercise)
                                                        .ThenInclude(x => x!.ExerciseQuestions)
                                                        .ThenInclude(x => x.Question)
                                                        .Where(x => videoIds.Contains(x.Id))
                                                        .ToListAsync(cancellationToken);

            var videoTimeCodes = videos.SelectMany(x => x.VideoTimeCodes).Where(x => x.TimeCodeType == EnumTimeCodeType.UnitTest).ToList();
            var exercises = videoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise).ToList();

            var skillScores = exercises.GroupBy(x => x!.CourseSkill).Select(x => new SkillScores
            {
                Skill = x.Key,
                CountQuestion = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Count(),
                TotalQuestion = x.SelectMany(x => x!.VideoTimeCodeAnswers).Where(y => videoResultIds.Contains(y.VideoResultId)).Count(),
                CorrectCount = x.SelectMany(x => x!.VideoTimeCodeAnswers).Where(y => videoResultIds.Contains(y.VideoResultId)).Sum(x => x.CorrectCount),
                TotalCount = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
            }).ToList();
            skillScores.ForEach(x => x.Percent = x.CorrectCount / x.TotalCount);
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CountQuestion = overallScoreReport.SkillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = overallScoreReport.SkillScores.Sum(x => x.TotalQuestion);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
