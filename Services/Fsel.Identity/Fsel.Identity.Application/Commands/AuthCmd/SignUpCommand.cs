using System.Text;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class SignUpCommand : SignUpCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly AppSetting _appSetting;
        private readonly ISenderService _senderService;
        private readonly IMapper _mapper;
        private readonly IHumanRepository _humanRepository;

        public SignUpCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            AppSetting appSetting,
            IHumanRepository humanRepository,
            ISenderService senderService,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _appSetting = appSetting;
            _senderService = senderService;
            _mapper = mapper;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            //Check User Exist
            var userExit = await _userManager.FindByEmailAsync(request?.Email ?? string.Empty);
            if (userExit != null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU04V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Email), request?.Email) });
                return methodResult;
            }

            //Add the Role in the database
            var role = await _roleManager.FindByNameAsync(request.Role.ToString());
            if (role == null)
            {
                role = new Role
                {
                    Name = request.Role.ToString(),
                    NormalizedName = request.Role.ToString(),
                };
                await _roleManager.CreateAsync(role);
            }

            //Add the User in the database
            var user = new User()
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, request.Password ?? string.Empty);
            if (!result.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(nameof(EnumAuthErrorCode.AU10ER));
                return methodResult;
            }
            // Add Role to the user
            await _userManager.AddToRoleAsync(user, request.Role.ToString());

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.ASCII.GetBytes(token));

            var configmationLink = $"{_appSetting?.Url?.EmailConfirmUrl}?token={token}&email={user.Email}";
            var senderCommandModel = new SendEmailCommandModel
            {
                Content = $"\"Confirmation email by link: \", {configmationLink}",
                Subject = "Xác thực tài khoản ",
                ToEmails = new List<string> { $"{request.Email}" }
            };

            await _senderService.SendEmailAsync(senderCommandModel);

            var human = _mapper.Map<Human>(request.Human);
            if (request.Role == EnumRoleRegister.Student)
            {
                human.Student = new Student
                {
                    HumanId = human.Id,
                };
            }
            else if (request.Role == EnumRoleRegister.Parent)
            {
                human.Parent = new Domain.Entities.Parent
                {
                    HumanId = human.Id,
                };
            }
            await _humanRepository.ExecuteTransactionAsync(async () =>
            {
                human = _humanRepository.Add(human);

                await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UserModel>(user);
                return methodResult;
            });

            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
