using AutoMapper;
using Azure;
using Fsel.Common.ActionResults;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Common.ConfigSettings;
using Fsel.Identity.Common.Models.Commands;
using Fsel.Identity.Common.Models.Entities;
using Fsel.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ConfirmEmailCommand : ConfirmEmailCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public ConfirmEmailCommandHandler(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

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

            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}