// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Core.Base;
using Fsel.Master.Identity.Domain.Entities;
using Fsel.Master.Identity.Domain.Enums.ErrorCodes;
using Fsel.Master.Identity.Domain.Models.CommandModels;
using Fsel.Master.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Master.Identity.Application.Commands.AuthCmd
{
    public class LoginCommand : MasterLoginCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<MasterUser> _userManager;
        private readonly SignInManager<MasterUser> _signInManager;
        private readonly IMediator _mediator;

        public LoginCommandHandler(
            UserManager<MasterUser> userManager,
            SignInManager<MasterUser> signInManager,
            IMediator mediator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mediator = mediator;
        }

        public async Task<MethodResult<TokenModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            request.Username = request.Username?.Trim();
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();

            if (string.IsNullOrWhiteSpace(request.Username) || request.Password == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMasterAuthErrorCode.UserNameAndPasswordNotEmpty),
                    new Error(nameof(request.Username)), new Error(nameof(request.Password)));
                return methodResult;
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || user.IsDeleted)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized,
                    nameof(EnumMasterAuthErrorCode.UserNameAndPasswordIncorrect),
                    new Error(nameof(request.Username), request.Username),
                    new Error(nameof(request.Password), request.Password));
                return methodResult;
            }

            if (user.Status.HasValue && user.Status == EnumUserStatus.Inactive)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized,
                    nameof(EnumMasterAuthErrorCode.AccountHasBeenLocked),
                    new Error(nameof(request.Username), request.Username));
                return methodResult;
            }

            var isCheckPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isCheckPassword)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized,
                    nameof(EnumMasterAuthErrorCode.UserNameAndPasswordIncorrect),
                    new Error(nameof(request.Username), request.Username),
                    new Error(nameof(request.Password), request.Password));
                return methodResult;
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized,
                    nameof(EnumMasterAuthErrorCode.UserNameAndPasswordIncorrect),
                    new Error(nameof(request.Username), request.Username),
                    new Error(nameof(request.Password), request.Password));
                return methodResult;
            }

            var generateToken = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);
            methodResult = generateToken;
            return methodResult;
        }
    }
}
