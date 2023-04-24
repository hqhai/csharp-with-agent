// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.UserCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.CommandModels.Users;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateUserCommand : CreateUserCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, MethodResult<UserModel>>
    {
        private readonly IUserService _userService;

        public CreateUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            var user = await _userService.CreateUserAsync(new CreateUserCommandModel
            {
                Address = request.Address,
                Birthday = request.Birthday,
                CourseLevels = request.CourseLevels,
                CSORoles = request.CSORoles,
                TeacherRoles = request.TeacherRoles,
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
            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }
    }
}
