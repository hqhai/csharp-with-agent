using System.Text;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ConfirmEmailCommand : ConfirmEmailCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public ConfirmEmailCommandHandler(UserManager<User> userManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumAuthErrorCode.AU04V),
                    nameof(request.Email), request.Email);
                return methodResult;
            }

            var token = Encoding.ASCII.GetString(WebEncoders.Base64UrlDecode(request.Token ?? string.Empty));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status500InternalServerError;
                methodResult.AddError(nameof(EnumAuthErrorCode.AU01ER));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
