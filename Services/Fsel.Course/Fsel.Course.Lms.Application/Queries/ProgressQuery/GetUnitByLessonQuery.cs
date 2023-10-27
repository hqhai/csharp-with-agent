// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByLessonQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetUnitByLessonQueryHandler : IRequestHandler<GetUnitByLessonQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUserService _userService;

        public GetUnitByLessonQueryHandler(AuthContext authContext
            , IVideoRepository videoRepository
            , IVideoResultRepository videoResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetUnitByLessonQuery request, CancellationToken cancellationToken)
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
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var video = await _videoRepository.Queryable.Include(x => x.VideoTimeCodes)
                                        .ThenInclude(x => x.TimeCodeExercises)
                                        .ThenInclude(x => x.Exercise)
                                        .ThenInclude(x => x!.ExerciseQuestions)
                                        .ThenInclude(x => x.Question)
                                        .Include(x => x.VideoTimeCodes)
                                        .ThenInclude(x => x.TimeCodeExercises)
                                        .ThenInclude(x => x.Exercise)
                                        .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id))
                                        .Where(x => x.Id == videoResult.VideoId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(cancellationToken);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }
            var videoTimeCodes = video.VideoTimeCodes.Where(x => x.TimeCodeType == EnumTimeCodeType.Standalone).ToList();
            var exercises = videoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise).ToList();
            var skillScores = exercises.GroupBy(x => x!.CourseSkill).Select(x => new SkillScores
            {
                Skill = x.Key,
                CountQuestion = x.SelectMany(x => x!.VideoTimeCodeAnswers).Count(),
                TotalQuestion = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Count(),
                CorrectCount = x.SelectMany(x => x!.VideoTimeCodeAnswers).Sum(x => x.CorrectCount),
                TotalCount = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
            }).ToList();

            skillScores.ForEach(x => x.Percent = NumberHelper.GetPercent(x.CorrectCount, x.TotalCount));
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CountQuestion = skillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = skillScores.Sum(x => x.TotalQuestion);
            overallScoreReport.CourseSkills = exercises.Select(x => x!.CourseSkill).Distinct().ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
