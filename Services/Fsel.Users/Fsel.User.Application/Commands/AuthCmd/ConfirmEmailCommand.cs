using AutoMapper;
using Azure;
using Fsel.Common.ActionResults;
using Fsel.User.Application.Services;
using Fsel.User.Common.ConfigSettings;
using Fsel.User.Common.Models.Commands;
using Fsel.User.Common.Models.Entities;
using Fsel.User.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Fsel.User.Application.Commands.AuthCmd
{
    public class ConfirmEmailCommand : ConfirmEmailCommandModel, IRequest<MethodResult<AccountModel>>
    {
    }

    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, MethodResult<AccountModel>>
    {
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public ConfirmEmailCommandHandler(UserManager<Account> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<AccountModel>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            MethodResult<AccountModel> methodResult = new MethodResult<AccountModel>();

            //Check User Exist
            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage("Email does not exist");
                return methodResult;
            }

            var token = Encoding.ASCII.GetString(WebEncoders.Base64UrlDecode(request.Token ?? string.Empty));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status403Forbidden;
                methodResult.AddErrorMessage("Email verified fail");
                return methodResult;
            }

            methodResult.Result = _mapper.Map<AccountModel>(user);
            return methodResult;
        }
    }
}