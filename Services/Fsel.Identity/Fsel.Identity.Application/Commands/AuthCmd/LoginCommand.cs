// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class LoginCommand : LoginCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly Microsoft.AspNetCore.Identity.SignInManager<User> _signInManager;
        private readonly IMediator _mediator;
        private readonly IPlatformRepository _platformRepository;

        public LoginCommandHandler(UserManager<User> userManager,
            Microsoft.AspNetCore.Identity.SignInManager<User> signInManager,
            IMediator mediator,
            IPlatformRepository platformRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mediator = mediator;
            _platformRepository = platformRepository;
        }

        public async Task<MethodResult<TokenModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();
            if (request.Username == null || request.Password == null)
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

            var platformCodes = await _platformRepository.Queryable.Include(x => x.UserPlatforms).Where(x => x.UserPlatforms.Select(n => n.UserId).Contains(user.Id)).Select(x => x.Code).ToListAsync(cancellationToken);
            if (platformCodes != null && platformCodes.Count > 0 && request.PlatformCode.HasValue && !platformCodes.Contains(request.PlatformCode.Value))
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.UserIsNotOnAnyPlatform), new Error(nameof(request.Username), request.Username), new Error(nameof(request.Password), request.Password));
                return methodResult;
            }

            if (!user.LockoutEnabled)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.AccountHasBeenLocked), new Error(nameof(request.Username), request.Username));
                return methodResult;
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
            {
                methodResult.AddError(
                    StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.UserNameAndPasswordIncorrect), new Error(nameof(request.Username), request.Username), new Error(nameof(request.Password), request.Password));
                return methodResult;
            }
            var generateToken = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);
            methodResult = generateToken;
            return methodResult;
        }
    }
}
