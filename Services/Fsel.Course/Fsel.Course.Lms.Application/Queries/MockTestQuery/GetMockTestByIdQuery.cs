// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
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
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestByIdQuery : IRequest<MethodResult<MockTestModel>>
    {
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid MockTestId { get; set; }
    }

    public class GetMockTestByIdQueryHandler : IRequestHandler<GetMockTestByIdQuery, MethodResult<MockTestModel>>
    {
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetMockTestByIdQueryHandler(IMockTestRepository mockTestRepository, SectionConverter sectionConverter, IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
            _mockTestResultRepository = mockTestResultRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetMockTestByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<MockTestModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? default;

            var mockTest = await _mockTestRepository.GetAsync(request.MockTestId, studentId);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }

            var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.MockTestId == request.MockTestId && x.StudentId == studentId && (!request.UnitId.HasValue || x.UnitId == request.UnitId), cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            else if (mockTestResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(mockTestResult));
                return methodResult;
            }

            methodResult.Result = GetMockTest(mockTest, mockTestResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private MockTestModel GetMockTest(MockTest mockTest, MockTestResult mockTestResult)
        {
            var mockTestDetail = _mapper.Map<MockTestModel>(mockTest);
            var sectionGroups = mockTest.MockTestSections.OrderBy(x => x.CreatedDate).Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            mockTestDetail.TotalQuestion = _sectionConverter.GetTotalQuestion(sectionGroups);
            mockTestDetail.MockTestResult = _mapper.Map<MockTestResultModel>(mockTestResult);
            mockTestDetail.CourseSkills = _sectionConverter.GetCourseSkill(sectionGroups);
            mockTestDetail.SectionGroups = _sectionConverter.GetSectionGroups(sectionGroups, mockTestDetail.MockTestResult.Id, "MockTestResultId");
            return mockTestDetail;
        }
    }
}
