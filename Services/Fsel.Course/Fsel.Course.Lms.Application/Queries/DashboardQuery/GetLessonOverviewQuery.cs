namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonOverviewQuery : IRequest<MethodResult<LessonOverviewModel>>
    {
    }

    public class GetLessonOverviewQueryHandler : IRequestHandler<GetLessonOverviewQuery, MethodResult<LessonOverviewModel>>
    {
        private const int PercentVideo = 50;
        private const int PercentClassForum = 20;
        private const int PercentHomeWork = 30;
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;

        public GetLessonOverviewQueryHandler(IUserService userService,
            ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IMockTestResultRepository mockTestResultRepository,
            IMapper mapper,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoResultRepository videoResultRepository,
            IVideoRepository videoRepository,
            IUnitResultRepository unitResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            ITrainingService trainingService,
            AuthContext authContext)
        {
            _userService = userService;
            _lessonResultRepository = lessonResultRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mapper = mapper;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoResultRepository = videoResultRepository;
            _videoRepository = videoRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _trainingService = trainingService;
            _authContext = authContext;
        }

        public async Task<MethodResult<LessonOverviewModel>> Handle(GetLessonOverviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonOverviewModel> methodResult = new MethodResult<LessonOverviewModel>();
            var method = await Validate();
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (studentId, courseId) = method.Result;
            var course = await _courseRepository.GetAsync(courseId, studentId);
            var courseResult = course?.CourseResults.FirstOrDefault();
            if (course == null || courseResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            LessonResult? lessonResult = default;
            if (courseResult.Status != EnumResultStatus.New)
            {
                lessonResult = await _lessonResultRepository.GetAsync(studentId, course.Id);
            }
            var (lesson, objectId, type, objectStatus) = await GetLesson(lessonResult, courseResult, course, cancellationToken);
            if (lesson == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = await GetLessonOverview(lesson, lessonResult, objectId, type, objectStatus, courseResult.Status == EnumResultStatus.Done ? courseResult.SkillScores : default);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<MethodResult<(Guid?, Guid)>> Validate()
        {
            MethodResult<(Guid?, Guid)> methodResult = new MethodResult<(Guid?, Guid)>();
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;
            var classResult = await _trainingService.GetClassByStudentId(studentId ?? default);
            if (!classResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError));
                return methodResult;
            }
            var @class = classResult?.Content?.Result;
            if (@class == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = (studentId, @class.CourseId);
            return methodResult;
        }

        private static (Guid?, Guid?, Guid?, string?, EnumLessonOverviewStatus?) GetInitValueUnit()
        {
            return default;
        }

        private async Task<(Guid?, string?, Guid?, EnumLessonOverviewStatus?)> HandleUnit(Domain.Entities.Unit unit, LessonResult? lessonResult, Guid? studentId)
        {
            if (unit.LessonResults.Any() && lessonResult?.UnitId == unit.Id)
            {
                return await HandleLessonResult(unit, lessonResult, studentId);
            }
            else if (lessonResult != null && lessonResult.UnitId != unit.Id)
            {
                return lessonResult.UnitId != unit.Id ? (lessonResult.LessonId, nameof(Domain.Entities.Unit), lessonResult.UnitId, EnumLessonOverviewStatus.Next) : (lessonResult.LessonId, nameof(Lesson), lessonResult.LessonId, EnumLessonOverviewStatus.Next);
            }
            return default;
        }

        private async Task<(Domain.Entities.Unit?, Guid?, string?, EnumLessonOverviewStatus?, bool)> GetUnit(LessonResult? lessonResult, CourseResult courseResult, Course course, CancellationToken cancellationToken)
        {
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ThenBy(x => x.CreatedDate).ToList();
            var (unitId, lessonId, objectId, type, objectStatus) = GetInitValueUnit();
            var unitFirstId = courseUnitMockTests.Select(x => x.UnitId).FirstOrDefault();
            (unitId, objectId, type, objectStatus) = await GetUnitId(lessonResult, courseUnitMockTests, courseResult.StudentId);
            if (courseResult.Status != EnumResultStatus.Process)
            {
                (objectId, type, objectStatus) = (course.Id, nameof(Course), GetStatusOverview(courseResult.Status, true));
            }
            var unit = await _unitRepository.GetIncludeAsync(unitId, courseResult.StudentId);
            return (unit, objectId, type, objectStatus, unitFirstId == unitId);
        }

        private async Task<(Lesson?, Guid?, string?, EnumLessonOverviewStatus?)> GetLesson(LessonResult? lessonResult, CourseResult courseResult, Course course, CancellationToken cancellationToken)
        {
            var (unit, objectId, type, objectStatus, isUnitFirst) = await GetUnit(lessonResult, courseResult, course, cancellationToken);
            if (unit == null)
            {
                return default;
            }
            var lessonFirstId = unit.UnitLessons.OrderBy(x => x.DisplayOrder).Select(x => x.LessonId).FirstOrDefault();
            Guid? lessonId = lessonResult?.LessonId ?? lessonFirstId;
            if (type == nameof(Domain.Entities.Unit))
            {
                if (lessonResult != null && objectStatus != EnumLessonOverviewStatus.Next)
                {
                    (lessonId, type, objectId, objectStatus) = await HandleUnit(unit, lessonResult, courseResult.StudentId);
                }
                else if (lessonId == lessonFirstId && isUnitFirst && courseResult.Status == EnumResultStatus.Process)
                {
                    (objectId, type, objectStatus) = (lessonFirstId, nameof(Lesson), EnumLessonOverviewStatus.StartNow);
                }
            }

            return (await _lessonRepository.GetAsync(lessonId), objectId, type, objectStatus);
        }

        private async Task<(Guid?, string?, Guid?, EnumLessonOverviewStatus?)> HandleLessonResult(Domain.Entities.Unit unit, LessonResult? lessonResult, Guid? studentId)
        {
            var lessonResultCurrent = unit.LessonResults.Where(x => x.StudentId == studentId && x.Status != EnumResultStatus.Unfinished && x.Status != EnumResultStatus.New).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.UpdatedDate).FirstOrDefault();
            var mockTestResult = unit.MockTestResults.FirstOrDefault();
            if (unit.LessonResults.All(x => x.Status == EnumResultStatus.Done) && mockTestResult != null && mockTestResult.Status != EnumResultStatus.Done)
            {
                return (lessonResult?.LessonId, nameof(EnumMockTestType.SkillMockTest), mockTestResult.MockTestId, GetStatusOverview(mockTestResult.Status));
            }
            else if (lessonResultCurrent != null)
            {
                var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResultCurrent.Id);
                if (videoResult != null && videoResult.Status != EnumResultStatus.Done && videoResult.CurrentVideoTimeCodeId.HasValue)
                {
                    var videoTimeCode = await _videoTimeCodeRepository.Queryable.Include(x => x.VideoTimeCodeResults.Where(x => x.VideoResultId == videoResult.Id)).FirstOrDefaultAsync(x => x.Id == videoResult.CurrentVideoTimeCodeId.Value);
                    var videoTimeCodeResult = videoTimeCode?.VideoTimeCodeResults.FirstOrDefault();
                    if (videoTimeCode != null && videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone && videoTimeCodeResult != null && videoTimeCodeResult.Status != EnumResultStatus.Done)
                    {
                        return videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest ? (lessonResultCurrent.LessonId, nameof(EnumTimeCodeType.UnitTest), videoTimeCode.Id, GetStatusOverview(videoTimeCodeResult.Status)) : (lessonResultCurrent.LessonId, nameof(EnumTimeCodeType.SkillTest), videoTimeCode.Id, GetStatusOverview(videoTimeCodeResult.Status));
                    }
                }
                if (lessonResultCurrent.Status == EnumResultStatus.Done)
                {
                    var lessonResultNew = unit.LessonResults.FirstOrDefault(x => x.Status == EnumResultStatus.New);
                    if (lessonResultNew != null)
                    {
                        return (lessonResultCurrent.LessonId, nameof(Lesson), lessonResultNew.LessonId, GetStatusOverview(lessonResultNew.Status));
                    }
                }
                return (lessonResultCurrent.LessonId, nameof(Lesson), lessonResultCurrent.LessonId, GetStatusOverview(lessonResultCurrent.Status));
            }
            return default;
        }

        private static CourseUnitMockTest? GetNextUnitWithMockTest(IList<CourseUnitMockTest> courseUnitMockTests, int currentIndex)
        {
            return courseUnitMockTests.Skip(currentIndex + 1).FirstOrDefault();
        }

        private async Task<(Guid?, Guid?, string?, EnumLessonOverviewStatus?)> GetUnitId(LessonResult? lessonResult, IList<CourseUnitMockTest>? courseUnitMockTests, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTests);
            var courseUnitMockTest = courseUnitMockTests.FirstOrDefault();
            var unitId = lessonResult?.UnitId ?? courseUnitMockTest?.UnitId;
            if (lessonResult != null && await _unitResultRepository.IsDoneAsync(lessonResult))
            {
                courseUnitMockTest = await GetCourseUnitMockTestFollow(courseUnitMockTests, unitId, studentId);
                unitId = courseUnitMockTest?.UnitId ?? unitId;
            }
            return (unitId, GetObjectId(courseUnitMockTest), GetObjectType(courseUnitMockTest), await GetStatusLessonOverview(courseUnitMockTest, studentId));
        }

        private async Task<EnumLessonOverviewStatus> GetStatusLessonOverview(CourseUnitMockTest? courseUnitMockTest, Guid? studentId)
        {
            var status = await GetStatus(courseUnitMockTest, studentId);
            return GetStatusOverview(status);
        }

        private static EnumLessonOverviewStatus GetStatusOverview(EnumResultStatus? status, bool isStart = false)
        {
            return status == EnumResultStatus.New ? isStart ? EnumLessonOverviewStatus.StartNow : EnumLessonOverviewStatus.Next : status == EnumResultStatus.Process ? EnumLessonOverviewStatus.Continue : EnumLessonOverviewStatus.Done;
        }

        private async Task<CourseUnitMockTest?> GetCourseUnitMockTestFollow(IList<CourseUnitMockTest>? courseUnitMockTests, Guid? unitId, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTests);
            var courseUnitMockTest = courseUnitMockTests.FirstOrDefault(x => x.UnitId == unitId);
            if (courseUnitMockTest == null)
            {
                return default;
            }
            var index = courseUnitMockTests.IndexOf(courseUnitMockTest);
            var courseUnitMockTestNext = GetNextUnitWithMockTest(courseUnitMockTests, index);
            if (courseUnitMockTestNext != null && await GetStatus(courseUnitMockTestNext, studentId) == EnumResultStatus.Done)
            {
                courseUnitMockTestNext = GetNextUnitWithMockTest(courseUnitMockTests, courseUnitMockTests.IndexOf(courseUnitMockTestNext));
            }
            return courseUnitMockTestNext;
        }

        private static Guid? GetObjectId(CourseUnitMockTest? nextUnit)
        {
            return nextUnit?.MockTestId ?? nextUnit?.FinalTestId ?? nextUnit?.UnitId;
        }

        private string? GetObjectType(CourseUnitMockTest? nextUnit)
        {
            return (nextUnit?.MockTestId.HasValue ?? default) ? nameof(EnumMockTestType.FullMockTest) : (nextUnit?.FinalTestId.HasValue ?? default) ? nameof(FinalTest) : (nextUnit?.UnitId.HasValue ?? default) ? nameof(Domain.Entities.Unit) : default;
        }

        private async Task<LessonOverviewModel> GetLessonOverview(Lesson? lesson, LessonResult? lessonResult, Guid? objectId, string? type, EnumLessonOverviewStatus? status, IList<SkillScores>? skillScores = default)
        {
            var lessonDashBoard = _mapper.Map<LessonOverviewModel>(lesson);
            lessonDashBoard.LessonInstructions = _mapper.Map<IList<LessonInstructionModel>>(lesson?.LessonInstructions.OrderBy(x => x.CreatedDate).ToList());
            lessonDashBoard.LessonResult = _mapper.Map<LessonResultModel>(lessonResult);
            lessonDashBoard.UnitId = lesson?.UnitLessons.FirstOrDefault()?.UnitId ?? (lessonResult?.UnitId ?? default);
            (lessonDashBoard.ObjectId, lessonDashBoard.Type, lessonDashBoard.Status, lessonDashBoard.SkillScores) = (objectId, type, status, skillScores);
            if (lessonResult != null)
            {
                (lessonDashBoard.StatusVideo, lessonDashBoard.StatusClassForum, lessonDashBoard.StatusHomeWork, lessonDashBoard.PercentProgress) = await GetStatus(lessonResult);
            }
            return lessonDashBoard;
        }

        private async Task<EnumResultStatus?> GetStatus(CourseUnitMockTest? courseUnitMockTest, Guid? studentId)
        {
            if (courseUnitMockTest != null)
            {
                var objectId = GetObjectId(courseUnitMockTest);
                if (courseUnitMockTest.MockTestId.HasValue)
                {
                    var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestId == objectId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == studentId);
                    return mockTestResult != null ? mockTestResult.Status : EnumResultStatus.New;
                }
                else if (courseUnitMockTest.FinalTestId.HasValue)
                {
                    var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestId == objectId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == studentId);
                    return finalTestResult != null ? finalTestResult.Status : EnumResultStatus.New;
                }
                else if (courseUnitMockTest.UnitId.HasValue)
                {
                    var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == objectId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == studentId);
                    return unitResult != null ? unitResult.Status : EnumResultStatus.New;
                }
            }
            return default;
        }

        private async Task<(EnumResultStatus, EnumResultStatus, EnumResultStatus, double)> GetStatus(LessonResult lessonResult)
        {
            var homeWorks = lessonResult.HomeWorkResults.ToList();
            var classForumResult = lessonResult.ClassForumResults.FirstOrDefault();
            var (statusVideo, numberVideo) = await GetStatus(lessonResult.VideoResult);
            var (statusClassForum, numberClassForum) = GetStatus(classForumResult, statusVideo);
            var (statusHomeWork, numberHomeWork) = GetStatus(homeWorks, statusClassForum);
            var numbers = new List<double> { numberClassForum, numberVideo, numberHomeWork };
            return (statusVideo, statusClassForum, statusHomeWork, numbers.Sum());
        }

        private async Task<(EnumResultStatus, double)> GetStatus(VideoResult? videoResult)
        {
            var status = EnumResultStatus.Unfinished;
            if (videoResult != null)
            {
                var video = await _videoRepository.Queryable.Include(x => x.VideoTimeCodes).ThenInclude(x => x.VideoTimeCodeResults.Where(x => x.StudentId == videoResult.StudentId))
                                                            .FirstOrDefaultAsync(x => x.Id == videoResult.VideoId);
                if (video != null)
                {
                    var videoTimeCodes = video.VideoTimeCodes.ToList();
                    var videoTimeCodeResults = videoTimeCodes.SelectMany(x => x.VideoTimeCodeResults).Where(x => x.Status == EnumResultStatus.Done).ToList();
                    status = (videoResult.Status == EnumResultStatus.Process || videoResult.Status == EnumResultStatus.New) ? EnumResultStatus.Process : EnumResultStatus.Done;
                    var percent = NumberHelper.ConvertDoublePercent(PercentVideo * NumberHelper.GetPercent(videoTimeCodeResults.Count, videoTimeCodes.Count));
                    return (status, percent);
                }
            }
            return (status, default);
        }

        private static (EnumResultStatus, double) GetStatus(IList<HomeWorkResult>? homeWorkResults, EnumResultStatus status)
        {
            var statusHomeWork = (status == EnumResultStatus.Process || status == EnumResultStatus.Done) ? EnumResultStatus.Process : EnumResultStatus.Unfinished;
            if (homeWorkResults != null && homeWorkResults.Any())
            {
                var countDone = homeWorkResults.Where(x => x.Status == EnumResultStatus.Done).Count();
                if (homeWorkResults.Count == countDone)
                {
                    var percent = NumberHelper.ConvertDoublePercent(PercentHomeWork * NumberHelper.GetPercent(countDone, homeWorkResults.Count));
                    return (EnumResultStatus.Done, percent);
                }
            }
            return (statusHomeWork, default);
        }

        private static (EnumResultStatus, double) GetStatus(ClassForumResult? classForumResult, EnumResultStatus status)
        {
            if (classForumResult != null)
            {
                if (classForumResult.Status == EnumClassForumResultStatus.PendingForGrading || classForumResult.Status == EnumClassForumResultStatus.Graded)
                {
                    return (EnumResultStatus.Done, PercentClassForum);
                }
                else if (classForumResult.Status == EnumClassForumResultStatus.Pending)
                {
                    return (EnumResultStatus.Process, default);
                }
            }
            return (status == EnumResultStatus.Done ? EnumResultStatus.New : EnumResultStatus.Unfinished, default);
        }
    }
}
