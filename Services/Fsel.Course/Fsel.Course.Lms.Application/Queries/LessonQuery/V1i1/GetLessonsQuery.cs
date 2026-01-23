// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i1
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
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class GetLessonsQuery : IRequest<MethodResult<IList<LessonMockTestResultModel>>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid? UserId { get; set; }
    }

    public class GetLessonsQueryHandler : IRequestHandler<GetLessonsQuery, MethodResult<IList<LessonMockTestResultModel>>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ILogger<GetLessonsQuery> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly AuthContext _authContext;
        private readonly SectionGroupConverter _sectionGroupConverter;

        public GetLessonsQueryHandler(AuthContext authContext,
            SectionGroupConverter sectionGroupConverter,
            IMapper mapper,
            ILessonResultRepository lessonResultRepository,
            IUserService userService,
            ILogger<GetLessonsQuery> logger,
            ICourseRepository courseRepository,
            IMockTestRepository mockTestRepository,
            IMockTestResultRepository mockTestResultRepository,
            IUnitRepository unitRepository)
        {
            _mapper = mapper;
            _lessonResultRepository = lessonResultRepository;
            _authContext = authContext;
            _sectionGroupConverter = sectionGroupConverter;
            _userService = userService;
            _logger = logger;
            _courseRepository = courseRepository;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<IList<LessonMockTestResultModel>>> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonMockTestResultModel>>();
            var userId = request.UserId ?? _authContext.CurrentUserId;
            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(userId);
            if (studentsResult == null)
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
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == student.Id && x.CourseId == request.CourseId))
                                 .Include(x => x.UnitLessons)
                                 .Include(x => x.UnitSkillMockTests)
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
            else if (unitResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(unitResult.Status));
                return methodResult;
            }
            var data = new List<LessonMockTestResultModel>();
            data.AddRange(await UpdateLessonResults(unitResult, student.Id, unit, cancellationToken));
            if (unit.UnitSkillMockTests.Any())
            {
                data.Add(await UpdateMockTestResults(request, student.Id, unit, course, cancellationToken));
            }
            methodResult.Result = data;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<LessonMockTestResultModel>> UpdateLessonResults(UnitResult unitResult, Guid? studentId, Domain.Entities.Unit unit, CancellationToken cancellationToken)
        {
            var lessonResults = await _lessonResultRepository.Queryable.Where(x => x.UnitResultId == unitResult.Id && x.StudentId == studentId).ToListAsync(cancellationToken);
            if (!lessonResults.Any())
            {
                lessonResults = unit.UnitLessons.OrderBy(x => x.DisplayOrder).Select((x, index) => new LessonResult
                {
                    UnitId = x.UnitId,
                    LessonId = x.LessonId,
                    CourseId = unitResult.CourseId,
                    Status = index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished,
                    StudentId = studentId ?? default
                }).ToList();
                lessonResults = await CreateLessonResultsAsync(lessonResults, cancellationToken);
            }
            return _mapper.Map<IList<LessonMockTestResultModel>>(lessonResults?.OrderBy(x => x.CreatedDate).ToList());
        }

        private async Task<List<LessonResult>> CreateLessonResultsAsync(List<LessonResult> lessonResults, CancellationToken cancellationToken)
        {
            if (!lessonResults.Any())
            {
                return lessonResults;
            }
            foreach (var lessonResult in lessonResults)
            {
                await CreateLessonResultAsync(lessonResult, cancellationToken);
            }
            return lessonResults;
        }

        private async Task CreateLessonResultAsync(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            try
            {
                await _lessonResultRepository.BulkMergeAsync(new List<LessonResult> { lessonResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.LessonId, c.IsDeleted };
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate LessonResult : {ex.Message}");
            }
        }

        private async Task<LessonMockTestResultModel> UpdateMockTestResults(GetLessonsQuery request, Guid? studentId, Domain.Entities.Unit unit, Course course, CancellationToken cancellationToken)
        {
            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).FirstOrDefaultAsync(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId, cancellationToken);
            if (mockTestResult == null)
            {
                var unitSkillMockTest = unit.UnitSkillMockTests.FirstOrDefault();
                mockTestResult = new MockTestResult
                {
                    UnitId = unitSkillMockTest?.UnitId,
                    MockTestId = unitSkillMockTest?.MockTestId ?? default,
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.Unfinished,
                    CourseId = request.CourseId
                };

                _mockTestResultRepository.Add(mockTestResult);
                try
                {
                    await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Log Duplicate Skill MockTestResult : {ex.Message}");
                }
            }
            var mockTest = await _mockTestRepository.Queryable.Where(x => x.Id == mockTestResult.MockTestId)
                                        .Include(x => x!.MockTestSections)
                                        .ThenInclude(x => x.SectionGroup)
                                        .FirstOrDefaultAsync(cancellationToken);
            if (mockTest == null)
            {
                return new LessonMockTestResultModel();
            }

            var scores = mockTestResult.SkillScores?.Select(x => x.Scores).FirstOrDefault() ?? default;
            var lessonMockTestResult = _mapper.Map<LessonMockTestResultModel>(mockTestResult);
            var courseSkills = mockTest.MockTestSections.Select(x => x.SectionGroup!.CourseSkill).ToList();
            lessonMockTestResult.CourseSkill = courseSkills.FirstOrDefault();
            lessonMockTestResult.IsTeacherGraded = await _sectionGroupConverter.IsTeacherGraded(mockTestResult, courseSkills);
            (lessonMockTestResult.IsCheckScoreColor, lessonMockTestResult.TargetBandScore) = course.CourseLevel.CheckScoreColor(scores);
            return lessonMockTestResult;
        }
    }
}
