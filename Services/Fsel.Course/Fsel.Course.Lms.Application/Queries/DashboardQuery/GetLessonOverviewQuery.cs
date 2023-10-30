namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
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
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _trainingService = trainingService;
            _authContext = authContext;
        }

        public async Task<MethodResult<LessonOverviewModel>> Handle(GetLessonOverviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonOverviewModel> methodResult = new MethodResult<LessonOverviewModel>();
            var lessonOverview = new LessonOverviewModel();
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
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
            var course = await GetCourse(@class.CourseId, studentId, cancellationToken);
            var courseResult = course?.CourseResults.FirstOrDefault();
            if (course == null || courseResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            if (courseResult.Status == EnumResultStatus.New)
            {
                var (lesson, objectId, type, objectStatus) = await GetLesson(default, studentId, course, cancellationToken);
                lessonOverview = GetLessonOverview(lesson, default, studentId, objectId, type, objectStatus);
            }
            else
            {
                var lessonResult = await _lessonResultRepository.GetAsync(studentId, @class.CourseId);
                var lesson = lessonResult?.Lesson;
                string? type = nameof(Lesson);
                Guid? objectId = lesson?.Id;
                EnumResultStatus? objectStatus = lessonResult?.Status;

                if ((lessonResult != null && lessonResult.Status == EnumResultStatus.Done) || lessonResult == null)
                {
                    (lesson, objectId, type, objectStatus) = await GetLesson(lessonResult, studentId, course, cancellationToken);
                    if (lessonResult != null)
                    {
                        lesson = lessonResult.Lesson;
                    }
                }
                if (courseResult.Status == EnumResultStatus.Done)
                {
                    lessonOverview = GetLessonOverview(lesson, lessonResult, studentId, @class.CourseId, nameof(Course), EnumResultStatus.Done, courseResult.SkillScores);
                }
                else
                {
                    lessonOverview = GetLessonOverview(lesson, lessonResult, studentId, objectId, type, objectStatus);
                }
            }
            methodResult.Result = lessonOverview;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Course?> GetCourse(Guid courseId, Guid? studentId, CancellationToken cancellationToken)
        {
            return await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Include(x => x.CourseResults.Where(x => x.StudentId == studentId)).FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
        }

        private (Guid?, string?, Guid?, EnumResultStatus?) HandleUnit(Domain.Entities.Unit unit, LessonResult? lessonResult, Guid? studentId)
        {
            if (unit.LessonResults.Any() && lessonResult?.UnitId == unit.Id)
            {
                return HandleLessonResult(unit, lessonResult, studentId);
            }
            else if (lessonResult != null && lessonResult.UnitId != unit.Id)
            {
                return (lessonResult.LessonId, nameof(Domain.Entities.Unit), lessonResult.UnitId, EnumResultStatus.New);
            }
            else
            {
                return (lessonResult?.LessonId, nameof(Lesson), lessonResult?.LessonId, EnumResultStatus.New);
            }
        }

        private (Guid?, string?, Guid?, EnumResultStatus?) HandleLessonResult(Domain.Entities.Unit unit, LessonResult? lessonResult, Guid? studentId)
        {
            var lessonResultCurrent = unit.LessonResults.Where(x => x.StudentId == studentId && x.Status != EnumResultStatus.Unfinished).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.UpdatedDate).FirstOrDefault();
            var lessonId = lessonResultCurrent?.LessonId;
            var mockTestResult = unit.MockTestResults.FirstOrDefault();
            if (unit.UnitSkillMockTests.Any() && unit.LessonResults.All(x => x.Status == EnumResultStatus.Done) && mockTestResult?.Status != EnumResultStatus.Done)
            {
                return (unit.UnitSkillMockTests.Select(x => x.MockTestId).FirstOrDefault(), nameof(EnumMockTestType.SkillMockTest), unit.UnitSkillMockTests.Select(x => x.MockTestId).FirstOrDefault(), EnumResultStatus.New);
            }
            else
            {
                return (lessonId, nameof(Lesson), lessonId, lessonResultCurrent?.Status);
            }
        }

        private async Task<(Lesson?, Guid?, string?, EnumResultStatus?)> GetLesson(LessonResult? lessonResult, Guid? studentId, Course course, CancellationToken cancellationToken)
        {
            var (unitId, objectId, type, objectStatus) = await GetUnitId(lessonResult, course, studentId);
            var unit = await _unitRepository.GetIncludeAsync(unitId, studentId);
            if (unit == null)
            {
                return default;
            }

            Guid? lessonId = lessonResult?.LessonId ?? unit.UnitLessons.OrderBy(x => x.DisplayOrder).FirstOrDefault()?.LessonId;
            if (type == nameof(Domain.Entities.Unit) && objectStatus != EnumResultStatus.New)
            {
                (lessonId, type, objectId, objectStatus) = HandleUnit(unit, lessonResult, studentId);
            }
            return (await GetLessonAsync(lessonId, cancellationToken), objectId, type, objectStatus);
        }

        private async Task<Lesson?> GetLessonAsync(Guid? lessonId, CancellationToken cancellationToken)
        {
            return await _lessonRepository.Queryable.Include(x => x.LessonInstructions).FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
        }

        private async Task<EnumResultStatus?> GetStatus(CourseUnitMockTest? courseUnitMockTest, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTest);
            var objectId = courseUnitMockTest.FinalTestId ?? courseUnitMockTest.MockTestId ?? courseUnitMockTest.UnitId;
            if (courseUnitMockTest.MockTestId.HasValue)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.MockTestId == objectId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == studentId).FirstOrDefaultAsync();
                return mockTestResult != null ? mockTestResult.Status : EnumResultStatus.New;
            }
            else if (courseUnitMockTest.FinalTestId.HasValue)
            {
                var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.FinalTestId == objectId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == studentId).FirstOrDefaultAsync();
                return finalTestResult != null ? finalTestResult.Status : EnumResultStatus.New;
            }
            else if (courseUnitMockTest.UnitId.HasValue)
            {
                var unitResult = await _unitResultRepository.Queryable.Where(x => x.UnitId == objectId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == studentId).FirstOrDefaultAsync();
                return unitResult != null ? unitResult.Status : EnumResultStatus.New;
            }
            return default;
        }

        private async Task<bool> IsDoneUnit(LessonResult lessonResult)
        {
            return await _unitResultRepository.Queryable.AnyAsync(x => x.StudentId == lessonResult.StudentId && x.UnitId == lessonResult.UnitId && x.CourseId == lessonResult.CourseId && x.Status == EnumResultStatus.Done);
        }

        private static CourseUnitMockTest? GetNextUnitWithMockTest(IList<CourseUnitMockTest>? courseUnitMockTests, int currentIndex)
        {
            return courseUnitMockTests?.Skip(currentIndex + 1).FirstOrDefault();
        }

        private async Task<(Guid?, Guid?, string?, EnumResultStatus?)> GetUnitId(LessonResult? lessonResult, Course course, Guid? studentId)
        {
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
            var courseUnitMockTest = courseUnitMockTests.FirstOrDefault();
            var unitId = lessonResult?.UnitId ?? courseUnitMockTest?.UnitId;
            if (lessonResult != null && await IsDoneUnit(lessonResult))
            {
                courseUnitMockTest = await GetCourseUnitMockTestFollow(courseUnitMockTests, unitId, studentId);
                return (unitId, GetObjectId(courseUnitMockTest), GetObjectType(courseUnitMockTest), courseUnitMockTest != null ? await GetStatus(courseUnitMockTest, studentId) : EnumResultStatus.Done);
            }
            return (unitId, unitId, nameof(Domain.Entities.Unit), await GetStatus(courseUnitMockTest, studentId));
        }

        private async Task<CourseUnitMockTest?> GetCourseUnitMockTestFollow(IList<CourseUnitMockTest>? courseUnitMockTests, Guid? unitId, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTests);
            var courseUnitMockTest = courseUnitMockTests.FirstOrDefault(x => x.UnitId == unitId);
            if (courseUnitMockTest != null)
            {
                var index = courseUnitMockTests.IndexOf(courseUnitMockTest);
                var courseUnitMockTestNext = GetNextUnitWithMockTest(courseUnitMockTests, index);
                var objectStatus = await GetStatus(courseUnitMockTestNext, studentId);
                if (objectStatus == EnumResultStatus.Done && courseUnitMockTestNext != null)
                {
                    courseUnitMockTestNext = GetNextUnitWithMockTest(courseUnitMockTests, courseUnitMockTests.IndexOf(courseUnitMockTestNext));
                }
                return courseUnitMockTestNext;
            }
            return default;
        }

        private static Guid? GetObjectId(CourseUnitMockTest? nextUnit)
        {
            return nextUnit?.MockTestId ?? nextUnit?.FinalTestId ?? nextUnit?.UnitId;
        }

        private string? GetObjectType(CourseUnitMockTest? nextUnit)
        {
            return (nextUnit?.MockTestId.HasValue ?? default) ? nameof(EnumMockTestType.FullMockTest) : (nextUnit?.FinalTestId.HasValue ?? default) ? nameof(FinalTest) : (nextUnit?.UnitId.HasValue ?? default) ? nameof(Domain.Entities.Unit) : default;
        }

        private LessonOverviewModel GetLessonOverview(Lesson? lesson, LessonResult? lessonResult, Guid? studentId, Guid? objectId, string? type, EnumResultStatus? status, IList<SkillScores>? skillScores = default)
        {
            ArgumentNullException.ThrowIfNull(lesson);
            var lessonDashBoard = _mapper.Map<LessonOverviewModel>(lesson);
            lessonDashBoard.LessonInstructions = _mapper.Map<IList<LessonInstructionModel>>(lesson.LessonInstructions.OrderBy(x => x.CreatedDate).ToList());
            lessonDashBoard.LessonResult = _mapper.Map<LessonResultModel>(lessonResult);
            lessonDashBoard.UnitId = lesson.UnitLessons.FirstOrDefault()?.UnitId ?? (lessonResult?.UnitId ?? default);
            lessonDashBoard.ObjectId = objectId;
            lessonDashBoard.Type = type;
            lessonDashBoard.Status = status;
            lessonDashBoard.SkillScores = skillScores;
            if (lessonResult != null)
            {
                var homeWorks = lessonResult.HomeWorkResults.Where(x => x.StudentId == studentId && x.LessonResultId == lessonResult.Id).ToList();
                var classForumResult = lessonResult.ClassForumResults.FirstOrDefault(x => x.StudentId == studentId && x.LessonResultId == lessonResult.Id);
                var (statusVideo, numberVideo) = GetStatus(lessonResult.VideoResult);
                var (statusClassForum, numberClassForum) = GetStatus(classForumResult, statusVideo);
                var (statusHomeWork, numberHomeWork) = GetStatus(homeWorks, statusClassForum);
                var numbers = new List<int> { numberClassForum, numberVideo, numberHomeWork };
                lessonDashBoard.StatusVideo = statusVideo;
                lessonDashBoard.StatusClassForum = statusClassForum;
                lessonDashBoard.StatusHomeWork = statusHomeWork;
                lessonDashBoard.PercentProgress = NumberHelper.ConvertPercentDouble((double)numbers.Average());
            }
            return lessonDashBoard;
        }

        private static (EnumResultStatus, int) GetStatus(VideoResult? videoResult)
        {
            if (videoResult != null)
            {
                if (videoResult.Status == EnumResultStatus.Process || videoResult.Status == EnumResultStatus.New)
                {
                    return (EnumResultStatus.Process, 0);
                }
                else
                {
                    return (EnumResultStatus.Done, 1);
                }
            }
            return (EnumResultStatus.Unfinished, 0);
        }

        private static (EnumResultStatus, int) GetStatus(IList<HomeWorkResult>? homeWorkResults, EnumResultStatus status)
        {
            var statusHomeWork = EnumResultStatus.Unfinished;
            if (status == EnumResultStatus.Process || status == EnumResultStatus.Done)
            {
                statusHomeWork = EnumResultStatus.Process;
            }
            if (homeWorkResults != null && homeWorkResults.Count > 0)
            {
                if (homeWorkResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    return (EnumResultStatus.Done, 1);
                }
            }
            return (statusHomeWork, 0);
        }

        private static (EnumResultStatus, int) GetStatus(ClassForumResult? classForumResult, EnumResultStatus status)
        {
            var statusClassForum = EnumResultStatus.Unfinished;
            if (status == EnumResultStatus.Done)
            {
                statusClassForum = EnumResultStatus.New;
            }
            if (classForumResult != null)
            {
                if (classForumResult.Status == EnumClassForumResultStatus.PendingForGrading || classForumResult.Status == EnumClassForumResultStatus.Graded)
                {
                    return (EnumResultStatus.Done, 1);
                }
                else if (classForumResult.Status == EnumClassForumResultStatus.Pending)
                {
                    return (EnumResultStatus.Process, 0);
                }
            }
            return (statusClassForum, 0);
        }
    }
}
