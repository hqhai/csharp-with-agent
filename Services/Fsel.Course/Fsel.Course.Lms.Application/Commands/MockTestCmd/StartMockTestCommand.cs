// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly SectionConverter _sectionConverter;

        public StartMockTestCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , AuthContext authContext
            , IMockTestRepository mockTestRepository
            , IMockTestResultRepository mockTestResultRepository
            , SectionConverter sectionConverter)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _authContext = authContext;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(StartMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<MockTestModel>();

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
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
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

                _mockTestResultRepository.Add(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                mockTestResult.Status = EnumResultStatus.Process;
                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
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
                                                .ThenInclude(x => x.MockTestAnswers)
                                                .Include(x => x.MockTestSections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                .ThenInclude(x => x.MockTestAnswers)
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
                CourseType = mockTest.CourseType,
                CreatedDate = mockTest.CreatedDate,
                CreatedFullName = mockTest.CreatedFullName,
                CreatedUserId = mockTest.CreatedUserId,
                IsActive = mockTest.UnitSkillMockTests.Any() || mockTest.CourseUnitMockTests.Any(),
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

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = mockTestModel;
            return methodResult;
        }
    }
}
