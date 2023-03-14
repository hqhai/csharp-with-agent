using AutoMapper;
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
        private readonly IMapper _mapper;

        public LoginCommandHandler(UserManager<User> userManager,
            SignInManager<User> signInManager,
            IMediator mediator,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MethodResult<TokenModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();

            #region Validation

            if (request.Username == null || request.Password == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(nameof(EnumAuthErrorCode.AU04ER), new[] { nameof(request.Username), nameof(request.Password) });
                return methodResult;
            }

            var user = await _userManager.FindByNameAsync(request.Username) ?? await _userManager.FindByEmailAsync(request.Username) ?? await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.Username);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                methodResult.AddErrorMessage(nameof(EnumAuthErrorCode.AU05ER), new[] { nameof(request.Username), nameof(request.Password) });
                return methodResult;
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                methodResult.AddErrorMessage(nameof(EnumAuthErrorCode.AU05ER), new[] { nameof(request.Username), nameof(request.Password) });
                return methodResult;
            }

            #endregion Validation

            methodResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }).ConfigureAwait(false);
            return methodResult;
        }
    }
}
