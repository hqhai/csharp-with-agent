// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
    using AutoMapper;
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
    using Unit = Course.Domain.Entities.Unit;

    public class GetUnitByUnitQuery : IRequest<MethodResult<IList<UnitModel>>>
    {
        public Guid CourseId { get; set; }
        public EnumProcessType Type { get; set; }
    }

    public class GetUnitByUnitVideoQueryHandler : IRequestHandler<GetUnitByUnitQuery, MethodResult<IList<UnitModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;

        public GetUnitByUnitVideoQueryHandler(AuthContext authContext
            , IMapper mapper
            , ICourseRepository courseRepository
            , IVideoRepository videoRepository
            , IVideoResultRepository videoResultRepository
            , IUnitRepository unitRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _mapper = mapper;
            _courseRepository = courseRepository;
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _unitRepository = unitRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<UnitModel>>> Handle(GetUnitByUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UnitModel>> methodResult = new MethodResult<IList<UnitModel>>();
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
            else if (course.CourseType == EnumCourseType.Ielts && request.Type == EnumProcessType.UnitTest)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotTypeAcademic), nameof(course));
                return methodResult;
            }

            var units = new List<Unit>();
            if (request.Type == EnumProcessType.LessonVideo)
            {
                units = await _unitRepository.Queryable.Include(x => x.UnitLessons).ThenInclude(x => x.Lesson)
                               .Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                               .Include(x => x.CourseUnitMockTests)
                               .Include(x => x.LessonResults.Where(x => x.StudentId == studentId))
                               .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                               .AsNoTracking()
                               .ToListAsync(cancellationToken);
            }
            else if (request.Type == EnumProcessType.HomeWork)
            {
                units = await _unitRepository.Queryable.Include(x => x.UnitLessons).ThenInclude(x => x.Lesson).ThenInclude(x => x!.LessonHomeWorks).ThenInclude(x => x.HomeWork)
                               .Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                               .Include(x => x.CourseUnitMockTests)
                               .Include(x => x.LessonResults.Where(x => x.StudentId == studentId))
                               .ThenInclude(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                               .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                               .AsNoTracking()
                               .ToListAsync(cancellationToken);
            }
            else if (request.Type == EnumProcessType.ClassForum)
            {
                units = await _unitRepository.Queryable.Include(x => x.UnitLessons).ThenInclude(x => x.Lesson).ThenInclude(x => x!.ClassForum)
                             .Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                             .Include(x => x.CourseUnitMockTests)
                             .Include(x => x.LessonResults.Where(x => x.StudentId == studentId))
                             .ThenInclude(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                             .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                             .AsNoTracking()
                             .ToListAsync(cancellationToken);
            }
            else if (request.Type == EnumProcessType.UnitTest)
            {
                units = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                              .Include(x => x.UnitLessons)
                              .ThenInclude(x => x.Lesson)
                              .ThenInclude(x => x!.LessonVideos)
                              .Include(x => x.CourseUnitMockTests)
                              .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                              .AsNoTracking()
                              .ToListAsync(cancellationToken);
            }

            if (units == null || units.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }
            var unitModels = new List<UnitModel>();
            foreach (var item in units)
            {
                var (countDone, totalDone) = await GetCountDone(item, request.Type, studentId);
                unitModels.Add(GetUnitModel(item, request.CourseId, totalDone, countDone));
            }
            unitModels = unitModels.OrderBy(x => x.DisplayOrder).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = unitModels;
            return methodResult;
        }

        private async Task<(double, double)> GetCountDone(Unit x, EnumProcessType type, Guid? studentId)
        {
            if (type == EnumProcessType.LessonVideo)
            {
                var countDone = x.LessonResults.Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done).Count();
                var totalDone = x.UnitLessons.Select(x => x.Lesson).Count();
                return (countDone, totalDone);
            }
            else if (type == EnumProcessType.HomeWork)
            {
                var homeWorks = x.UnitLessons.Select(x => x.Lesson).SelectMany(x => x.LessonHomeWorks).Select(x => x.HomeWork).ToList();
                var countDone = x.LessonResults.SelectMany(x => x.HomeWorkResults).Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done).Count();
                return (countDone, homeWorks.Count);
            }
            else if (type == EnumProcessType.UnitTest)
            {
                var videoIds = x.UnitLessons.Select(x => x.Lesson).SelectMany(x => x!.LessonVideos).Select(x => x!.VideoId).ToList();
                var videoResultIds = await _videoResultRepository.Queryable.Where(x => x.StudentId == studentId && videoIds.Contains(x.VideoId)).Select(x => x.Id).ToListAsync();
                var videos = await _videoRepository.Queryable.Include(x => x.VideoTimeCodes)
                                                            .ThenInclude(x => x.VideoTimeCodeResults.Where(y => videoResultIds.Contains(y.VideoResultId)))
                                                            .ThenInclude(x => x.VideoTimeCodeAnswers)
                                                            .Include(x => x.VideoTimeCodes)
                                                            .ThenInclude(x => x.TimeCodeExercises)
                                                            .ThenInclude(x => x.Exercise)
                                                            .ThenInclude(x => x!.ExerciseQuestions)
                                                            .ThenInclude(x => x.Question)
                                                            .Where(x => videoIds.Contains(x.Id))
                                                            .ToListAsync();
                if (videos == null || videos.Count == 0)
                {
                    return (0, 0);
                }
                var videoTimeCodes = videos.SelectMany(x => x.VideoTimeCodes).Where(x => x.TimeCodeType == EnumTimeCodeType.UnitTest).ToList();
                var exercises = videoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise).ToList();

                var skillScores = exercises.GroupBy(x => x!.CourseSkill).Select(x => new SkillScores
                {
                    Skill = x.Key,
                    CountQuestion = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Count(),
                    TotalQuestion = x.SelectMany(x => x!.VideoTimeCodeAnswers).Count(),
                    CorrectCount = x.SelectMany(x => x!.VideoTimeCodeAnswers).Sum(x => x.CorrectCount),
                    TotalCount = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
                    Percent = NumberHelper.ConvertPercentDouble(x.SelectMany(x => x!.VideoTimeCodeAnswers).Sum(x => x.CorrectCount) / x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal))
                }).ToList();
                var countDone = skillScores.Sum(x => x.CountQuestion);
                var totalDone = skillScores.Sum(x => x.TotalQuestion);
                return (countDone, totalDone);
            }
            else if (type == EnumProcessType.ClassForum)
            {
                var countDone = x.LessonResults.SelectMany(x => x.ClassForumResults).Where(x => x.StudentId == studentId && (x.Status == EnumClassForumResultStatus.Graded || x.Status == EnumClassForumResultStatus.PendingForGrading)).Count();
                var totalDone = x.UnitLessons.Select(x => x.Lesson).Select(x => x!.ClassForum).Count();
                return (countDone, totalDone);
            }
            return (0, 0);
        }

        private UnitModel GetUnitModel(Unit x, Guid courseId, double totalDone, double countDone)
        {
            var unitModel = new UnitModel
            {
                Id = x.Id,
                Code = x.Code,
                CourseLevel = x.CourseLevel,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                IsActive = true,
                Name = x.Name,
                DisplayOrder = x.CourseUnitMockTests.FirstOrDefault(y => y.CourseId == courseId && y.UnitId == x.Id)?.DisplayOrder ?? default,
                UpdatedDate = x.UpdatedDate,
                UpdatedFullName = x.UpdatedFullName,
                UpdatedUserId = x.UpdatedUserId,
                UnitResult = _mapper.Map<UnitResultModel>(x.UnitResults.FirstOrDefault()),
                Percent = totalDone > 0 ? NumberHelper.ConvertPercentDouble(countDone / totalDone) : default,
            };
            return unitModel;
        }
    }
}
