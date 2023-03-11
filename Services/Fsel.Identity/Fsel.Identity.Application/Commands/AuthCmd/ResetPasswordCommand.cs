using Fsel.Common.ActionResults;
using Fsel.Core.Base;
using Fsel.Identity.Common.Models.Commands;
using Fsel.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ResetPasswordCommand : ResetPasswordCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;

        public ResetPasswordCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            IMediator mediator)
        {
            _authContext = authContext;
            _userManager = userManager;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (request.OldPassword == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                methodResult.AddErrorMessage("Old Password not null");
                return methodResult;
            }
            if (request.Password == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage("Password not null");
                return methodResult;
            }
            if (request.ConfirmPassword == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage("ConfirmPassword not null");
                return methodResult;
            }

            await ResetPassword(_authContext.CurrentUserId, request.Password);
            return methodResult;
        }

        public async Task ResetPassword(Guid userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return;
            }
            else
            {
                var hashPassword = _userManager.PasswordHasher.HashPassword(user, password);
                user.PasswordHash = hashPassword;
                await _userManager.UpdateAsync(user);
            }
        }
    }
}