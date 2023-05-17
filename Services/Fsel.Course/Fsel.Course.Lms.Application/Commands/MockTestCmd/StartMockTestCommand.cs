// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartMockTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid MockTestId { get; set; }
        public Guid SectionGroupId { get; set; }
    }

    public class StartMockTestCommandHandler : IRequestHandler<StartMockTestCommand, MethodResult<bool>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public StartMockTestCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , IMapper mapper
            , AuthContext authContext
            , IMockTestRepository mockTestRepository
            , IMockTestResultRepository mockTestResultRepository)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(StartMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestSections)
                                                    .FirstOrDefaultAsync(x => x.Id == request.MockTestId && x.MockTestSections.Select(x => x.SectionGroupId).Contains(request.SectionGroupId), cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.MockTestId), request.MockTestId);
                return methodResult;
            }

            if (mockTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestInActiveState), nameof(mockTest.IsActive), mockTest.IsActive);
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }

            var studentId = student?.Content?.Result?.Id;
            var mockTestSectionId = mockTest.MockTestSections.FirstOrDefault()!.Id;
            var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestId == mockTestSectionId && x.StudentId == studentId, cancellationToken);
            if (mockTestResult == null)
            {
                _mockTestResultRepository.Add(new MockTestResult { StudentId = studentId ?? default, MockTestId = mockTestSectionId });
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
