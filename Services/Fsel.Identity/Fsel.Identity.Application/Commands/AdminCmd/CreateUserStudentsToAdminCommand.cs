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
    using Microsoft.Net.Http.Headers;
    using Microsoft.Extensions.Hosting;

    public class CreateUserStudentsToAdminCommand : CreateUserStudentsToAdminCommandModel, IRequest<MethodResult<IList<UserModel>>>
    {
    }

    public class CreateUserStudentsToAdminCommandHandler : IRequestHandler<CreateUserStudentsToAdminCommand, MethodResult<IList<UserModel>>>
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _environment;

        public CreateUserStudentsToAdminCommandHandler(IMediator mediator, IHttpContextAccessor httpContextAccessor, IHostEnvironment environment)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public async Task<MethodResult<IList<UserModel>>> Handle(CreateUserStudentsToAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UserModel>>();
            //if (_environment.IsProduction())
            //{
            //    methodResult.AddError(StatusCodes.Status401Unauthorized, "Not Have Access Production");
            //    return methodResult;
            //}

            if (request.Emails == null || !request.Emails.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Emails));
                return methodResult;
            }
            var tokenAdmin = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
            var listUser = new List<UserModel>();
            foreach (var email in request.Emails)
            {
                var userResult = await _mediator.Send(new CreateUserStudentToAdminCommand { CourseId = request.CourseId, Email = email, IsTrialRegistration = request.IsTrialRegistration }, cancellationToken);
                if (!userResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(userResult.ErrorMessages);
                    return methodResult;
                }
                if (userResult.Result != null)
                {
                    listUser.Add(userResult.Result);
                }
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization] = tokenAdmin;
                }
            }
            methodResult.Result = listUser;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
