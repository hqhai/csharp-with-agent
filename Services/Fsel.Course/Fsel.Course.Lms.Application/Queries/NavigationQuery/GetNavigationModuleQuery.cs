// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.NavigationCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetNavigationModuleQuery : IRequest<MethodResult<ModuleNavigationModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetNavigationModuleQueryHandler : IRequestHandler<GetNavigationModuleQuery, MethodResult<ModuleNavigationModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetNavigationModuleQueryHandler(ICourseRepository courseRepository, IFinalTestResultRepository finalTestResultRepository, ISectionGroupRepository sectionGroupRepository, IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService, IUnitRepository unitRepository, ILessonResultRepository lessonResultRepository)
        {
            _courseRepository = courseRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _authContext = authContext;
            _userService = userService;
            _unitRepository = unitRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<ModuleNavigationModel>> Handle(GetNavigationModuleQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ModuleNavigationModel> methodResult = new MethodResult<ModuleNavigationModel>();
            ModuleNavigationModel? moduleNavigation = default;
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var course = await _courseRepository.GetAsync(request.CourseId, student.Id);
            var courseResult = course?.CourseResults.FirstOrDefault();
            if (course == null || courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var lessonResult = await GetLessonResultAsync(courseResult.Id, student.Id, cancellationToken);
            if (lessonResult == null || lessonResult.Status != EnumResultStatus.Done)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons.OrderBy(x => x.DisplayOrder)).FirstOrDefaultAsync(x => x.Id == lessonResult.UnitId, cancellationToken);
            if (unit == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var unitLessons = unit.UnitLessons.Select(x => new { x.LessonId, x.DisplayOrder }).ToList();
            var unitLesson = unitLessons.FirstOrDefault(x => x.LessonId == lessonResult.LessonId);
            if (unitLesson != null)
            {
                if (unitLesson.DisplayOrder == unitLessons.Max(x => x.DisplayOrder))
                {
                    var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
                    var courseUnitMockTest = courseUnitMockTests.FirstOrDefault(x => x.UnitId == unit.Id);

                    var skillMockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.StudentId == lessonResult.StudentId && x.UnitId == lessonResult.UnitId)
                                                                                     .FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId, cancellationToken);

                    if ((course.CourseType == EnumCourseType.Academic || (skillMockTestResult != null && skillMockTestResult.Status == EnumResultStatus.Done)) && courseUnitMockTest != null)
                    {
                        moduleNavigation = await GetModuleNavigationModelAsync(courseUnitMockTests, courseUnitMockTest, lessonResult);
                    }
                    else if (skillMockTestResult != null && skillMockTestResult.Status == EnumResultStatus.New && courseUnitMockTest != null)
                    {
                        moduleNavigation = new ModuleNavigationModel
                        {
                            CourseId = skillMockTestResult.CourseId,
                            Type = nameof(EnumMockTestType.SkillMockTest),
                            UnitId = skillMockTestResult.UnitId,
                            ObjectId = skillMockTestResult.MockTestId,
                            DisplayOrder = unitLessons.Count + 1,
                            CourseSkills = await GetCourseSkillToMockTests(courseUnitMockTest, skillMockTestResult),
                            CurrentLessonId = lessonResult.LessonId
                        };
                    }
                }
                else
                {
                    // Lesson Tiếp Theo
                    var lessonNext = unitLessons[unitLessons.IndexOf(unitLesson) + 1];
                    var lessonResultNext = await _lessonResultRepository.Queryable.Where(x => x.CourseResultId == lessonResult.CourseResultId && x.UnitResultId == lessonResult.UnitResultId)
                        .FirstOrDefaultAsync(x => x.LessonId == lessonNext.LessonId && x.StudentId == student.Id && x.Status == EnumResultStatus.New, cancellationToken);
                    if (lessonResultNext != null)
                    {
                        moduleNavigation = new ModuleNavigationModel
                        {
                            Type = nameof(Lesson),
                            CourseId = lessonResultNext.CourseId,
                            UnitId = lessonResultNext.UnitId,
                            ObjectId = lessonNext.LessonId,
                            DisplayOrder = lessonNext.DisplayOrder,
                            CurrentLessonId = lessonResult.LessonId
                        };
                    }
                }
            }
            methodResult.Result = moduleNavigation;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<LessonResult?> GetLessonResultAsync(Guid courseResultId, Guid studentId, CancellationToken cancellationToken)
        {
            var lessonResult = await _lessonResultRepository.Queryable
                                                   .Where(x => x.CourseResultId == courseResultId && x.StudentId == studentId)
                                                   .OrderByDescending(x => x.UpdatedDate)
                                                   .ThenByDescending(x => x.CreatedDate)
                                                   .FirstOrDefaultAsync(cancellationToken);

            if (lessonResult != null && lessonResult.Status == EnumResultStatus.New)
            {
                lessonResult = await _lessonResultRepository.Queryable
                                   .Where(x => x.CourseResultId == courseResultId && x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                   .OrderByDescending(x => x.CreatedDate)
                                   .FirstOrDefaultAsync(cancellationToken);
            }

            return lessonResult;
        }

        private async Task<bool> IsDoneModuleAsync(CourseUnitMockTest courseUnitMockTest, Guid studentId)
        {
            var listCourseSkill = new List<EnumCourseSkill>();
            if (courseUnitMockTest.MockTestId.HasValue)
            {
                return await _mockTestResultRepository.Queryable.Where(x => x.CourseId == courseUnitMockTest.CourseId && x.MockTestId == courseUnitMockTest.MockTestId).AnyAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId);
            }
            else if (courseUnitMockTest.FinalTestId.HasValue)
            {
                return await _finalTestResultRepository.Queryable.Where(x => x.CourseId == courseUnitMockTest.CourseId && x.FinalTestId == courseUnitMockTest.FinalTestId).AnyAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId);
            }
            return false;
        }

        private async Task<ModuleNavigationModel> GetModuleNavigationModelAsync(IList<CourseUnitMockTest> courseUnitMockTests, CourseUnitMockTest courseUnitMockTest, LessonResult lessonResult)
        {
            var courseUnitMockTestNext = courseUnitMockTests[courseUnitMockTests.IndexOf(courseUnitMockTest) + 1];
            if (courseUnitMockTestNext != null && await IsDoneModuleAsync(courseUnitMockTestNext, lessonResult.StudentId))
            {
                return await GetModuleNavigationModelAsync(courseUnitMockTests, courseUnitMockTestNext, lessonResult);
            }
            if (courseUnitMockTestNext != null)
            {
                courseUnitMockTest = courseUnitMockTestNext;
            }
            return new ModuleNavigationModel
            {
                CourseId = courseUnitMockTest.CourseId,
                ObjectId = GetIdModule(courseUnitMockTest),
                Type = GetTypeModule(courseUnitMockTest),
                DisplayOrder = courseUnitMockTest.DisplayOrder,
                CourseSkills = await GetCourseSkillToMockTests(courseUnitMockTest),
                CurrentLessonId = lessonResult.LessonId
            };
        }

        private async Task<IList<EnumCourseSkill>> GetCourseSkillToMockTests(CourseUnitMockTest courseUnitMockTest, MockTestResult? skillMockTestResult = default)
        {
            var listCourseSkill = new List<EnumCourseSkill>();
            if (courseUnitMockTest.MockTestId.HasValue)
            {
                listCourseSkill = await _sectionGroupRepository.Queryable.Where(x => x.MockTestSections.Any(x => x.MockTestId == courseUnitMockTest.MockTestId.Value)).Select(x => x.CourseSkill).ToListAsync();
            }
            else if (courseUnitMockTest.FinalTestId.HasValue)
            {
                listCourseSkill = await _sectionGroupRepository.Queryable.Where(x => x.FinalTestSections.Any(x => x.FinalTestId == courseUnitMockTest.FinalTestId.Value)).Select(x => x.CourseSkill).ToListAsync();
            }
            else if (skillMockTestResult != null)
            {
                listCourseSkill = await _sectionGroupRepository.Queryable.Where(x => x.MockTestSections.Any(x => x.MockTestId == skillMockTestResult.MockTestId)).Select(x => x.CourseSkill).ToListAsync();
            }
            return listCourseSkill;
        }

        private string? GetTypeModule(CourseUnitMockTest courseUnitMockTestNext)
        {
            return courseUnitMockTestNext.UnitId.HasValue ? nameof(Domain.Entities.Unit) :
                courseUnitMockTestNext.MockTestId.HasValue ? nameof(EnumMockTestType.FullMockTest) :
                courseUnitMockTestNext.FinalTestId.HasValue ? nameof(FinalTest) : default;
        }

        private static Guid? GetIdModule(CourseUnitMockTest courseUnitMockTestNext)
        {
            return courseUnitMockTestNext.UnitId.HasValue ? courseUnitMockTestNext.UnitId :
                courseUnitMockTestNext.MockTestId.HasValue ? courseUnitMockTestNext.MockTestId :
                courseUnitMockTestNext.FinalTestId.HasValue ? courseUnitMockTestNext.FinalTestId : default;
        }
    }
}
