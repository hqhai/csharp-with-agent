// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallScoreByUnitTestQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByUnitTestQueryHandler : IRequestHandler<GetOverallScoreByUnitTestQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUnitRepository _unitRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;

        public GetOverallScoreByUnitTestQueryHandler(AuthContext authContext
            , IUnitRepository unitRepository
            , IVideoRepository videoRepository
            , IVideoResultRepository videoResultRepository
            , ICourseRepository courseRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _unitRepository = unitRepository;
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _courseRepository = courseRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByUnitTestQuery request, CancellationToken cancellationToken)
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

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            else if (course.CourseLevel.GetEnumCourseType() != EnumCourseType.Academic)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotTypeAcademic), nameof(course));
                return methodResult;
            }

            var units = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                              .ThenInclude(x => x.Lesson)
                              .ThenInclude(x => x!.LessonVideos)
                              .Include(x => x.CourseUnitMockTests)
                              .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                              .ToListAsync(cancellationToken);
            if (units == null || units.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }
            var videoIds = units.SelectMany(x => x.UnitLessons).Select(x => x.Lesson).SelectMany(x => x!.LessonVideos).Select(x => x.VideoId).ToList();
            var videoResultIds = await _videoResultRepository.Queryable.Where(x => x.StudentId == studentId && videoIds.Contains(x.VideoId)).Select(x => x.Id).ToListAsync(cancellationToken);
            var videos = await _videoRepository.Queryable.Include(x => x.VideoTimeCodes)
                                                    .ThenInclude(x => x.TimeCodeExercises)
                                                    .ThenInclude(x => x.Exercise)
                                                    .ThenInclude(x => x!.ExerciseQuestions)
                                                    .ThenInclude(x => x.Question)
                                                    .Include(x => x.VideoTimeCodes)
                                                    .ThenInclude(x => x.TimeCodeExercises)
                                                    .ThenInclude(x => x.Exercise)
                                                    .ThenInclude(x => x!.VideoTimeCodeAnswers)
                                                    .Where(x => videoIds.Contains(x.Id))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);
            if (videos == null || videos.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videos));
                return methodResult;
            }
            var videoTimeCodes = videos.SelectMany(x => x.VideoTimeCodes).Where(x => x.TimeCodeType == EnumTimeCodeType.UnitTest).ToList();
            var exercises = videoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise).ToList();

            var skillScores = exercises.GroupBy(x => x!.CourseSkill).Select(x =>
            {
                var videoResultAnswers = x.SelectMany(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoResultId.HasValue && videoResultIds.Contains(x.VideoResultId.Value)));
                return new SkillScores
                {
                    Skill = x.Key,
                    CountQuestion = videoResultAnswers.Count(),
                    TotalQuestion = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Count(),
                    CorrectCount = videoResultAnswers.Sum(x => x.CorrectCount),
                    TotalCount = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
                };
            }).ToList();
            skillScores.ForEach(x => x.Percent = x.TotalCount > 0 ? NumberHelper.ConvertPercentDouble(x.CorrectCount / x.TotalCount) : default);
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CountQuestion = overallScoreReport.SkillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = overallScoreReport.SkillScores.Sum(x => x.TotalQuestion);
            overallScoreReport.CourseSkills = exercises.Select(x => x!.CourseSkill).Distinct().ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
