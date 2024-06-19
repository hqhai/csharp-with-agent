// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetLevelsByStudentQuery : IRequest<MethodResult<object>>
    {
        public EnumCourseType? CourseType { get; set; }
    }

    public class GetLevelsByStudentQueryHandler : IRequestHandler<GetLevelsByStudentQuery, MethodResult<object>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetLevelsByStudentQueryHandler(AuthContext authContext, IUserService userService)
        {
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<object>> Handle(GetLevelsByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            methodResult.Result = request.CourseType.GetListCourseLevels(student?.CourseLevel ?? default);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
