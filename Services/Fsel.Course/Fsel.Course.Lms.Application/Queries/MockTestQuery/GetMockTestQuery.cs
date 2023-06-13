// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestQuery : IRequest<MethodResult<MockTestModel>>
    {
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid MockTestId { get; set; }
    }

    public class StartMockTestCommandHandler : IRequestHandler<GetMockTestQuery, MethodResult<MockTestModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly SectionConverter _sectionConverter;

        public StartMockTestCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , IMapper mapper
            , AuthContext authContext
            , IMockTestRepository mockTestRepository
            , IMockTestResultRepository mockTestResultRepository
            , SectionConverter sectionConverter)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetMockTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<MockTestModel>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var mockTest = await _mockTestRepository.Queryable
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
                                                .Include(x => x.MockTestResults.Where(x => !x.IsDeleted))
                                                .Include(x => x.MockTestSections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionParts.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionQuestions.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.MockTestAnswers.Where(x => !x.IsDeleted))
                                                .Where(x => x.Id == request.MockTestId && x.MockTestResults.Any(x => x.StudentId == studentId))
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.MockTestId), request.MockTestId);
                return methodResult;
            }

            if (!mockTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestInActiveState), nameof(mockTest.IsActive), mockTest.IsActive);
                return methodResult;
            }
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist), nameof(request.CourseId), request.CourseId);
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
                    methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitNotExist), nameof(request.UnitId), request.UnitId);
                    return methodResult;
                }
            }

            var mockTestResult = await _mockTestResultRepository.Queryable
                .FirstOrDefaultAsync(x => x.MockTestId == request.MockTestId && x.CourseId == request.CourseId && x.StudentId == studentId && request.UnitId == null || x.UnitId == request.UnitId, cancellationToken);

            var checkDone = mockTestResult != null && mockTestResult.Status == EnumResultStatus.Done;

            var mockTestModel = new MockTestModel()
            {
                Id = mockTest.Id,
                Name = mockTest.Name,
                MockTestType = mockTest.MockTestType,
                CourseType = mockTest.CourseType,
                CreatedDate = mockTest.CreatedDate,
                CreatedFullName = mockTest.CreatedFullName,
                CreatedUserId = mockTest.CreatedUserId,
                IsActive = mockTest.IsActive,
                SectionGroups = mockTest.MockTestSections.Where(x => x.SectionGroup != null)
                         .Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate)
                         .Select(x => _sectionConverter.GetSectionGroupModel(x, !checkDone)).ToList(),
                MockTestResult = mockTest.MockTestResults.Select(x => new MockTestResultModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    Percent = x.Percent,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    MockTestId = x.MockTestId,
                    StudentId = x.StudentId,
                    CourseId = course.Id,
                    UnitId = request.UnitId
                }).FirstOrDefault()
            };

            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult.Result = mockTestModel;
            return methodResult;
        }
    }
}
