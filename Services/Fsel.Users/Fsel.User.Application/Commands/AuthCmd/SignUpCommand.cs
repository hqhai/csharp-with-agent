using AutoMapper;
using Azure.Core;
using Fsel.Common.ActionResults;
using Fsel.User.Application.Services;
using Fsel.User.Common.ConfigSettings;
using Fsel.User.Common.Helpers;
using Fsel.User.Common.Models.Commands;
using Fsel.User.Common.Models.Entities;
using Fsel.User.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fsel.User.Application.Commands.AuthCmd
{
    public class SignUpCommand : SignUpCommandModel, IRequest<MethodResult<AccountModel>>
    {
    }
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, MethodResult<AccountModel>>
    {
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppSetting _appSetting;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        public SignUpCommandHandler(UserManager<Account> userManager,
            RoleManager<IdentityRole> roleManager,
            AppSetting appSetting,
            IEmailService emailService,
            IMediator mediator,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _appSetting = appSetting;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<MethodResult<AccountModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            MethodResult<AccountModel> methodResult = new MethodResult<AccountModel>();

            //Check User Exist
            var userExit = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (userExit != null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage("This Email doesnot exit");
                return methodResult;
            }

            //Add the Role in the database
            var role = await _roleManager.FindByNameAsync(request.Role.ToString());
            if (role == null)
            {
                role = new IdentityRole(request.Role.ToString())
                {
                    Name = request.Role.ToString(),
                    NormalizedName = request.Role.ToString().ToUpper(),
                };
                await _roleManager.CreateAsync(role);
            }

            //Add the User in the database
            var user = new Account()
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email
            };
            var result = await _userManager.CreateAsync(user, request.Password ?? string.Empty);
            if (!result.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage("Sign up fail");
                return methodResult;
            }
            // Add Role to the user
            await _userManager.AddToRoleAsync(user, request.Role.ToString());

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.ASCII.GetBytes(token));

            var configmationLink = $"{_appSetting?.Url?.EmailConfirmUrl}?token={token}&email={user.Email}";
            var message = new SendEmailModel ( new List<string> { user.Email ?? string.Empty }, "Confirmation email by link: ", configmationLink);
            await _emailService.SendEmailAsync(message);

            methodResult.Result = _mapper.Map<AccountModel>(user);
            return methodResult;
        }
    }
}
