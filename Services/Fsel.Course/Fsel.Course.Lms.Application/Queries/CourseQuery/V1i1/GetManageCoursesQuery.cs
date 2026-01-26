// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Unit = Domain.Entities.Unit;

    public class GetManageCoursesQuery : IRequest<MethodResult<IList<CourseManagerModel>>>
    {
        public EnumWorkingStatus WorkingStatus { get; set; }
    }

    public class GetManageCoursesQueryHandler : IRequestHandler<GetManageCoursesQuery, MethodResult<IList<CourseManagerModel>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ChangeCourseHelper _changeCourseHelper;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private const int MaxPercentOverall = 67;

        public GetManageCoursesQueryHandler(ICourseResultRepository courseResultRepository, ManagerProgressHelper managerProgressHelper, ChangeCourseHelper changeCourseHelper, ICourseUnitMockTestRepository courseUnitMockTestRepository, ISectionGroupRepository sectionGroupRepository, IUnitRepository unitRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, IMockTestResultRepository mockTestResultRepository, IFinalTestResultRepository finalTestResultRepository, ICourseRepository courseRepository, AuthContext authContext, IMapper mapper, IUserService userService, ISystemService systemService)
        {
            _courseResultRepository = courseResultRepository;
            _managerProgressHelper = managerProgressHelper;
            _changeCourseHelper = changeCourseHelper;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _courseRepository = courseRepository;
            _authContext = authContext;
            _mapper = mapper;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<CourseManagerModel>>> Handle(GetManageCoursesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseManagerModel>>();
            var courseManagers = new List<CourseManagerModel>();
            if (request.WorkingStatus == EnumWorkingStatus.NotWorking)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.WorkingStatus));
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var isCourseStudent = await _courseResultRepository.Queryable.AnyAsync(x => x.StudentId == student.Id, cancellationToken);
            if (!isCourseStudent)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var userCourseSettingResults = await _userService.GetUserCourseSettingsAsync(_authContext.CurrentUserId);
            if (!userCourseSettingResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(userCourseSettingResults));
                return methodResult;
            }

            var userCourseSettings = userCourseSettingResults?.Content?.Result;
            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => x.StudentId == student.Id && x.WorkingStatus == request.WorkingStatus).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.UpdatedDate).ToListAsync(cancellationToken);
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = courseResults.Select(x => new FeatureAccessTimeQueryModel
                {
                    CourseId = x.CourseId,
                    UserId = _authContext.CurrentUserId,
                }).ToList(),
                UserId = _authContext.CurrentUserId
            });

            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }

            var featureAccessTimes = featureAccessTimeResults.Content?.Result?.ToList();
            var isChangeLevelAllCourse = await _changeCourseHelper.CheckChangeLevelAllCourseAsync(student.Id);

            foreach (var courseResult in courseResults)
            {
                var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.CourseId == courseResult.CourseId);
                var courseResultModel = new CourseResultModel
                {
                    CourseType = courseResult.Course?.CourseType,
                    CourseId = courseResult.CourseId,
                    StudentId = courseResult.StudentId
                };
                var (currentProgress, progress) = await _managerProgressHelper.GetCompleteCourseAsync(courseResultModel);
                var courseManager = new CourseManagerModel
                {
                    CourseResultId = courseResult.Id,
                    Status = courseResult.Status,
                    CourseId = courseResult.CourseId,
                    CodeCourse = courseResult.Course?.Code,
                    Percent = courseResult.Percent,
                    ProcessDate = courseResult.ProcessDate,
                    CompletionDate = courseResult.CompletionDate,
                    CourseLevel = courseResult.Course?.CourseLevel,
                    CourseType = courseResult.Course?.CourseType,
                    WorkingStatus = courseResult.WorkingStatus,
                    ProgressPercent = NumberHelper.GetPercent(currentProgress, progress),
                    TimeSpent = featureAccessTime?.AccessTime ?? default
                };
                if (courseResult.Status == EnumResultStatus.Done)
                {
                    courseManager.Type = nameof(Course);
                    courseManager.ObjectId = courseResult.CourseId;
                }
                else
                {
                    await SetProgressModuleAsync(courseManager, courseResult);
                }
                await SetHideCourseOnCourse(courseManager, courseResult, student, isChangeLevelAllCourse);
                if (courseManager.IsHiddenCourseLevel)
                {
                    (courseManager.IsChangeLevel, courseManager.IsResetCourse) = (false, false);
                }
                else
                {
                    courseManager.IsChangeLevel = userCourseSettings.HasRemainingAttempts(EnumUserCourseType.ChangeLevel);
                    courseManager.IsResetCourse = userCourseSettings.HasRemainingAttempts(EnumUserCourseType.ResetAndLearnAgain, courseResult.Course?.CourseLevel);
                }
                if (courseManager.CourseType == EnumCourseType.Ielts && courseManager.CourseLevel.HasValue)
                {
                    var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId && !x.UnitId.HasValue)
                                                                                  .OrderByDescending(x => x.CreatedDate)
                                                                                  .ThenByDescending(x => x.UpdatedDate)
                                                                                  .FirstOrDefaultAsync(cancellationToken);
                    var mockTestResultModel = _mapper.Map<MockTestResultModel>(mockTestResult);
                    courseManager.BandScores = mockTestResultModel?.Scores ?? default;
                    courseManager.TargetBandScores = courseManager.CourseLevel.Value.GetBandScore();
                }
                courseManager.IsCheckPercentColor = await IsColorToPercentAsync(courseResult);
                courseManagers.Add(courseManager);
            }
            methodResult.Result = courseManagers;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetHideCourseOnCourse(CourseManagerModel courseManager, CourseResult courseResult, StudentModel student, bool isChangeLevelAllCourse)
        {
            var isCourseDone = await _courseResultRepository.Queryable.AnyAsync(x => x.StudentId == student.Id && x.Status == EnumResultStatus.Done);
            var isStudentsAchieveScore = await _changeCourseHelper.IsStudentsAchieveScoresAsync(student.Id, student.BaseCourseLevel);
            courseManager.IsHiddenCourseLevel = !student.BaseCourseLevel.CheckLevelByPass(courseResult.Course?.CourseLevel ?? default, isStudentsAchieveScore);
            if (isChangeLevelAllCourse)
            {
                courseManager.IsHiddenCourseLevel = isChangeLevelAllCourse;
            }
        }

        private async Task<bool?> IsColorToPercentAsync(CourseResult courseResult)
        {
            if (courseResult.Status != EnumResultStatus.Done)
            {
                return default;
            }
            var isCheckPercentColor = courseResult.Percent > MaxPercentOverall;
            if (courseResult.Course != null && courseResult.Course.CourseType == EnumCourseType.Ielts)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId && !x.UnitId.HasValue)
                                               .Where(x => x.Status == EnumResultStatus.Done)
                                               .OrderByDescending(x => x.CreatedDate)
                                               .ThenByDescending(x => x.UpdatedDate)
                                               .FirstOrDefaultAsync();
                var score = _changeCourseHelper.GetBandScore(mockTestResult?.SkillScores);
                var (isScorePassed, targetBandScore) = courseResult.Course.CourseLevel.CheckScoreColor(score);
                isCheckPercentColor = isCheckPercentColor && isScorePassed;
            }

            return isCheckPercentColor;
        }

        private async Task<EnumCourseSkill?> GetCourseSkillMockTest(Guid mockTestId)
        {
            var sectionGroup = await _sectionGroupRepository.Queryable.Where(x => x.MockTestSections.Any(x => x.MockTestId == mockTestId)).FirstOrDefaultAsync();
            return sectionGroup?.CourseSkill;
        }

        private async Task SetProgressModuleAsync(CourseManagerModel courseManager, CourseResult courseResult)
        {
            var unitResult = await _unitResultRepository.Queryable.Where(x => x.CourseResultId == courseResult.Id && x.CourseId == courseResult.CourseId && x.Status != EnumResultStatus.Unfinished)
                                                        .OrderByDescending(x => x.CreatedDate)
                                                        .ThenByDescending(x => x.UpdatedDate)
                                                        .FirstOrDefaultAsync();
            if (unitResult == null)
            {
                return;
            }

            if (unitResult.Status != EnumResultStatus.Done)
            {
                var unit = await _unitRepository.Queryable.Include(x => x.CourseUnitMockTests.Where(x => x.CourseId == courseResult.CourseId))
                                                     .Include(x => x.UnitLessons)
                                                     .Include(x => x.UnitSkillMockTests)
                                                     .FirstOrDefaultAsync(x => x.Id == unitResult.UnitId);
                if (unit == null)
                {
                    return;
                }
                courseManager.DisplayOrder = unit.CourseUnitMockTests.Max(x => x.DisplayOrder);
                courseManager.Type = nameof(Unit);
                courseManager.ObjectId = unitResult.UnitId;

                var lessonResults = await _lessonResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.UnitResultId == unitResult.Id).OrderBy(x => x.CreatedDate).ToListAsync();
                if (lessonResults != null && lessonResults.Any())
                {
                    courseManager.LessonDisplayOrder = lessonResults.Count(x => x.Status == EnumResultStatus.Done) + 1;
                    if (lessonResults.All(x => x.Status == EnumResultStatus.Done) && unit.UnitSkillMockTests.Any())
                    {
                        var skillMockTestId = unit.UnitSkillMockTests.Select(x => x.MockTestId).FirstOrDefault();
                        courseManager.LessonType = nameof(EnumMockTestType.SkillMockTest);
                        courseManager.ObjectLessonId = skillMockTestId;
                        courseManager.CourseSkill = await GetCourseSkillMockTest(skillMockTestId);
                    }
                    else
                    {
                        courseManager.LessonType = nameof(Lesson);
                        courseManager.ObjectLessonId = lessonResults.FirstOrDefault(x => x.Status != EnumResultStatus.Done && x.Status != EnumResultStatus.Unfinished)?.LessonId;
                    }
                }
            }
            else if (courseManager.CourseType == EnumCourseType.Academic || courseManager.CourseType == EnumCourseType.EnglishFoundation)
            {
                var finalTestResult = await _finalTestResultRepository.Queryable.Include(x => x.FinalTest).ThenInclude(x => x.CourseUnitMockTests.Where(x => x.CourseId == courseResult.CourseId))
                    .Where(x => x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId && x.Status != EnumResultStatus.Unfinished)
                    .FirstOrDefaultAsync();
                if (finalTestResult == null)
                {
                    return;
                }
                courseManager.DisplayOrder = finalTestResult.FinalTest?.CourseUnitMockTests.Max(x => x.DisplayOrder) ?? default;
                courseManager.Type = nameof(FinalTest);
                courseManager.ObjectId = finalTestResult.FinalTest?.Id ?? default;
            }
            else if (courseManager.CourseType == EnumCourseType.Ielts)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTest).ThenInclude(x => x.CourseUnitMockTests.Where(x => x.CourseId == courseResult.CourseId))
                                                        .Where(x => !x.UnitId.HasValue && x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId && x.Status != EnumResultStatus.Unfinished)
                                                        .OrderByDescending(x => x.CreatedDate)
                                                        .ThenByDescending(x => x.UpdatedDate)
                                                        .FirstOrDefaultAsync();
                if (mockTestResult == null)
                {
                    return;
                }
                courseManager.DisplayOrder = mockTestResult.MockTest?.CourseUnitMockTests.Max(x => x.Number) ?? default;
                courseManager.Type = nameof(EnumMockTestType.FullMockTest);
                courseManager.ObjectId = mockTestResult.MockTest?.Id ?? default;
            }
        }
    }
}
