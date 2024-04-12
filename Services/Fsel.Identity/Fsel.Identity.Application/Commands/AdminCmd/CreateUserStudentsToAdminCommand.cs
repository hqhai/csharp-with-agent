// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateUserStudentsToAdminCommand : CreateUserStudentsToAdminCommandModel, IRequest<MethodResult<IList<UserModel>>>
    {
    }

    public class CreateUserStudentsToAdminCommandHandler : IRequestHandler<CreateUserStudentsToAdminCommand, MethodResult<IList<UserModel>>>
    {
        private readonly IMediator _mediator;

        public CreateUserStudentsToAdminCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<IList<UserModel>>> Handle(CreateUserStudentsToAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UserModel>>();
            if (request.Users == null || !request.Users.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Users));
                return methodResult;
            }
            var listUser = new List<UserModel>();
            foreach (var user in request.Users)
            {
                var userResult = await _mediator.Send(new CreateUserStudentToAdminCommand { CourseId = request.CourseId, Email = user.Email, IsTrialRegistration = user.IsTrialRegistration }, cancellationToken);
                if (!userResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(userResult.ErrorMessages);
                    return methodResult;
                }
                if (userResult.Result != null)
                {
                    listUser.Add(userResult.Result);
                }
            }
            methodResult.Result = listUser;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
