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
            LessonResult? lessonResult = default;
            Lesson? lesson = default;
            Guid? objectId = default;
            string? type = default;
            EnumResultStatus? objectStatus = default;
            IList<SkillScores>? skillScores = default;
            var course = await GetCourse(@class.CourseId, studentId, cancellationToken);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var courseResult = course.CourseResults.FirstOrDefault();
            if (courseResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            if (courseResult.Status == EnumResultStatus.New)
            {
                (lesson, objectId, type, objectStatus) = await GetLesson(lessonResult, studentId, course, cancellationToken);
            }
            else if (courseResult.Status != EnumResultStatus.New)
            {
                lessonResult = await _lessonResultRepository.GetAsync(studentId, @class.CourseId);
                lesson = lessonResult?.Lesson;
                if ((lessonResult != null && lessonResult.Status == EnumResultStatus.Done) || lessonResult == null)
                {
                    (lesson, objectId, type, objectStatus) = await GetLesson(lessonResult, studentId, course, cancellationToken);
                    if (lessonResult != null && lesson != null && lessonResult.LessonId != lesson.Id)
                    {
                        lessonResult = default;
                    }
                    else if (lessonResult != null)
                    {
                        lesson = lessonResult.Lesson;
                    }
                }
                else
                {
                    type = nameof(Lesson);
                    objectId = lesson?.Id;
                    objectStatus = lessonResult.Status;
                }
                if (courseResult.Status == EnumResultStatus.Done)
                {
                    skillScores = courseResult.SkillScores;
                    objectStatus = EnumResultStatus.Done;
                    objectId = @class.CourseId;
                    type = nameof(Course);
                }
            }

            if (lesson == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = GetLessonOverview(lesson, lessonResult, studentId, objectId, type, objectStatus, skillScores);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Course?> GetCourse(Guid courseId, Guid? studentId, CancellationToken cancellationToken)
        {
            return await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Include(x => x.CourseResults.Where(x => x.StudentId == studentId)).FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
        }

        private async Task<(Lesson?, Guid?, string?, EnumResultStatus?)> GetLesson(LessonResult? lessonResult, Guid? studentId, Course course, CancellationToken cancellationToken)
        {
            var (unitId, objectId, type, objectStatus) = await GetUnitId(lessonResult, course);
            var unit = await _unitRepository.Queryable.Include(x => x.UnitSkillMockTests).Include(x => x.UnitLessons).Include(x => x.LessonResults.Where(x => x.StudentId == studentId)).FirstOrDefaultAsync(x => x.Id == unitId, cancellationToken);
            if (unit == null)
            {
                return default;
            }

            Guid? lessonId = default;
            if (unit.LessonResults.Any())
            {
                var lessonResultCurrent = unit.LessonResults.Where(x => x.StudentId == studentId).OrderByDescending(x => x.CreatedDate).ThenBy(x => x.UpdatedDate).FirstOrDefault();
                lessonId = lessonResultCurrent?.LessonId;
                if (unit.UnitSkillMockTests.Any() && unit.LessonResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    type = nameof(EnumMockTestType.SkillMockTest);
                    objectId = unit.UnitSkillMockTests.Select(x => x.MockTestId).FirstOrDefault();
                    objectStatus = EnumResultStatus.New;
                }
                else
                {
                    type = nameof(Lesson);
                    objectId = lessonId;
                    objectStatus = lessonResultCurrent?.Status;
                }
            }
            else
            {
                lessonId = unit.UnitLessons.OrderBy(x => x.DisplayOrder).FirstOrDefault()?.LessonId;
                type = nameof(Lesson);
                objectId = lessonId;
                objectStatus = EnumResultStatus.New;
            }
            return (await _lessonRepository.Queryable.Include(x => x.LessonInstructions).FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken), objectId, type, objectStatus);
        }

        private async Task<EnumResultStatus?> GetStatus(CourseUnitMockTest courseUnitMockTest)
        {
            var objectId = courseUnitMockTest.FinalTestId ?? courseUnitMockTest.MockTestId;
            if (courseUnitMockTest.MockTestId.HasValue)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable
                .Where(x => x.MockTestId == objectId && x.CourseId == courseUnitMockTest.CourseId)
                .FirstOrDefaultAsync();
                return mockTestResult?.Status;
            }
            else if (courseUnitMockTest.FinalTestId.HasValue)
            {
                var finalTestResult = await _finalTestResultRepository.Queryable
                .Where(x => x.FinalTestId == objectId && x.CourseId == courseUnitMockTest.CourseId)
                .FirstOrDefaultAsync();
                return finalTestResult?.Status;
            }
            return default;
        }

        private async Task<bool> IsDoneUnit(LessonResult lessonResult)
        {
            return await _unitResultRepository.Queryable.AnyAsync(x => x.StudentId == lessonResult.StudentId && x.UnitId == lessonResult.UnitId && x.CourseId == lessonResult.CourseId && x.Status == EnumResultStatus.Done);
        }

        private static CourseUnitMockTest? GetNextUnitWithMockTest(List<CourseUnitMockTest> courseUnitMockTests, int currentIndex)
        {
            return courseUnitMockTests.Skip(currentIndex + 1).FirstOrDefault();
        }

        private async Task<(Guid?, Guid?, string?, EnumResultStatus?)> GetUnitId(LessonResult? lessonResult, Course course)
        {
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
            Guid? unitId = lessonResult?.UnitId ?? courseUnitMockTests.FirstOrDefault()?.UnitId;
            Guid? objectId = default;
            string? type = string.Empty;
            EnumResultStatus? objectStatus = default;
            if (lessonResult != null && await IsDoneUnit(lessonResult))
            {
                var courseUnit = courseUnitMockTests.FirstOrDefault(x => x.UnitId == lessonResult.UnitId);
                if (courseUnit != null)
                {
                    var index = courseUnitMockTests.IndexOf(courseUnit);
                    var nextUnit = GetNextUnitWithMockTest(courseUnitMockTests, index);
                    if (nextUnit != null && (nextUnit.MockTestId.HasValue || nextUnit.FinalTestId.HasValue))
                    {
                        objectStatus = await GetStatus(nextUnit);
                        if (objectStatus == EnumResultStatus.Done)
                        {
                            nextUnit = GetNextUnitWithMockTest(courseUnitMockTests, courseUnitMockTests.IndexOf(nextUnit));
                        }
                        objectId = nextUnit?.MockTestId ?? nextUnit?.FinalTestId;
                        type = (nextUnit?.MockTestId.HasValue ?? default) ? nameof(EnumMockTestType.FullMockTest) : (nextUnit?.FinalTestId.HasValue ?? default) ? nameof(FinalTest) : default;
                    }
                    if (nextUnit != null && nextUnit.UnitId.HasValue)
                    {
                        unitId = nextUnit.UnitId;
                        type = nameof(Domain.Entities.Unit);
                    }
                }
            }
            return (unitId, objectId, type, objectStatus);
        }

        private LessonOverviewModel GetLessonOverview(Lesson? lesson, LessonResult? lessonResult, Guid? studentId, Guid? objectId, string? type, EnumResultStatus? status, IList<SkillScores>? skillScores)
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
