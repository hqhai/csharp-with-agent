// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class LoginCommand : LoginCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMediator _mediator;

        public LoginCommandHandler(UserManager<User> userManager,
            SignInManager<User> signInManager,
            IMediator mediator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mediator = mediator;
        }

        public async Task<MethodResult<TokenModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();
            if (request.Username == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.UserNameAndPasswordNotEmpty), new Error(nameof(request.Username)), new Error(nameof(request.Password)));
                return methodResult;
            }

            var user = await _userManager.FindByNameAsync(request.Username) ?? await _userManager.FindByEmailAsync(request.Username) ??
                await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.Username, cancellationToken: cancellationToken);
            if (user == null)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.UserNameAndPasswordIncorrect), new Error(nameof(request.Username), request.Username), new Error(nameof(request.Password), request.Password));
                return methodResult;
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
            {
                methodResult.AddError(
                    StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.UserNameAndPasswordIncorrect), new Error(nameof(request.Username), request.Username), new Error(nameof(request.Password), request.Password));
                return methodResult;
            }
            methodResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);
            return methodResult;
        }
    }
}
