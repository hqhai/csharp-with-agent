// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery.V1i1
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.DashboardModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeNavigationTargetQuery : IRequest<MethodResult<HomeNavigationTargetModel>>
    {
    }

    public class GetHomeNavigationTargetQueryHandler : IRequestHandler<GetHomeNavigationTargetQuery, MethodResult<HomeNavigationTargetModel>>
    {
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly AuthContext _authContext;

        public GetHomeNavigationTargetQueryHandler(
            IUserService userService,
            ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IMockTestResultRepository mockTestResultRepository,
            IMapper mapper,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoResultRepository videoResultRepository,
            IUnitResultRepository unitResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            ICourseResultRepository courseResultRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
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
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _courseResultRepository = courseResultRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<HomeNavigationTargetModel>> Handle(GetHomeNavigationTargetQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<HomeNavigationTargetModel>();

            // 1) Student
            var studentMethod = await GetStudentModelAsync();
            if (!studentMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(studentMethod.ErrorMessages);
                return methodResult;
            }
            var student = studentMethod.Result!;

            // 2) CourseResult
            var courseResultMethod = await GetCourseResultAsync(student.Id, cancellationToken);
            if (!courseResultMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(courseResultMethod.ErrorMessages);
                return methodResult;
            }
            var courseResult = courseResultMethod.Result!;

            // 3) Course
            var courseMethod = await GetCourseAsync(courseResult, cancellationToken);
            if (!courseMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(courseMethod.ErrorMessages);
                return methodResult;
            }
            var course = courseMethod.Result!;

            // 4) LessonResult gần nhất
            var lessonResult = await GetLessonResultAsync(courseResult, cancellationToken);

            // 5) Xây lesson overview (điểm bắt đầu/tiếp tục)
            var lessonOverview = await BuildLessonOverviewAsync(lessonResult, course, courseResult, cancellationToken);

            // 6) Lấy lesson chi tiết để map ra dashboard
            var lesson = await _lessonRepository.GetByIdAsync(lessonOverview.LessonId ?? Guid.Empty);
            if (lesson == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var overall = MapLessonOverview(lesson, lessonResult, lessonOverview);
            overall.CourseId = course.Id;
            methodResult.Result = overall;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<Course>> GetCourseAsync(CourseResult courseResult, CancellationToken ct)
        {
            var methodResult = new MethodResult<Course>();
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(x => x.Id == courseResult.CourseId, ct);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            methodResult.Result = course;
            return methodResult;
        }

        public async Task<MethodResult<StudentModel>> GetStudentModelAsync()
        {
            var methodResult = new MethodResult<StudentModel>();

            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentsResult));
                return methodResult;
            }

            var student = studentsResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            methodResult.Result = student;
            return methodResult;
        }

        private async Task<MethodResult<CourseResult>> GetCourseResultAsync(Guid studentId, CancellationToken ct)
        {
            var methodResult = new MethodResult<CourseResult>();

            var courseResult = await _courseResultRepository.Queryable
                .Where(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }

            methodResult.Result = courseResult;
            return methodResult;
        }

        public async Task<LessonResult?> GetLessonResultAsync(CourseResult courseResult, CancellationToken ct)
        {
            // Ưu tiên bản mới nhất theo UpdatedDate, fallback CreatedDate
            return await _lessonResultRepository.Queryable
                .Include(x => x.Lesson)
                .Where(x => x.StudentId == courseResult.StudentId &&
                            x.CourseId == courseResult.CourseId &&
                            x.Status != EnumResultStatus.Unfinished)
                .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);
        }

        private async Task<LessonHomeModel> BuildLessonOverviewAsync(
            LessonResult? lessonResult,
            Course course,
            CourseResult courseResult,
            CancellationToken ct)
        {
            var orderedMockTests = (course.CourseUnitMockTests ?? Enumerable.Empty<CourseUnitMockTest>())
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.CreatedDate)
                .ToList();

            var firstUnitMockTest = orderedMockTests.FirstOrDefault();
            var firstUnitId = firstUnitMockTest?.UnitId;

            Guid? unitId = lessonResult?.UnitId ?? firstUnitId;

            CourseUnitMockTest? currentMockTest = firstUnitMockTest;

            // Nếu unit hiện tại đã Done => lấy unit tiếp theo
            if (lessonResult != null && await _unitResultRepository.IsDoneAsync(lessonResult))
            {
                currentMockTest = await GetNextUnitMockTestIfNeededAsync(orderedMockTests, unitId, courseResult.StudentId, ct);
                unitId = currentMockTest?.UnitId ?? unitId;
            }

            var overview = new LessonHomeModel
            {
                UnitId = unitId,
                IsUnitFirst = unitId == firstUnitId
            };

            // Nếu CourseResult đã Process xong => show theo Course
            if (courseResult.Status != EnumResultStatus.Process)
            {
                overview.ObjectId = course.Id;
                overview.Type = nameof(Course);
                overview.Status = MapOverviewStatus(courseResult.Status);
                return overview;
            }

            overview.ObjectId = GetObjectId(currentMockTest);
            overview.Type = GetObjectType(currentMockTest);
            overview.Status = MapOverviewStatus(await GetResultStatusAsync(currentMockTest, courseResult.StudentId, ct));

            // Hoàn thiện thông tin Lesson/Video theo Unit
            return await EnrichOverviewWithUnitAsync(overview, lessonResult, courseResult, ct);
        }

        private async Task<CourseUnitMockTest?> GetNextUnitMockTestIfNeededAsync(
            IList<CourseUnitMockTest> orderedMockTests,
            Guid? currentUnitId,
            Guid? studentId,
            CancellationToken ct)
        {
            if (currentUnitId == null)
            {
                return null;
            }
            var current = orderedMockTests.FirstOrDefault(x => x.UnitId == currentUnitId);
            if (current == null)
            {
                return null;
            }

            var next = GetNextUnitWithMockTest(orderedMockTests, orderedMockTests.IndexOf(current));
            if (next == null)
            {
                return null;
            }
            // Nếu next đã Done => nhảy tiếp 1 bước
            var nextStatus = await GetResultStatusAsync(next, studentId, ct);
            if (nextStatus == EnumResultStatus.Done)
            {
                next = GetNextUnitWithMockTest(orderedMockTests, orderedMockTests.IndexOf(next));
            }

            return next;
        }

        private async Task<LessonHomeModel> EnrichOverviewWithUnitAsync(
            LessonHomeModel overview,
            LessonResult? lessonResult,
            CourseResult courseResult,
            CancellationToken ct)
        {
            var unit = await _unitRepository.GetIncludeAsync(overview.UnitId, courseResult.CourseId, courseResult.StudentId);
            if (unit == null)
            {
                return overview;
            }
            var firstLessonId = unit.UnitLessons.OrderBy(x => x.DisplayOrder).Select(x => x.LessonId).FirstOrDefault();
            overview.LessonId = lessonResult?.LessonId ?? firstLessonId;

            // Nếu đang là Unit: xác định Lesson/Móc video cụ thể
            if (overview.Type == nameof(Domain.Entities.Unit))
            {
                // Case 1: đã có LessonResult trong Unit
                if (lessonResult != null && unit.LessonResults.Any() && lessonResult.UnitId == unit.Id)
                {
                    overview = await ResolveUnitDetailAsync(unit, lessonResult, overview, ct);
                }
                // Case 2: Lesson đầu tiên & Unit đầu tiên & vẫn đang Process => StartNow bài đầu
                else if (overview.LessonId == firstLessonId && overview.IsUnitFirst && courseResult.Status == EnumResultStatus.Process)
                {
                    overview.ObjectId = firstLessonId;
                    overview.Type = nameof(Lesson);
                    overview.Status = EnumLessonOverviewStatus.StartNow;
                }
                else
                {
                    // Fallback: bắt đầu Unit
                    overview.Type = nameof(Domain.Entities.Unit);
                    overview.ObjectId = lessonResult?.UnitId;
                    overview.Status = EnumLessonOverviewStatus.StartNow;
                }
            }

            return overview;
        }

        private async Task<LessonHomeModel> ResolveUnitDetailAsync(
            Domain.Entities.Unit unit,
            LessonResult lessonResult,
            LessonHomeModel overview,
            CancellationToken ct)
        {
            // Nếu tất cả lessons Done và có MockTestResult chưa Done => đẩy sang SkillMockTest
            var mockTestResult = unit.MockTestResults.FirstOrDefault();
            if (unit.LessonResults.All(x => x.Status == EnumResultStatus.Done) &&
                mockTestResult != null &&
                mockTestResult.Status != EnumResultStatus.Done)
            {
                overview.Type = nameof(EnumMockTestType.SkillMockTest);
                overview.ObjectId = mockTestResult.MockTestId;
                overview.Status = MapOverviewStatus(mockTestResult.Status);
                return overview;
            }

            // Nếu đang có VideoResult chưa xong và đang ở 1 TimeCode chưa Done => tiếp tục ở đó
            var videoResult = await _videoResultRepository.Queryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id, ct);

            if (videoResult != null && videoResult.Status != EnumResultStatus.Done && videoResult.CurrentVideoTimeCodeId.HasValue)
            {
                var currentTimeCodeId = videoResult.CurrentVideoTimeCodeId.Value;

                var queryData = await (from baseQ in _videoTimeCodeRepository.Queryable
                                       join vtr in _videoTimeCodeResultRepository.Queryable on baseQ.Id equals vtr.VideoTimeCodeId
                                       where baseQ.Id == currentTimeCodeId && vtr.VideoResultId == videoResult.Id
                                       select new
                                       {
                                           VideoTimeCode = baseQ,
                                           VideoTimeCodeResult = vtr
                                       }).AsNoTracking().FirstOrDefaultAsync(ct);

                var videoTimeCode = queryData?.VideoTimeCode;
                var videoTimeCodeResult = queryData?.VideoTimeCodeResult;

                var notStandalone = videoTimeCode != null && videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone;
                var notDone = videoTimeCodeResult == null || videoTimeCodeResult.Status != EnumResultStatus.Done;

                if (notStandalone && notDone && videoTimeCode != null)
                {
                    overview.ObjectId = videoTimeCode.Id;
                    overview.VideoId = videoTimeCode.VideoId;
                    overview.Status = MapOverviewStatus(videoTimeCodeResult?.Status ?? EnumResultStatus.New);
                    overview.Type = videoTimeCode.TimeCodeType.ToString();
                    return overview;
                }
            }

            // Mặc định: nếu Lesson hiện tại Done và còn Lesson New => sang Lesson New tiếp theo, ngược lại tiếp tục Lesson hiện tại
            var nextNewLesson = unit.LessonResults.FirstOrDefault(x => x.Status == EnumResultStatus.New);
            overview.Type = nameof(Lesson);
            if (lessonResult.Status == EnumResultStatus.Done && nextNewLesson != null)
            {
                overview.ObjectId = nextNewLesson.LessonId;
                overview.Status = MapOverviewStatus(nextNewLesson.Status);
            }
            else
            {
                overview.ObjectId = lessonResult.LessonId;
                overview.Status = MapOverviewStatus(lessonResult.Status);
            }

            return overview;
        }

        private static CourseUnitMockTest? GetNextUnitWithMockTest(IList<CourseUnitMockTest> list, int currentIndex)
            => list.Skip(currentIndex + 1).FirstOrDefault();

        private static EnumLessonOverviewStatus MapOverviewStatus(EnumResultStatus? status)
            => status switch
            {
                EnumResultStatus.New => EnumLessonOverviewStatus.StartNow,
                EnumResultStatus.Process => EnumLessonOverviewStatus.Continue,
                _ => EnumLessonOverviewStatus.Done // giữ nguyên logic cũ: null hoặc Done => Done
            };

        private static Guid? GetObjectId(CourseUnitMockTest? itm)
            => itm?.MockTestId ?? itm?.FinalTestId ?? itm?.UnitId;

        private static string? GetObjectType(CourseUnitMockTest? itm)
            => itm switch
            {
                null => null,
                _ when itm.MockTestId.HasValue => nameof(EnumMockTestType.FullMockTest),
                _ when itm.FinalTestId.HasValue => nameof(FinalTest),
                _ when itm.UnitId.HasValue => nameof(Domain.Entities.Unit),
                _ => null
            };

        private HomeNavigationTargetModel MapLessonOverview(
            Lesson lesson,
            LessonResult? lessonResult,
            LessonHomeModel overview)
        {
            var vm = _mapper.Map<HomeNavigationTargetModel>(lesson);
            vm.LessonResult = _mapper.Map<LessonResultModel>(lessonResult);
            vm.UnitId = lesson.UnitLessons.FirstOrDefault()?.UnitId ?? lessonResult?.UnitId ?? default;
            vm.ObjectId = overview.ObjectId;
            vm.Type = overview.Type;
            vm.Status = overview.Status;
            vm.VideoId = overview.VideoId;
            return vm;
        }

        private async Task<EnumResultStatus?> GetResultStatusAsync(CourseUnitMockTest? courseUnitMockTest, Guid? studentId, CancellationToken ct)
        {
            if (courseUnitMockTest == null)
            {
                return null;
            }
            var objectId = GetObjectId(courseUnitMockTest);
            if (!objectId.HasValue)
            {
                return null;
            }
            if (courseUnitMockTest.MockTestId.HasValue)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MockTestId == objectId &&
                        x.CourseId == courseUnitMockTest.CourseId &&
                        x.StudentId == studentId, ct);

                return mockTestResult?.Status ?? EnumResultStatus.New;
            }

            if (courseUnitMockTest.FinalTestId.HasValue)
            {
                var finalTestResult = await _finalTestResultRepository.Queryable
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.FinalTestId == objectId &&
                        x.CourseId == courseUnitMockTest.CourseId &&
                        x.StudentId == studentId, ct);

                return finalTestResult?.Status ?? EnumResultStatus.New;
            }

            if (courseUnitMockTest.UnitId.HasValue)
            {
                var unitResult = await _unitResultRepository.Queryable
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.UnitId == objectId &&
                        x.CourseId == courseUnitMockTest.CourseId &&
                        x.StudentId == studentId, ct);

                return unitResult?.Status ?? EnumResultStatus.New;
            }

            return null;
        }
    }
}
