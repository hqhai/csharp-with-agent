// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonOverviewQuery : IRequest<MethodResult<LessonDashboardModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetLessonDashboardQueryHandler : IRequestHandler<GetLessonOverviewQuery, MethodResult<LessonDashboardModel>>
    {
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetLessonDashboardQueryHandler(IUserService userService,
            ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IMapper mapper,
            AuthContext authContext)
        {
            _userService = userService;
            _lessonResultRepository = lessonResultRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<LessonDashboardModel>> Handle(GetLessonOverviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonDashboardModel> methodResult = new MethodResult<LessonDashboardModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            LessonResult? lessonResult = default;
            var studentId = studentsResult.Content?.Result?.Id;
            var course = await _courseRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId)).FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);
            if (course == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            if (course.Status != EnumCourseStatus.New)
            {
                lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Lesson)
                                                                   .ThenInclude(x => x!.LessonInstructions)
                                                                   .Include(x => x.VideoResult)
                                                                   .Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                                                   .Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                                   .Where(x => x.StudentId == studentId && (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process))
                                                                   .AsNoTracking()
                                                                   .FirstOrDefaultAsync(cancellationToken);

                if (lessonResult == null)
                {
                    lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Unit)
                                                                      .ThenInclude(x => x!.UnitSkillMockTests)
                                                                      .Include(x => x.Lesson)
                                                                      .ThenInclude(x => x!.LessonInstructions)
                                                                      .Include(x => x.VideoResult)
                                                                      .Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                                                      .Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                                      .Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done)
                                                                      .OrderByDescending(x => x.CreatedDate)
                                                                      .AsNoTracking()
                                                                      .FirstOrDefaultAsync(cancellationToken);
                }
            }
            var lesson = lessonResult?.Lesson;
            if (lessonResult == null)
            {
                var unitResult = course.UnitResults.FirstOrDefault(x => x.StudentId == studentId && x.CourseId == request.CourseId && x.Status != EnumResultStatus.Unfinished);
                if (unitResult != null)
                {
                    var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons.Where(x => x.DisplayOrder == 0)).FirstOrDefaultAsync(x => x.Id == unitResult.UnitId, cancellationToken);
                    if (unit != null)
                    {
                        var lessonId = unit.UnitLessons.FirstOrDefault(x => x.DisplayOrder == 0)?.LessonId ?? default;
                        lesson = await _lessonRepository.Queryable.Include(x => x.LessonInstructions).FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
                    }
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

        private LessonDashboardModel GetLessonOverview(Lesson? lesson, LessonResult? lessonResult, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(lesson);
            var lessonDashBoard = new LessonDashboardModel
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
                var (statusVideo, numberVideo) = GetStatusVideo(lessonResult.VideoResult);
                var (statusClassForum, numberClassForum) = GetStatusClassForums(classForumResult, lessonDashBoard.StatusVideo);
                var (statusHomeWork, numberHomeWork) = GetStatusHomeWorks(homeWorks, lessonDashBoard.StatusClassForum);
                var numbers = new List<int> { numberClassForum, numberVideo, numberHomeWork };
                lessonDashBoard.StatusVideo = statusVideo;
                lessonDashBoard.StatusClassForum = statusClassForum;
                lessonDashBoard.StatusHomeWork = statusHomeWork;
                lessonDashBoard.PercentProgress = Math.Round((double)numbers.Average() * 100, 0);
            }
            return lessonDashBoard;
        }

        private static (EnumResultStatus, int) GetStatusVideo(VideoResult? videoResult)
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

        private static (EnumResultStatus, int) GetStatusHomeWorks(IList<HomeWorkResult>? homeWorkResults, EnumResultStatus status)
        {
            var statusHomeWork = EnumResultStatus.Unfinished;
            if (status == EnumResultStatus.Done)
            {
                statusHomeWork = EnumResultStatus.Process;
            }
            if (homeWorkResults != null && homeWorkResults.Count > 0)
            {
                if (homeWorkResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    return (EnumResultStatus.Done, 1);
                }
                else if (homeWorkResults.All(x => x.Status == EnumResultStatus.Unfinished))
                {
                    return (EnumResultStatus.Unfinished, 0);
                }
                else if (homeWorkResults.Any(x => x.Status == EnumResultStatus.Process))
                {
                    return (EnumResultStatus.Process, 0);
                }
            }
            return (statusHomeWork, 0);
        }

        private static (EnumResultStatus, int) GetStatusClassForums(ClassForumResult? classForumResult, EnumResultStatus status)
        {
            var statusClassForum = EnumResultStatus.Unfinished;
            if (status == EnumResultStatus.Done)
            {
                statusClassForum = EnumResultStatus.Process;
            }
            if (classForumResult != null)
            {
                if (classForumResult.Status == EnumClassForumResultStatus.Graded)
                {
                    return (EnumResultStatus.Done, 1);
                }
                else if (classForumResult.Status == EnumClassForumResultStatus.Pending || classForumResult.Status == EnumClassForumResultStatus.PendingForGrading)
                {
                    return (EnumResultStatus.Process, 0);
                }
            }
            return (statusClassForum, 0);
        }
    }
}
