// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartMockTestCommand : StartMockTestCommandModel, IRequest<MethodResult<MockTestModel>>
    {
    }

    public class StartMockTestCommandHandler : IRequestHandler<StartMockTestCommand, MethodResult<MockTestModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IUnitSkillMockTestRepository _unitSkillMockTestRepository;
        private readonly SectionConverter _sectionConverter;

        public StartMockTestCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , IMapper mapper
            , AuthContext authContext
            , IMockTestRepository mockTestRepository
            , IMockTestResultRepository mockTestResultRepository
            , IUnitSkillMockTestRepository unitSkillMockTestRepository
            , SectionConverter sectionConverter)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _unitSkillMockTestRepository = unitSkillMockTestRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(StartMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            var mockTest = await _mockTestRepository.Queryable
                                                .Include(x => x.MockTestSections)
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections)
                                                .ThenInclude(x => x.SectionParts)
                                                .ThenInclude(x => x.SectionQuestions)
                                                .ThenInclude(x => x.Question)
                                                .Include(x => x.MockTestSections)
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections)
                                                .ThenInclude(x => x.SectionTimeCodes)
                                                .Include(x => x.MockTestResults)
                                                .Include(x => x.MockTestSections)
                                                .ThenInclude(x => x.SectionGroup)
                                                .ThenInclude(x => x!.Sections)
                                                .ThenInclude(x => x.SectionParts)
                                                .ThenInclude(x => x.SectionQuestions)
                                                .ThenInclude(x => x.MockTestAnswers)
                                                .Where(x => x.Id == request.MockTestId)
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

            var unit = await _unitRepository.GetByIdAsync(request.UnitId);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitNotExist), nameof(request.UnitId), request.UnitId);
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var unitSkillMockTest = _unitSkillMockTestRepository.Queryable.Where(x => x!.UnitId == request.UnitId).FirstOrDefault();
            if (unitSkillMockTest == null)
            {
                unitSkillMockTest = new UnitSkillMockTest
                {
                    MockTestId = request.MockTestId,
                    UnitId = unit.Id,
                };

                _unitSkillMockTestRepository.Add(unitSkillMockTest);
                await _unitSkillMockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            var mockTestResult = _mockTestResultRepository.Queryable.Where(x => x!.MockTestId == request.MockTestId && x!.UnitId == request.UnitId && x!.CourseId == request.CourseId && x.StudentId == studentId).FirstOrDefault();

            var checkDone = mockTestResult != null && mockTestResult.Status == EnumResultStatus.Done;

            var mockTestModel = new MockTestModel()
            {
                Id = mockTest.Id,
                Name = mockTest.Name,
                CourseType = mockTest.CourseType,
                CreatedDate = mockTest.CreatedDate,
                CreatedFullName = mockTest.CreatedFullName,
                CreatedUserId = mockTest.CreatedUserId,
                IsActive = mockTest.IsActive,
                SectionGroups = mockTest!.MockTestSections.Where(x => x.SectionGroup != null)
                         .Select(x => x.SectionGroup)
                         .Select(x => _sectionConverter.GetSectionGroupModel(x, !checkDone)).ToList(),

                MockTestResult = mockTest.MockTestResults.Where(x => x.StudentId == studentId).Select(x => new MockTestResultModel
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
                    UnitId = unit.Id
                }).FirstOrDefault()
            };

            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult.Result = _mapper.Map<MockTestModel>(mockTestModel);
            return methodResult;
        }
    }
}
