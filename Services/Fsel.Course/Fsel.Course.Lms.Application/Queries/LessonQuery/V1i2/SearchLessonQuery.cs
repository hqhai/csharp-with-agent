// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i2
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Domain.Entities;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.V1i1;
    using Domain.Models.QueryModels.Lessons.V1i2;
    using Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Services.ApplicationServices.CacheServices;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;
    using Shared.Helpers;
    using LessonModel = Domain.Models.EntityModels.V1i2.LessonModel;
    using Unit = Domain.Entities.Unit;

    public class SearchLessonQuery : SearchLessonQueryModel, IRequest<MethodResult<IList<LessonModel>>>
    {
    }

    public class SearchLessonQueryHandler : IRequestHandler<SearchLessonQuery, MethodResult<IList<LessonModel>>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly AuthContext _authContext;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly ILogger<SearchLessonQuery> _logger;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IListLessonCachingService _listLessonCachingService;

        public SearchLessonQueryHandler(AuthContext authContext,
            SectionGroupConverter sectionGroupConverter,
            IMapper mapper,
            ILessonResultRepository lessonResultRepository,
            IUserService userService,
            ICourseRepository courseRepository,
            IMockTestRepository mockTestRepository,
            IMockTestResultRepository mockTestResultRepository,
            IUnitRepository unitRepository,
            ILogger<SearchLessonQuery> logger,
            ILessonModuleRepository lessonModuleRepository,
            IListLessonCachingService listLessonCachingService)
        {
            _mapper = mapper;
            _lessonResultRepository = lessonResultRepository;
            _authContext = authContext;
            _sectionGroupConverter = sectionGroupConverter;
            _userService = userService;
            _courseRepository = courseRepository;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _unitRepository = unitRepository;
            _logger = logger;
            _lessonModuleRepository = lessonModuleRepository;
            _listLessonCachingService = listLessonCachingService;
        }

        public async Task<MethodResult<IList<LessonModel>>> Handle(SearchLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonModel>>();

            var userId = request.UserId ?? _authContext.CurrentUserId;
            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(userId);

            var student = studentsResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(u => u.StudentId == student.Id && u.CourseId == request.CourseId))
                .Include(x => x.UnitLessons)
                .Include(x => x.UnitSkillMockTests)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);

            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            var unitResult = unit.UnitResults.FirstOrDefault();

            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            if (unitResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(unitResult.Status));
                return methodResult;
            }

            string cacheKey = $"lesson:{userId}:{request.CourseId}:{request.UnitId}";

            var baseList = new List<LessonModel>();

            baseList.AddRange(await UpdateLessonResults(request, student.Id, unit, cancellationToken));

            if (unit.UnitSkillMockTests.Any())
            {
                baseList.Add(await UpdateMockTestResults(request, student.Id, unit, course, cancellationToken));
            }

            var data = await _listLessonCachingService.GetOrSetAsync(cacheKey, async (_, token) =>
            {
                var lessonIds = baseList
                    .Where(_ => true)
                    .Select(x => x.ObjectId)
                    .Distinct()
                    .ToList();

                if (!lessonIds.Any())
                {
                    return baseList;
                }

                var moduleEntities = await _lessonModuleRepository.Queryable
                    .Where(m => lessonIds.Contains(m.LessonId))
                    .AsNoTracking()
                    .OrderBy(m => m.DisplayNumber)
                    .ToListAsync(token);

                var modulesByLessonId = moduleEntities
                    .GroupBy(m => m.LessonId)
                    .ToDictionary(
                        g => g.Key,
                        g => _mapper.Map<List<LessonModuleModel>>(g.ToList())
                    );

                var lessonResultsForStudent = await _lessonResultRepository.Queryable
                    .Where(x => x.CourseId == request.CourseId
                                && x.UnitId == request.UnitId
                                && x.StudentId == student.Id
                                && lessonIds.Contains(x.LessonId))
                    .Include(x => x.VideoResult)
                    .Include(x => x.HomeWorkResults)
                    .Include(x => x.ClassForumResults)
                    .AsNoTracking()
                    .ToListAsync(token);

                var resultByLessonId = lessonResultsForStudent
                    .GroupBy(x => x.LessonId)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var lesson in baseList.Where(lesson => lessonIds.Contains(lesson.ObjectId)))
                {
                    if (modulesByLessonId.TryGetValue(lesson.ObjectId, out var mods) && mods != null && mods.Count > 0)
                    {
                        lesson.LessonModules.AddRange(mods);
                    }

                    if (!resultByLessonId.TryGetValue(lesson.ObjectId, out var lr))
                    {
                        continue;
                    }

                    bool videoDone = lr.VideoResult?.Status == EnumResultStatus.Done;
                    var classForumResult = lr.ClassForumResults.FirstOrDefault();

                    bool? any = lr.HomeWorkResults.Any(x => x.Status != EnumResultStatus.Unfinished);

                    bool homeworkUnlocked =
                        ((bool)any)
                        || (classForumResult is { Status: not null } &&
                            classForumResult.Status != EnumClassForumResultStatus.Draft);

                    foreach (var m in lesson.LessonModules)
                    {
                        if (videoDone)
                        {
                            m.IsClassForumLock = false;
                            m.IsDocumentLock = false;
                        }

                        if (homeworkUnlocked)
                        {
                            m.IsHomeWorkLock = false;
                        }
                    }
                }
                return baseList;
            }, default, null, cancellationToken);

            methodResult.Result = data;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }

        private async Task<IList<LessonModel>> UpdateLessonResults(SearchLessonQuery request, Guid? studentId, Unit unit, CancellationToken cancellationToken)
        {
            var lessonResults = await _lessonResultRepository.Queryable
                .AsNoTracking()
                .Where(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId)
                .ToListAsync(cancellationToken);

            if (lessonResults.Any())
            {
                var ordered = lessonResults.OrderBy(x => x.CreatedDate).ToList();
                return _mapper.Map<IList<LessonModel>>(ordered);
            }

            lessonResults = unit.UnitLessons.OrderBy(x => x.DisplayOrder)
                .Select((x, index) => new LessonResult
                {
                    UnitId = x.UnitId,
                    LessonId = x.LessonId,
                    CourseId = request.CourseId,
                    Status = ResolveInitialStatus(index),
                    StudentId = studentId ?? Guid.Empty
                }).ToList();

            lessonResults = await CreateLessonResultsAsync(lessonResults);

            var orderedResult = lessonResults.OrderBy(x => x.CreatedDate).ToList();
            return _mapper.Map<IList<LessonModel>>(orderedResult);
        }

        private static EnumResultStatus ResolveInitialStatus(int index)
        {
            return index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished;
        }

        private async Task<List<LessonResult>> CreateLessonResultsAsync(List<LessonResult> lessonResults)
        {
            if (!lessonResults.Any())
            {
                return lessonResults;
            }

            try
            {
                await _lessonResultRepository.BulkMergeAsync(lessonResults, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new
                    {
                        c.CourseId,
                        c.StudentId,
                        c.UnitId,
                        c.LessonId,
                        c.IsDeleted
                    };
                }).ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Duplicate during CreateLessonResults");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CreateLessonResults");
            }

            return lessonResults;
        }

        private async Task<LessonModel> UpdateMockTestResults(SearchLessonQuery request, Guid? studentId, Unit unit, Course course, CancellationToken cancellationToken)
        {
            var mockTestResult = await _mockTestResultRepository.Queryable
                .Include(x => x.MockTestScores)
                .FirstOrDefaultAsync(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId, cancellationToken);

            if (mockTestResult == null)
            {
                var unitSkillMockTest = unit.UnitSkillMockTests.FirstOrDefault();

                mockTestResult = new MockTestResult
                {
                    UnitId = unitSkillMockTest?.UnitId,
                    MockTestId = unitSkillMockTest?.MockTestId ?? Guid.Empty,
                    StudentId = studentId ?? Guid.Empty,
                    Status = EnumResultStatus.Unfinished,
                    CourseId = request.CourseId
                };

                _mockTestResultRepository.Add(mockTestResult);

                try
                {
                    await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogInformation(ex, "Log Duplicate Skill MockTestResult");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create MockTestResult");
                    throw;
                }
            }

            var mockTest = await _mockTestRepository.Queryable.Where(x => x.Id == mockTestResult.MockTestId)
                .Include(x => x.MockTestSections)
                .ThenInclude(x => x.SectionGroup)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (mockTest == null)
            {
                return new LessonModel();
            }

            var scores = mockTestResult.SkillScores?
                .Select(x => x.Scores)
                .FirstOrDefault() ?? 0;

            var lessonMockTestResult = _mapper.Map<LessonModel>(mockTestResult);

            var courseSkills = mockTest.MockTestSections
                .Select(x => x.SectionGroup!.CourseSkill)
                .ToList();

            lessonMockTestResult.CourseSkill = courseSkills.FirstOrDefault();
            lessonMockTestResult.IsTeacherGraded = await _sectionGroupConverter.IsTeacherGraded(mockTestResult, courseSkills);
            (lessonMockTestResult.IsCheckScoreColor, lessonMockTestResult.TargetBandScore) = course.CourseLevel.CheckScoreColor(scores);

            return lessonMockTestResult;
        }
    }
}
