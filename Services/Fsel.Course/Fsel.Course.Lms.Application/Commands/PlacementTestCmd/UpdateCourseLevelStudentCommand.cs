// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateCourseLevelStudentCommand : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class UpdateCourseLevelStudentCommandHandler : IRequestHandler<UpdateCourseLevelStudentCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public UpdateCourseLevelStudentCommandHandler(AuthContext authContext,
            IUserService userService
            )
        {
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(UpdateCourseLevelStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var isCheck = await _userService.UpdateStudentByLevelAsync(new UpdateStudentByLevelModel { Id = _authContext.CurrentUserId, Level = request.CourseLevel });
            if (!isCheck.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = isCheck!.Content!.Result;
            return methodResult;
        }
    }
}
