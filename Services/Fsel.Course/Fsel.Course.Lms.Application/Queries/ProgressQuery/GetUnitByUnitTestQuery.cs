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
            var unit = await _unitRepository.Queryable
                              .Include(x => x.LessonResults.Where(x => x.StudentId == studentId))
                              .Include(x => x.UnitLessons)
                              .ThenInclude(x => x.Lesson)
                              .ThenInclude(x => x!.LessonVideos)
                              .Include(x => x.CourseUnitMockTests)
                              .Where(x => x.CourseUnitMockTests.Any(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId))
                              .FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);

            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            var lessonResultIds = unit.LessonResults.Where(x => x.StudentId == studentId).Select(x => x.Id).ToList();
            var videoIds = unit.UnitLessons.Select(x => x.Lesson).SelectMany(x => x!.LessonVideos).Select(x => x!.VideoId).ToList();
            var videoResultIds = await _videoResultRepository.Queryable.Where(x => x.StudentId == studentId && lessonResultIds.Contains(x.LessonResultId)).Select(x => x.Id).ToListAsync(cancellationToken);
            var videos = await _videoRepository.Queryable.Include(x => x.VideoTimeCodes)
                                                        .Include(x => x.VideoTimeCodes)
                                                        .ThenInclude(x => x.TimeCodeExercises)
                                                        .ThenInclude(x => x.Exercise)
                                                        .ThenInclude(x => x!.ExerciseQuestions)
                                                        .ThenInclude(x => x.Question)
                                                        .ThenInclude(x => x.VideoTimeCodeAnswers)
                                                        .Where(x => videoIds.Contains(x.Id))
                                                        .AsNoTracking()
                                                        .ToListAsync(cancellationToken);
            if (videos == null || videos.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videos));
                return methodResult;
            }
            if (!request.Type.HasValue)
            {
                request.Type = EnumTimeCodeType.UnitTest;
            }

            var videoTimeCodes = videos.SelectMany(x => x.VideoTimeCodes).Where(x => x.TimeCodeType == request.Type).ToList();
            var exercises = videoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise).ToList();

            var skillScores = exercises.GroupBy(x => x!.CourseSkill).Select(x =>
            {
                var questions = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).ToList();
                var anwsers = questions.SelectMany(x => x!.VideoTimeCodeAnswers).ToList();
                var skillScore = new SkillScores
                {
                    Skill = x.Key,
                    CountQuestion = anwsers.Count,
                    TotalQuestion = questions.Count,
                    CorrectCount = anwsers.Sum(x => x.CorrectCount),
                    TotalCount = questions.Sum(x => x!.CorrectTotal),
                };
                return skillScore;
            }).ToList();
            skillScores.ForEach(x => x.Percent = NumberHelper.ConvertPercentDouble(x.CorrectCount / x.TotalCount));
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CourseSkills = skillScores.Select(x => x.Skill).ToList();
            overallScoreReport.CountQuestion = overallScoreReport.SkillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = overallScoreReport.SkillScores.Sum(x => x.TotalQuestion);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
