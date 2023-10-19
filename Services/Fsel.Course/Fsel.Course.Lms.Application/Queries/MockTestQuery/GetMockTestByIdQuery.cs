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
    using Fsel.Shared.Enums;
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

            methodResult.Result = await GetMockTest(mockTest, studentId, request);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MockTestModel> GetMockTest(MockTest mockTest, Guid studentId, GetMockTestByIdQuery request)
        {
            var mockTestDetail = _mapper.Map<MockTestModel>(mockTest);
            var mockTestResult = await GetMockTestResult(request, studentId);
            var sectionGroups = mockTest.MockTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            mockTestDetail.TotalQuestion = _sectionConverter.GetTotalQuestion(sectionGroups);
            mockTestDetail.ExecutionTime = _sectionConverter.GetExecutionTime(sectionGroups);
            mockTestDetail.MockTestResult = mockTestResult;
            mockTestDetail.Skills = _sectionConverter.GetCourseSkill(sectionGroups);
            mockTestDetail.SectionGroups = GetSectionGroups(sectionGroups, mockTestResult.Id);
            return mockTestDetail;
        }

        private async Task<MockTestResultModel> GetMockTestResult(GetMockTestByIdQuery request, Guid studentId)
        {
            var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == request.CourseId && x.MockTestId == request.MockTestId && x.StudentId == studentId)
                .FirstOrDefaultAsync(x => !request.UnitId.HasValue || x.UnitId == request.UnitId.Value);
            if (mockTestResult == null)
            {
                mockTestResult = _mockTestResultRepository.Add(new MockTestResult { StudentId = studentId, CourseId = request.CourseId, UnitId = request.UnitId, MockTestId = request.MockTestId });
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            return _mapper.Map<MockTestResultModel>(mockTestResult);
        }

        private IList<SectionGroupModel> GetSectionGroups(IList<SectionGroup>? sectionGroups, Guid mockTestResultId)
        {
            ArgumentNullException.ThrowIfNull(sectionGroups);

            var indexProcess = GetIndexProcess(sectionGroups, mockTestResultId);
            return sectionGroups.Select(x =>
            {
                var index = sectionGroups.IndexOf(x);
                var sectionGroup = _mapper.Map<SectionGroupModel>(x);
                sectionGroup.Sections!.Clear();
                sectionGroup.Status = GetCurrentStatus(indexProcess, index);
                sectionGroup.SectionGroupResult = _mapper.Map<SectionGroupResultModel>(x.SectionGroupResults.FirstOrDefault());
                return sectionGroup;
            }).ToList();
        }

        private static EnumCurrentStatus GetCurrentStatus(int? indexProcess, int index)
        {
            var currentStatus = EnumCurrentStatus.Lock;
            if (indexProcess < index)
            {
                return currentStatus;
            }
            else if (indexProcess == index)
            {
                currentStatus = EnumCurrentStatus.Process;
            }
            else if (indexProcess > index || indexProcess == null)
            {
                currentStatus = EnumCurrentStatus.Done;
            }
            return currentStatus;
        }

        private static int? GetIndexProcess(IList<SectionGroup>? sectionGroups, Guid mockTestResultId)
        {
            ArgumentNullException.ThrowIfNull(sectionGroups);
            var timeCode = sectionGroups.Where(x => !x.SectionGroupResults.Any() || x.SectionGroupResults.Any(x => x.MockTestResultId == mockTestResultId && x.Status != EnumResultStatus.Done)).FirstOrDefault();
            if (timeCode == null)
            {
                return null;
            }
            return sectionGroups.IndexOf(timeCode);
        }
    }
}
