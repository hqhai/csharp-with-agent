// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartMockTestCommand : IRequest<MethodResult<MockTestModel>>
    {
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid MockTestId { get; set; }
    }

    public class StartMockTestCommandHandler : IRequestHandler<StartMockTestCommand, MethodResult<MockTestModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public StartMockTestCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , AuthContext authContext
            , IMockTestRepository mockTestRepository
            , IMockTestResultRepository mockTestResultRepository
            , SectionGroupConverter sectionGroupConverter
            , IRequestSafeCachingService requestSafeCachingService)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _authContext = authContext;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<MockTestModel>> Handle(StartMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<MockTestModel>();

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            else if (course.Status == EnumCourseStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIsNewStateCantStartLesson), nameof(course.Status), course.Status);
                return methodResult;
            }

            if (request.UnitId != null)
            {
                var unit = await _unitRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
                if (unit == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                    return methodResult;
                }
            }
            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var mockTestResult = await _mockTestResultRepository.Queryable
                .FirstOrDefaultAsync(x => x.MockTestId == request.MockTestId && x.CourseId == request.CourseId && x.StudentId == studentId && (request.UnitId == null || x.UnitId == request.UnitId), cancellationToken);
            if (mockTestResult == null)
            {
                mockTestResult = new MockTestResult
                {
                    MockTestId = request.MockTestId,
                    UnitId = request.UnitId,
                    CourseId = request.CourseId,
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.Unfinished
                };
                await _requestSafeCachingService.SafeRequest<MockTestResult>(
                    key: $"Add_MockTestResult_{mockTestResult.CourseId}_{mockTestResult.StudentId}_{mockTestResult.MockTestId}_{mockTestResult.UnitId}_{mockTestResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _mockTestResultRepository.BulkMergeAsync(new List<MockTestResult> { mockTestResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.MockTestId, c.IsDeleted };
                        });
                        return mockTestResult;
                    });
            }

            var mockTest = await _mockTestRepository.Queryable
                                                .Include(x => x.UnitSkillMockTests.Where(x => !x.IsDeleted))
                                                .Include(x => x.CourseUnitMockTests.Where(x => !x.IsDeleted))
                                                .Include(x => x.MockTestSections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionParts.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionQuestions.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.Question)
                                                .Include(x => x.MockTestSections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionTimeCodes.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.MockTestAnswers.Where(x => !x.IsDeleted && x.MockTestResultId == mockTestResult.Id))
                                                .Include(x => x.MockTestSections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.MockTestAnswers.Where(x => !x.IsDeleted && x.MockTestResultId == mockTestResult.Id))
                                                .Include(x => x.MockTestResults.Where(x => !x.IsDeleted))
                                                .Include(x => x.MockTestSections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionParts.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionQuestions.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.MockTestAnswers.Where(x => !x.IsDeleted && x.MockTestResultId == mockTestResult.Id))
                                                .Where(x => x.Id == request.MockTestId)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }
            if (!(mockTest.CourseUnitMockTests.Any() || mockTest.UnitSkillMockTests.Any()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestNotInActiveState));
                return methodResult;
            }
            var checkDone = mockTestResult.Status == EnumResultStatus.Done;

            var mockTestModel = new MockTestModel()
            {
                Id = mockTest!.Id,
                Name = mockTest.Name,
                MockTestType = mockTest.MockTestType,
                CreatedDate = mockTest.CreatedDate,
                CreatedFullName = mockTest.CreatedFullName,
                CreatedUserId = mockTest.CreatedUserId,
                IsActive = mockTest.UnitSkillMockTests.Any() || mockTest.CourseUnitMockTests.Any(),
                SectionGroups = mockTest.MockTestSections.Where(x => x.SectionGroup != null)
                         .Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate)
                         .Select(x => _sectionGroupConverter.GetSectionGroupModel(x, !checkDone)).ToList(),
                MockTestResult = mockTest.MockTestResults.Where(x => x.MockTestId == request.MockTestId && x.CourseId == request.CourseId && x.StudentId == studentId && (request.UnitId == null || x.UnitId == request.UnitId))
                .Select(x => new MockTestResultModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    SkillScores = x.SkillScores,
                    Scores = x.SkillScores?.Average(x => x.Scores) ?? 0,
                    Percent = x.Percent,
                    Status = x.Status,
                    MockTestId = x.MockTestId,
                    StudentId = x.StudentId,
                    CourseId = course.Id,
                    UnitId = request.UnitId
                }).FirstOrDefault()
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = mockTestModel;
            return methodResult;
        }
    }
}
