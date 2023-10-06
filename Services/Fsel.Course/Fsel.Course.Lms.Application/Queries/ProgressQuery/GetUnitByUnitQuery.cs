// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
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

            var units = await _unitRepository.GetListAsync(studentId, request.CourseId, request.Type);
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
                var lessonResultIds = x.LessonResults.Select(x => x.Id).ToList();
                var videoIds = x.UnitLessons.Select(x => x.Lesson).SelectMany(x => x!.LessonVideos).Select(x => x!.VideoId).ToList();
                var videoResultIds = await _videoResultRepository.Queryable.Where(x => x.StudentId == studentId && lessonResultIds.Contains(x.LessonResultId)).Select(x => x.Id).ToListAsync();
                var videos = await _videoRepository.GetListAsync(videoIds, videoResultIds);
                if (videos == null || videos.Count == 0)
                {
                    return (default, default);
                }
                var videoTimeCodes = videos.SelectMany(x => x.VideoTimeCodes).Where(x => x.TimeCodeType == EnumTimeCodeType.UnitTest).ToList();
                var exercises = videoTimeCodes.SelectMany(x => x.TimeCodeExercises).Where(x => x.Exercise != null).Select(x => x.Exercise ?? new Exercise()).ToList();

                var skillScores = exercises.GroupBy(x => x!.CourseSkill).Select(x => GetSkillScores(x)).ToList();
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
            return (default, default);
        }

        private static SkillScores GetSkillScores(IGrouping<EnumCourseSkill, Exercise> x)
        {
            var questions = x.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).ToList();
            var correctCount = questions.SelectMany(x => x!.VideoTimeCodeAnswers).Sum(x => x.CorrectCount);
            var correctTotal = questions.Sum(x => x!.CorrectTotal);

            var totalQuestion = questions.Count;
            var countQuestion = questions.SelectMany(x => x!.VideoTimeCodeAnswers).Count();
            var percent = NumberHelper.ConvertPercentDouble((double)correctCount / correctTotal);
            return new SkillScores
            {
                Skill = x.Key,
                CountQuestion = countQuestion,
                TotalQuestion = totalQuestion,
                CorrectCount = correctCount,
                TotalCount = correctTotal,
                Percent = percent
            };
        }

        private UnitModel GetUnitModel(Unit x, Guid courseId, double totalDone, double countDone)
        {
            var displayOrder = x.CourseUnitMockTests.FirstOrDefault(y => y.CourseId == courseId && y.UnitId == x.Id)?.DisplayOrder ?? default;
            var unitModel = new UnitModel
            {
                Id = x.Id,
                Code = x.Code,
                CourseLevel = x.CourseLevel,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                IsActive = x.CourseUnitMockTests.Any(),
                Name = x.Name,
                DisplayOrder = displayOrder,
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
