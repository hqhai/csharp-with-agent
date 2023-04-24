// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.UserCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.CommandModels.Users;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateUserCommand : UpdateUserCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, MethodResult<UserModel>>
    {
        private readonly IUserService _userService;

        public UpdateUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            var user = await _userService.UpdateUserAsync(new UpdateUserCommandModel
            {
                Address = request.Address,
                Birthday = request.Birthday,
                CourseLevels = request.CourseLevels,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                Types = request.Types
            });

            if (!user.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotCreate), nameof(user), user);
                return methodResult;
            }

            if (user == null || user!.Content!.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNull), nameof(user), user);
                return methodResult;
            }

            methodResult.Result = user!.Content!.Result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
