// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSectionGroupByMockTestIdQuery : IRequest<MethodResult<MockTestModel>>
    {
        public Guid MockTestId { get; set; }
    }

    public class GetSectionGroupByMockTestIdQueryHandler : IRequestHandler<GetSectionGroupByMockTestIdQuery, MethodResult<MockTestModel>>
    {
        private readonly IMockTestRepository _mockTestRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetSectionGroupByMockTestIdQueryHandler(IMockTestRepository mockTestRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _mockTestRepository = mockTestRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetSectionGroupByMockTestIdQuery request, CancellationToken cancellationToken)
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

            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestSections)
                                .ThenInclude(x => x.SectionGroup)
                                .Where(x => x.Id == request.MockTestId)
                                .FirstOrDefaultAsync(cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }

            methodResult.Result = GetMockTest(mockTest, studentId);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private MockTestModel GetMockTest(MockTest mockTest, Guid? studentId)
        {
            var mockTestDetail = _mapper.Map<MockTestModel>(mockTest);
            var sectionGroups = mockTest.MockTestSections.Select(x => x.SectionGroup).ToList();
            mockTestDetail.SectionGroups = sectionGroups.Select(x => new SectionGroupModel
            {
                ExecutionTime = x.ExecutionTime,
                CourseSkill = x.CourseSkill,
                SectionGroupResult = _mapper.Map<SectionGroupResultModel>(x.SectionGroupResults.FirstOrDefault())
            }).ToList();
            return mockTestDetail;
        }

        private async Task<IList<SectionGroupModel>> GetSectionGroupsAsync(IList<Guid>? sectionGroupIds, Guid? studentId)
        {
            var sectionGroups =
        }
    }
}
