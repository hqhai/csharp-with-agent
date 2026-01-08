// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery.V1i1
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.CachingModels;
    using Fsel.Course.Domain.Models.EntityModels.DashboardModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeNavigationTargetQuery : IRequest<MethodResult<HomeNavigationTargetModel>>
    {
    }

    public class GetHomeNavigationTargetQueryHandler : IRequestHandler<GetHomeNavigationTargetQuery, MethodResult<HomeNavigationTargetModel>>
    {
        #region DI & Ctor

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
        private readonly ICourseService _courseService;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly ITestRepository _testRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IDocumentResultRepository _documentResultRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IDocumentRepository _documentRepository;
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
            ICourseService courseService,
            ITestGroupResultRepository testGroupResultRepository,
            IUnitModuleRepository unitModuleRepository,
            ITestRepository testRepository,
            ILessonModuleRepository lessonModuleRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IDocumentResultRepository documentResultRepository,
            IVideoRepository videoRepository,
            IHomeWorkRepository homeWorkRepository,
            IClassForumRepository classForumRepository,
            IDocumentRepository documentRepository,
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
            _courseService = courseService;
            _testGroupResultRepository = testGroupResultRepository;
            _unitModuleRepository = unitModuleRepository;
            _testRepository = testRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _documentResultRepository = documentResultRepository;
            _videoRepository = videoRepository;
            _homeWorkRepository = homeWorkRepository;
            _classForumRepository = classForumRepository;
            _documentRepository = documentRepository;
            _authContext = authContext;
        }

        #endregion DI & Ctor

        #region Private NavigationTargetContext

        private sealed class NavigationTargetContext
        {
            public Guid CourseId { get; set; }
            public Guid? UnitId { get; set; }
            public Guid? LessonId { get; set; }
            public Guid? UnitResultId { get; set; }
            public Guid? LessonResultId { get; set; }
            public Guid? ObjectId { get; set; }
            public Guid? VideoId { get; set; }
            public EnumResultStatus? ResultStatus { get; set; }
            public string? Type { get; set; }
        }

        #endregion Private NavigationTargetContext

        #region Handle (Luồng chính)

        public async Task<MethodResult<HomeNavigationTargetModel>> Handle(GetHomeNavigationTargetQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<HomeNavigationTargetModel>();

            // 1. Lấy Student
            var studentMethod = await GetStudentModelAsync();
            if (!studentMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(studentMethod.ErrorMessages);
                return methodResult;
            }
            var student = studentMethod.Result!;

            // 2. Lấy CourseResult active
            var courseResultMethod = await GetCourseResultAsync(student, cancellationToken);
            if (!courseResultMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(courseResultMethod.ErrorMessages);
                return methodResult;
            }
            var courseResult = courseResultMethod.Result!;

            // 3. Lấy Course
            var courseMethod = await GetCourseAsync(courseResult, cancellationToken);
            if (!courseMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(courseMethod.ErrorMessages);
                return methodResult;
            }
            var course = courseMethod.Result!;

            // 4. Build NavigationTarget (điểm điều hướng cuối cùng)
            var navigationTarget = await BuildNavigationTargetAsync(course, courseResult, cancellationToken);
            if (navigationTarget == null || !navigationTarget.ObjectId.HasValue)
            {
                return methodResult;
            }

            // 5. Map sang HomeNavigationTargetModel
            var vm = await BuildHomeNavigationTargetModelAsync(navigationTarget, cancellationToken);
            methodResult.Result = vm;
            return methodResult;
        }

        #endregion Handle (Luồng chính)

        #region Map Status

        private static EnumLessonOverviewStatus MapOverviewStatus(EnumResultStatus? status)
            => status switch
            {
                EnumResultStatus.New => EnumLessonOverviewStatus.StartNow,
                EnumResultStatus.Process => EnumLessonOverviewStatus.Continue,
                _ => EnumLessonOverviewStatus.Done // null hoặc Done => Done
            };

        #endregion Map Status

        #region Level 1 - Student

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

        #endregion Level 1 - Student

        #region Level 2 - CourseResult

        private async Task<MethodResult<CourseResult>> GetCourseResultAsync(StudentModel student, CancellationToken ct)
        {
            var methodResult = new MethodResult<CourseResult>();

            var courseResult = await _courseResultRepository.ReadQueryable
                .Where(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active)
                .Where(x => x.CourseId == student.CourseId)
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

        #endregion Level 2 - CourseResult

        #region Level 3 - Course

        private async Task<MethodResult<Course>> GetCourseAsync(CourseResult courseResult, CancellationToken ct)
        {
            var methodResult = new MethodResult<Course>();

            var course = await _courseRepository.ReadQueryable
                .Include(x => x.CourseModules)
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

        #endregion Level 3 - Course

        #region Level 4-6 - Build NavigationTarget

        private async Task<NavigationTargetContext?> BuildNavigationTargetAsync(
            Course course,
            CourseResult courseResult,
            CancellationToken cancellationToken)
        {
            // Level 4 - CourseModules
            var navigationModules = await BuildNavigationModulesAsync(course, courseResult, cancellationToken);
            if (navigationModules == null || navigationModules.Count == 0)
            {
                return null;
            }

            var currentModuleNav = SelectCurrentModule(navigationModules);
            if (currentModuleNav == null)
            {
                return null;
            }

            // Level 5 - Module type
            if (currentModuleNav.ConfigType == EnumCourseConfigType.Unit)
            {
                return await BuildUnitNavigationTargetAsync(course, courseResult, currentModuleNav, cancellationToken);
            }

            // Module là Test độc lập
            return await BuildCourseTestNavigationTargetAsync(course, currentModuleNav, cancellationToken);
        }

        private async Task<NavigationTargetContext?> BuildUnitNavigationTargetAsync(
            Course course,
            CourseResult courseResult,
            CourseModuleBuildModel currentModuleNav,
            CancellationToken cancellationToken)
        {
            // Level 5 - UnitModule navigation
            var navigationUnitModules = await BuildNavigationUnitModulesAsync(courseResult, currentModuleNav, cancellationToken);
            if (navigationUnitModules == null || navigationUnitModules.Count == 0)
            {
                return null;
            }

            var selectedUnitModule = SelectUnitModule(navigationUnitModules, currentModuleNav.Status);
            if (selectedUnitModule == null)
            {
                return null;
            }

            if (selectedUnitModule.ConfigType == EnumUnitConfigType.Lesson)
            {
                return await BuildLessonNavigationTargetAsync(course, courseResult, currentModuleNav, selectedUnitModule, cancellationToken);
            }

            // Unit module là Test
            return new NavigationTargetContext
            {
                CourseId = course.Id,
                UnitId = currentModuleNav.UnitId,
                UnitResultId = currentModuleNav.ResultId,
                LessonId = null,
                LessonResultId = null,
                ObjectId = selectedUnitModule.TestId,
                VideoId = null,
                Type = EnumUnitConfigType.Test.ToString(),
                ResultStatus = selectedUnitModule.Status
            };
        }

        private async Task<NavigationTargetContext?> BuildLessonNavigationTargetAsync(
            Course course,
            CourseResult courseResult,
            CourseModuleBuildModel currentModuleNav,
            UnitModuleBuildModel selectedUnitModule,
            CancellationToken cancellationToken)
        {
            // Level 6 - LessonModule navigation
            var navigationLessonModules = await BuildNavigationLessonModulesAsync(courseResult, selectedUnitModule, cancellationToken);
            if (navigationLessonModules == null || navigationLessonModules.Count == 0)
            {
                return null;
            }

            var selectedLessonModule = SelectLessonModule(navigationLessonModules, selectedUnitModule.Status);
            if (selectedLessonModule == null)
            {
                return null;
            }

            var navigationTarget = new NavigationTargetContext
            {
                CourseId = course.Id,
                UnitId = currentModuleNav.UnitId,
                UnitResultId = currentModuleNav.ResultId,       // ResultId ở CourseModule = UnitResultId
                LessonId = selectedUnitModule.LessonId,
                LessonResultId = selectedUnitModule.ResultId,
                ResultStatus = selectedLessonModule.Status,
                Type = selectedLessonModule.ConfigType.ToString()
            };

            // ObjectId + VideoId tùy theo loại LessonModule
            switch (selectedLessonModule.ConfigType)
            {
                case EnumLessonConfigType.HomeWork:
                    navigationTarget.ObjectId = selectedLessonModule.HomeWorkId;
                    break;

                case EnumLessonConfigType.ClassForum:
                    navigationTarget.ObjectId = selectedLessonModule.ClassForumId;
                    break;

                case EnumLessonConfigType.Document:
                    navigationTarget.ObjectId = selectedLessonModule.DocumentId;
                    break;

                case EnumLessonConfigType.Video:
                    navigationTarget.ObjectId = selectedLessonModule.VideoId;
                    navigationTarget.VideoId = selectedLessonModule.VideoId;
                    break;
            }

            // Nếu là Video => apply thêm logic TimeCode
            if (selectedLessonModule.ConfigType == EnumLessonConfigType.Video)
            {
                await ApplyVideoTimeCodeIfNeededAsync(navigationTarget, selectedLessonModule, cancellationToken);
            }

            return navigationTarget;
        }

        private async Task<NavigationTargetContext?> BuildCourseTestNavigationTargetAsync(
            Course course,
            CourseModuleBuildModel currentModuleNav,
            CancellationToken cancellationToken)
        {
            var test = await _testRepository.ReadQueryable
                .FirstOrDefaultAsync(x => x.Id == currentModuleNav.TestId, cancellationToken);

            if (test == null)
            {
                return null;
            }

            return new NavigationTargetContext
            {
                CourseId = course.Id,
                UnitId = null,
                UnitResultId = null,
                LessonId = null,
                LessonResultId = null,
                ObjectId = test.Id,
                VideoId = null,
                Type = EnumCourseConfigType.Test.ToString(),
                ResultStatus = currentModuleNav.Status
            };
        }

        private async Task ApplyVideoTimeCodeIfNeededAsync(
            NavigationTargetContext navigationTarget,
            LessonModuleBuildModel selectedLessonModule,
            CancellationToken cancellationToken)
        {
            if (!selectedLessonModule.CurrentVideoTimeCodeId.HasValue ||
                selectedLessonModule.Status == EnumResultStatus.Done)
            {
                return;
            }

            var currentTimeCodeId = selectedLessonModule.CurrentVideoTimeCodeId.Value;

            var queryData = await (from baseQ in _videoTimeCodeRepository.Queryable
                                   join vtr in _videoTimeCodeResultRepository.Queryable on baseQ.Id equals vtr.VideoTimeCodeId
                                   where baseQ.Id == currentTimeCodeId && vtr.VideoResultId == selectedLessonModule.ResultId
                                   select new
                                   {
                                       VideoTimeCode = baseQ,
                                       VideoTimeCodeResult = vtr
                                   }).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

            var videoTimeCode = queryData?.VideoTimeCode;
            var videoTimeCodeResult = queryData?.VideoTimeCodeResult;

            var notStandalone = videoTimeCode != null && videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone;
            var notDone = videoTimeCodeResult == null || videoTimeCodeResult.Status != EnumResultStatus.Done;
            if (!notStandalone || !notDone || videoTimeCode == null)
            {
                return;
            }

            navigationTarget.ObjectId = videoTimeCode.Id;
            navigationTarget.VideoId = videoTimeCode.VideoId;
            navigationTarget.Type = videoTimeCode.TimeCodeType.ToString();
            navigationTarget.ResultStatus = videoTimeCodeResult?.Status ?? EnumResultStatus.New;
        }

        #endregion Level 4-6 - Build NavigationTarget

        #region Build HomeNavigationTargetModel

        private async Task<HomeNavigationTargetModel> BuildHomeNavigationTargetModelAsync(
            NavigationTargetContext navigationTarget,
            CancellationToken cancellationToken)
        {
            var vm = new HomeNavigationTargetModel();

            Lesson? lesson = null;
            LessonResult? lessonResult = null;

            if (navigationTarget.LessonId.HasValue)
            {
                lesson = await _lessonRepository.GetByIdAsync(navigationTarget.LessonId.Value);
                if (lesson != null)
                {
                    vm = _mapper.Map<HomeNavigationTargetModel>(lesson);
                }

                if (navigationTarget.LessonResultId.HasValue)
                {
                    lessonResult = await _lessonResultRepository.ReadQueryable
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == navigationTarget.LessonResultId.Value, cancellationToken);

                    if (lessonResult != null)
                    {
                        vm.LessonResult = _mapper.Map<LessonResultModel>(lessonResult);
                    }
                }

                // Nếu AutoMapper chưa set UnitId, fallback sang UnitId từ lessonResult
                if (vm.UnitId == Guid.Empty && lessonResult != null)
                {
                    vm.UnitId = lessonResult.UnitId;
                }
            }

            vm.CourseId = navigationTarget.CourseId;
            vm.UnitId = navigationTarget.UnitId ?? vm.UnitId;
            vm.UnitResultId = navigationTarget.UnitResultId;
            vm.ObjectId = navigationTarget.ObjectId;
            vm.Type = navigationTarget.Type;
            vm.VideoId = navigationTarget.VideoId;
            vm.Status = MapOverviewStatus(navigationTarget.ResultStatus);

            return vm;
        }

        #endregion Build HomeNavigationTargetModel

        #region Level 4 - CourseModule Navigation

        private async Task<List<CourseModuleBuildModel>?> BuildNavigationModulesAsync(
            Course course,
            CourseResult courseResult,
            CancellationToken cancellationToken)
        {
            var courseBuild = await _courseService.GetCourseBuildModel(course.Id);
            if (courseBuild?.CourseModules == null || courseBuild.CourseModules.Count == 0)
            {
                return null;
            }

            var courseModules = courseBuild.CourseModules
                .OrderBy(cm => cm.OpenOrder)
                .ToList();

            var moduleIds = courseModules.Select(x => x.Id).ToList();

            var testGroupResults = await _testGroupResultRepository.ReadQueryable
                .Include(x => x.TestResults)
                .Where(x => x.CourseResultId == courseResult.Id)
                .Where(x => x.CourseModuleId.HasValue && moduleIds.Contains(x.CourseModuleId.Value))
                .ToListAsync(cancellationToken);

            var unitResults = await _unitResultRepository.ReadQueryable
                .Where(x => x.CourseResultId == courseResult.Id)
                .Where(x => x.CourseModuleId.HasValue && moduleIds.Contains(x.CourseModuleId.Value))
                .ToListAsync(cancellationToken);

            var testGroupByModule = testGroupResults
                .GroupBy(x => x.CourseModuleId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var unitResultByModule = unitResults
                .GroupBy(x => x.CourseModuleId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var navigationModules = courseModules.Select(cm =>
            {
                testGroupByModule.TryGetValue(cm.Id, out var testGroupResult);
                unitResultByModule.TryGetValue(cm.Id, out var unitResult);
                var testResult = testGroupResult?.TestResults?.FirstOrDefault();

                return new CourseModuleBuildModel
                {
                    ConfigType = cm.ConfigType,
                    DisplayNumber = cm.DisplayNumber,
                    DisplayOrder = cm.DisplayOrder,
                    Id = cm.Id,
                    OpenOrder = cm.OpenOrder,
                    OriginalId = cm.OriginalId,
                    Percent = cm.Percent,
                    TestId = testResult?.TestId ?? cm.TestId,
                    UnitId = unitResult?.UnitId ?? cm.UnitId,
                    Status = testGroupResult?.Status ?? unitResult?.Status,
                    ResultId = unitResult?.Id ?? testGroupResult?.Id,
                    UnitModuleBuilds = cm.UnitModuleBuilds,
                };
            })
            .OrderBy(x => x.OpenOrder)
            .ToList();

            return navigationModules;
        }

        private CourseModuleBuildModel? SelectCurrentModule(List<CourseModuleBuildModel> navigationModules)
        {
            var currentModuleNav = navigationModules
                .Where(x => x.Status != EnumResultStatus.Done)
                .Where(x => x.Status != EnumResultStatus.Unfinished)
                .OrderBy(x => x.OpenOrder)
                .ThenBy(x => x.DisplayOrder)
                .FirstOrDefault();

            if (currentModuleNav != null)
            {
                return currentModuleNav;
            }

            currentModuleNav = navigationModules
                .Where(x => x.Status != EnumResultStatus.Unfinished)
                .OrderBy(x => x.OpenOrder)
                .ThenByDescending(x => x.DisplayOrder)
                .FirstOrDefault();

            return currentModuleNav;
        }

        #endregion Level 4 - CourseModule Navigation

        #region Level 5 - UnitModule Navigation

        private async Task<List<UnitModuleBuildModel>?> BuildNavigationUnitModulesAsync(
            CourseResult courseResult,
            CourseModuleBuildModel currentModuleNav,
            CancellationToken cancellationToken)
        {
            var unitModules = await _unitModuleRepository.ReadQueryable
                .Where(x => x.UnitId == currentModuleNav.UnitId)
                .ToListAsync(cancellationToken);

            if (unitModules == null || unitModules.Count == 0)
            {
                return null;
            }

            var unitModuleIds = unitModules.Select(x => x.Id).ToList();
            var unitOriginalIds = unitModules
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var lastVersionLessons = await _lessonRepository.ReadQueryable
                .Where(l => l.VersionStatus == EnumVersionStatus.LastVersion
                            && unitOriginalIds.Contains(l.OriginalId))
                .ToListAsync(cancellationToken);

            var lastVersionTests = await _testRepository.ReadQueryable
                .Where(t => t.VersionStatus == EnumVersionStatus.LastVersion
                            && unitOriginalIds.Contains(t.OriginalId))
                .ToListAsync(cancellationToken);

            var lessonByOriginalId = lastVersionLessons
                .GroupBy(l => l.OriginalId)
                .ToDictionary(g => g.Key, g => g.First());

            var testByOriginalId = lastVersionTests
                .GroupBy(t => t.OriginalId)
                .ToDictionary(g => g.Key, g => g.First());

            var lessonResults = await _lessonResultRepository.ReadQueryable
                .Where(x => x.StudentId == courseResult.StudentId &&
                            x.CourseId == courseResult.CourseId &&
                            x.UnitResultId == currentModuleNav.ResultId &&
                            x.CourseResultId == courseResult.Id &&
                            x.UnitModuleId.HasValue &&
                            unitModuleIds.Contains(x.UnitModuleId.Value))
                .ToListAsync(cancellationToken);

            var unitTestResults = await _testGroupResultRepository.ReadQueryable
                .Include(x => x.TestResults)
                .Where(x => x.StudentId == courseResult.StudentId &&
                            x.CourseId == courseResult.CourseId &&
                            x.UnitResultId == currentModuleNav.ResultId &&
                            x.CourseResultId == courseResult.Id &&
                            x.UnitModuleId.HasValue &&
                            unitModuleIds.Contains(x.UnitModuleId.Value))
                .ToListAsync(cancellationToken);

            var lessonResultByUnitModule = lessonResults
                .GroupBy(x => x.UnitModuleId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.CreatedDate).First()
                );

            var unitTestResultByUnitModule = unitTestResults
                .GroupBy(x => x.UnitModuleId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.CreatedDate).First()
                );

            var navigationUnitModules = unitModules
                .Select(um =>
                {
                    lessonResultByUnitModule.TryGetValue(um.Id, out var lr);
                    unitTestResultByUnitModule.TryGetValue(um.Id, out var tr);

                    lessonByOriginalId.TryGetValue(um.OriginalId, out var lessonEntity);
                    testByOriginalId.TryGetValue(um.OriginalId, out var testEntity);

                    EnumResultStatus? status = null;
                    Guid? resultId = null;
                    Guid? lessonId = null;
                    Guid? testId = null;

                    if (um.UnitConfigType == EnumUnitConfigType.Lesson)
                    {
                        status = lr?.Status;
                        resultId = lr?.Id;
                        lessonId = lr?.LessonId ?? lessonEntity?.Id;
                    }
                    else if (um.UnitConfigType == EnumUnitConfigType.Test)
                    {
                        var testResult = tr?.TestResults?.FirstOrDefault();

                        status = tr?.Status;
                        resultId = tr?.Id;
                        testId = testResult?.TestId ?? testEntity?.Id;
                    }

                    return new UnitModuleBuildModel
                    {
                        Id = um.Id,
                        OpenOrder = um.OpenOrder,
                        DisplayOrder = um.DisplayOrder,
                        LessonId = lessonId,
                        TestId = testId,
                        Status = status,
                        ResultId = resultId,
                        ConfigType = um.UnitConfigType,
                        DisplayNumber = um.DisplayNumber,
                        OriginalId = um.OriginalId,
                        Percent = um.Percent,
                    };
                })
                .OrderBy(x => x.OpenOrder)
                .ThenBy(x => x.DisplayOrder)
                .ToList();

            return navigationUnitModules;
        }

        private UnitModuleBuildModel? SelectUnitModule(
            List<UnitModuleBuildModel> navigationUnitModules,
            EnumResultStatus? unitStatus)
        {
            if (navigationUnitModules == null || navigationUnitModules.Count == 0)
            {
                return null;
            }

            UnitModuleBuildModel? selectedUnitModule = null;

            if (unitStatus == EnumResultStatus.Done)
            {
                selectedUnitModule = navigationUnitModules
                    .LastOrDefault(m => m.Status == EnumResultStatus.Done)
                    ?? navigationUnitModules.LastOrDefault();
            }
            else
            {
                selectedUnitModule = navigationUnitModules
                    .FirstOrDefault(m => m.Status != EnumResultStatus.Done &&
                                         m.Status != EnumResultStatus.Unfinished);

                if (selectedUnitModule == null)
                {
                    selectedUnitModule = navigationUnitModules
                        .LastOrDefault(m => m.Status == EnumResultStatus.Done)
                        ?? navigationUnitModules.FirstOrDefault();
                }
            }

            return selectedUnitModule;
        }

        #endregion Level 5 - UnitModule Navigation

        #region Level 6 - LessonModule Navigation

        private async Task<List<LessonModuleBuildModel>?> BuildNavigationLessonModulesAsync(
            CourseResult courseResult,
            UnitModuleBuildModel selectedUnitModule,
            CancellationToken cancellationToken)
        {
            var lessonModules = await _lessonModuleRepository.ReadQueryable
                .Where(x => x.LessonId == selectedUnitModule.LessonId)
                .ToListAsync(cancellationToken);

            if (lessonModules == null || lessonModules.Count == 0)
            {
                return null;
            }

            var lessonModuleIds = lessonModules
                .Select(lm => lm.Id)
                .ToList();

            var lessonModuleOriginalIds = lessonModules
                .Select(lm => lm.OriginalId)
                .Distinct()
                .ToList();

            // 1) Entity LastVersion theo OriginalId
            var lastVersionHomeWorks = await _homeWorkRepository.ReadQueryable
                .Where(h => h.VersionStatus == EnumVersionStatus.LastVersion
                            && lessonModuleOriginalIds.Contains(h.OriginalId))
                .ToListAsync(cancellationToken);

            var lastVersionClassForums = await _classForumRepository.ReadQueryable
                .Where(c => c.VersionStatus == EnumVersionStatus.LastVersion
                            && lessonModuleOriginalIds.Contains(c.OriginalId))
                .ToListAsync(cancellationToken);

            var lastVersionDocuments = await _documentRepository.ReadQueryable
                .Where(d => d.VersionStatus == EnumVersionStatus.LastVersion
                            && lessonModuleOriginalIds.Contains(d.OriginalId))
                .ToListAsync(cancellationToken);

            var lastVersionVideos = await _videoRepository.ReadQueryable
                .Where(v => v.VersionStatus == EnumVersionStatus.LastVersion
                            && lessonModuleOriginalIds.Contains(v.OriginalId))
                .ToListAsync(cancellationToken);

            var homeWorkByOriginalId = lastVersionHomeWorks
                .GroupBy(h => h.OriginalId)
                .ToDictionary(g => g.Key, g => g.First());

            var classForumByOriginalId = lastVersionClassForums
                .GroupBy(c => c.OriginalId)
                .ToDictionary(g => g.Key, g => g.First());

            var documentByOriginalId = lastVersionDocuments
                .GroupBy(d => d.OriginalId)
                .ToDictionary(g => g.Key, g => g.First());

            var videoByOriginalId = lastVersionVideos
                .GroupBy(v => v.OriginalId)
                .ToDictionary(g => g.Key, g => g.First());

            // 2) Result theo LessonResult + LessonModule
            var homeWorkResults = await _homeWorkResultRepository.ReadQueryable
                .Where(x => x.StudentId == courseResult.StudentId &&
                            x.LessonModuleId.HasValue &&
                            x.LessonResultId == selectedUnitModule.ResultId &&
                            lessonModuleIds.Contains(x.LessonModuleId.Value))
                .ToListAsync(cancellationToken);

            var classForumResults = await _classForumResultRepository.ReadQueryable
                .Where(x => x.StudentId == courseResult.StudentId &&
                            x.LessonModuleId.HasValue &&
                            x.LessonResultId == selectedUnitModule.ResultId &&
                            lessonModuleIds.Contains(x.LessonModuleId.Value))
                .ToListAsync(cancellationToken);

            var documentResults = await _documentResultRepository.ReadQueryable
                .Where(x => x.StudentId == courseResult.StudentId &&
                            x.LessonResultId == selectedUnitModule.ResultId &&
                            lessonModuleIds.Contains(x.LessonModuleId))
                .ToListAsync(cancellationToken);

            var videoResults = await _videoResultRepository.ReadQueryable
                .Where(x => x.StudentId == courseResult.StudentId &&
                            x.LessonModuleId.HasValue &&
                            x.LessonResultId == selectedUnitModule.ResultId &&
                            lessonModuleIds.Contains(x.LessonModuleId.Value))
                .ToListAsync(cancellationToken);

            var homeWorkResultByLessonModule = homeWorkResults
                .GroupBy(x => x.LessonModuleId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.CreatedDate).First()
                );

            var classForumResultByLessonModule = classForumResults
                .GroupBy(x => x.LessonModuleId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.CreatedDate).First()
                );

            var documentResultByLessonModule = documentResults
                .GroupBy(x => x.LessonModuleId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.CreatedDate).First()
                );

            var videoResultByLessonModule = videoResults
                .GroupBy(x => x.LessonModuleId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.CreatedDate).First()
                );

            var navigationLessonModules = lessonModules
                .Select(lm =>
                {
                    homeWorkResultByLessonModule.TryGetValue(lm.Id, out var hwResult);
                    classForumResultByLessonModule.TryGetValue(lm.Id, out var cfResult);
                    documentResultByLessonModule.TryGetValue(lm.Id, out var docResult);
                    videoResultByLessonModule.TryGetValue(lm.Id, out var vResult);

                    homeWorkByOriginalId.TryGetValue(lm.OriginalId, out var homeWorkEntity);
                    classForumByOriginalId.TryGetValue(lm.OriginalId, out var classForumEntity);
                    documentByOriginalId.TryGetValue(lm.OriginalId, out var documentEntity);
                    videoByOriginalId.TryGetValue(lm.OriginalId, out var videoEntity);

                    EnumResultStatus? status = null;
                    Guid? resultId = null;

                    Guid? classForumId = null;
                    Guid? videoId = null;
                    Guid? homeWorkId = null;
                    Guid? documentId = null;
                    Guid? currentVideoTimeCodeId = null;

                    switch (lm.LessonConfigType)
                    {
                        case EnumLessonConfigType.HomeWork:
                            status = hwResult?.Status;
                            resultId = hwResult?.Id;
                            homeWorkId = hwResult?.HomeWorkId ?? homeWorkEntity?.Id;
                            break;

                        case EnumLessonConfigType.ClassForum:
                            status = cfResult?.ResultStatus;
                            resultId = cfResult?.Id;
                            classForumId = cfResult?.ClassForumId ?? classForumEntity?.Id;
                            break;

                        case EnumLessonConfigType.Document:
                            status = docResult?.Status;
                            resultId = docResult?.Id;
                            documentId = docResult?.DocumentId ?? documentEntity?.Id;
                            break;

                        case EnumLessonConfigType.Video:
                            status = vResult?.Status;
                            resultId = vResult?.Id;
                            videoId = vResult?.VideoId ?? videoEntity?.Id;
                            currentVideoTimeCodeId = vResult?.CurrentVideoTimeCodeId;
                            break;
                    }

                    return new LessonModuleBuildModel
                    {
                        Id = lm.Id,
                        OpenOrder = lm.OpenOrder,
                        DisplayOrder = lm.DisplayOrder,
                        DisplayNumber = lm.DisplayNumber,
                        Percent = lm.Percent,
                        ConfigType = lm.LessonConfigType,
                        OriginalId = lm.OriginalId,
                        Status = status,
                        CurrentVideoTimeCodeId = currentVideoTimeCodeId,
                        ResultId = resultId,
                        ClassForumId = classForumId,
                        VideoId = videoId,
                        HomeWorkId = homeWorkId,
                        DocumentId = documentId
                    };
                })
                .OrderBy(x => x.OpenOrder)
                .ThenBy(x => x.DisplayOrder)
                .ToList();

            return navigationLessonModules;
        }

        private LessonModuleBuildModel? SelectLessonModule(
            List<LessonModuleBuildModel> navigationLessonModules,
            EnumResultStatus? lessonStatus)
        {
            if (navigationLessonModules == null || navigationLessonModules.Count == 0)
            {
                return null;
            }

            LessonModuleBuildModel? selectedLessonModule = null;

            if (lessonStatus == EnumResultStatus.Done)
            {
                selectedLessonModule = navigationLessonModules
                    .LastOrDefault(m => m.Status == EnumResultStatus.Done)
                    ?? navigationLessonModules.LastOrDefault();
            }
            else
            {
                selectedLessonModule = navigationLessonModules
                    .FirstOrDefault(m => m.Status != EnumResultStatus.Done &&
                                         m.Status != EnumResultStatus.Unfinished);

                if (selectedLessonModule == null)
                {
                    selectedLessonModule = navigationLessonModules
                        .LastOrDefault(m => m.Status == EnumResultStatus.Done)
                        ?? navigationLessonModules.FirstOrDefault();
                }
            }

            return selectedLessonModule;
        }

        #endregion Level 6 - LessonModule Navigation
    }
}
