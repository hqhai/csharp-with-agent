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
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
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
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;

        public GetLessonOverviewQueryHandler(IUserService userService,
            ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IMockTestResultRepository mockTestResultRepository,
            IMapper mapper,
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
            var course = await GetCourse(@class.CourseId, cancellationToken);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            if (course.Status != EnumCourseStatus.New)
            {
                lessonResult = await _lessonResultRepository.GetAsync(studentId, @class.CourseId);
            }
            var lesson = lessonResult?.Lesson;
            if ((lessonResult != null && lessonResult.Status == EnumResultStatus.Done) || lessonResult == null)
            {
                lesson = await GetLesson(lessonResult, course, cancellationToken);
                if (lessonResult != null && lesson != null && lessonResult.LessonId != lesson.Id)
                {
                    lessonResult = default;
                }
                else if (lessonResult != null)
                {
                    lesson = lessonResult.Lesson;
                }
            }

            if (lesson == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = GetLessonOverview(lesson, lessonResult, studentId);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Course?> GetCourse(Guid courseId, CancellationToken cancellationToken)
        {
            return await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
        }

        private async Task<Lesson?> GetLesson(LessonResult? lessonResult, Course course, CancellationToken cancellationToken)
        {
            var unitId = await GetUnitId(lessonResult, course);
            var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons).Include(x => x.LessonResults).FirstOrDefaultAsync(x => x.Id == unitId, cancellationToken);
            if (unit != null)
            {
                Guid? lessonId = default;
                if (unit.LessonResults.Any())
                {
                    lessonId = unit.LessonResults.OrderByDescending(x => x.CreatedDate).ThenBy(x => x.UpdatedDate).FirstOrDefault()?.LessonId;
                }
                else
                {
                    lessonId = unit.UnitLessons.OrderBy(x => x.DisplayOrder).FirstOrDefault()?.LessonId;
                }
                return await _lessonRepository.Queryable.Include(x => x.LessonInstructions).FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
            }
            return default;
        }

        private async Task<bool> IsMockTestDone(Course course, Guid mockTestId)
        {
            var mockTestResult = await _mockTestResultRepository.Queryable
                .Where(x => x.MockTestId == mockTestId && x.CourseId == course.Id)
                .FirstOrDefaultAsync();

            return mockTestResult != null && mockTestResult.Status == EnumResultStatus.Done;
        }

        private static CourseUnitMockTest? GetNextUnitWithMockTest(List<CourseUnitMockTest> courseUnitMockTests, int currentIndex)
        {
            return courseUnitMockTests.Skip(currentIndex + 1).FirstOrDefault();
        }

        private async Task<Guid?> GetUnitId(LessonResult? lessonResult, Course course)
        {
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
            Guid? unitId = lessonResult?.UnitId ?? courseUnitMockTests.FirstOrDefault()?.UnitId;
            if (lessonResult != null && lessonResult.Status == EnumResultStatus.Done)
            {
                var courseUnit = courseUnitMockTests.FirstOrDefault(x => x.UnitId == lessonResult.UnitId);
                if (courseUnit != null)
                {
                    var index = courseUnitMockTests.IndexOf(courseUnit);
                    var nextUnit = GetNextUnitWithMockTest(courseUnitMockTests, index);
                    while (nextUnit != null && nextUnit.MockTestId.HasValue && await IsMockTestDone(course, nextUnit.MockTestId.Value))
                    {
                        nextUnit = GetNextUnitWithMockTest(courseUnitMockTests, courseUnitMockTests.IndexOf(nextUnit));
                    }
                    if (nextUnit != null)
                    {
                        unitId = nextUnit.UnitId;
                    }
                }
            }
            return unitId;
        }

        private LessonOverviewModel GetLessonOverview(Lesson? lesson, LessonResult? lessonResult, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(lesson);
            var lessonDashBoard = new LessonOverviewModel
            {
                Id = lesson.Id,
                Name = lesson.Name,
                CourseLevel = lesson.CourseLevel,
                UnitId = lesson.UnitLessons.FirstOrDefault()?.UnitId ?? (lessonResult?.UnitId ?? default),
                InstructionContent = lesson.InstructionContent,
                LessonInstructions = _mapper.Map<IList<LessonInstructionModel>>(lesson.LessonInstructions.OrderBy(x => x.CreatedDate).ToList()),
                LessonResult = _mapper.Map<LessonResultModel>(lessonResult),
            };
            if (lessonResult != null)
            {
                var mockTestId = lessonResult.Unit?.UnitSkillMockTests.FirstOrDefault()?.MockTestId ?? null;
                if (mockTestId != null)
                {
                    lessonDashBoard.MockTestId = mockTestId;
                }
                var homeWorks = lessonResult.HomeWorkResults.Where(x => x.StudentId == studentId && x.LessonResultId == lessonResult.Id).ToList();
                var classForumResult = lessonResult.ClassForumResults.FirstOrDefault(x => x.StudentId == studentId && x.LessonResultId == lessonResult.Id);
                var (statusVideo, numberVideo) = GetStatus(lessonResult.VideoResult);
                var (statusClassForum, numberClassForum) = GetStatus(classForumResult, statusVideo);
                var (statusHomeWork, numberHomeWork) = GetStatus(homeWorks, statusClassForum);
                var numbers = new List<int> { numberClassForum, numberVideo, numberHomeWork };
                lessonDashBoard.StatusVideo = statusVideo;
                lessonDashBoard.StatusClassForum = statusClassForum;
                lessonDashBoard.StatusHomeWork = statusHomeWork;
                lessonDashBoard.PercentProgress = Math.Round((double)numbers.Average() * 100, 0);
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
