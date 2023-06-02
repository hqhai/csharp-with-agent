// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestQuery : IRequest<MethodResult<PlacementTestModel>>
    {
        public EnumPlacementTestLevel Level { get; set; }
    }

    public class StartPlacementTestCommandHandler : IRequestHandler<GetPlacementTestQuery, MethodResult<PlacementTestModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IPlacementTestRepository _placementTestRepository;

        public StartPlacementTestCommandHandler(AuthContext authContext
            , IUserService userService
            , IPlacementTestRepository placementTestRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _placementTestRepository = placementTestRepository;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(GetPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            var placementTests = await _placementTestRepository.Queryable.Where(x => x.Level == request.Level && x.IsActive).ToListAsync(cancellationToken);
            if (placementTests == null || placementTests.Count > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist));
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            Random random = new Random();
            var placementTest = placementTests.OrderBy(x => random.Next()).FirstOrDefault();
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNull));
                return methodResult;
            }
            var placementTestModel = await _placementTestRepository.GetIncludePlacementTestById(placementTest.Id);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = placementTestModel;
            return methodResult;
        }
    }
}
