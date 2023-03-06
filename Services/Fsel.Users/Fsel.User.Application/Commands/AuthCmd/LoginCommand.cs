using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.User.Common.Models.Commands;
using Fsel.User.Common.Models.Entities;
using Fsel.User.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fsel.User.Application.Commands.AuthCmd
{
    public class LoginCommand : LoginCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<Account> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public LoginCommandHandler(UserManager<Account> userManager,
            IMediator mediator,
            IMapper mapper)
        {
            _userManager = userManager;
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
                return methodResult;
            }

            var user = await _userManager.FindByNameAsync(request.Username) ?? await _userManager.FindByEmailAsync(request.Username) ?? await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                //methodResult.AddResultFromErrorList(placementTest.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            methodResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }).ConfigureAwait(false);
            return methodResult;
        }
    }
}