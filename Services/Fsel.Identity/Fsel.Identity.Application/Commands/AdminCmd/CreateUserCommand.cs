// Copyright (c) Atlantic. All rights reserved.
using System.Globalization;
using System.Text;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Users;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    public class CreateUserCommand : CreateUserCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICSORepository _cSORepository;
        private readonly IHumanRepository _humanRepository;

        public CreateUserCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IMediator mediator,
            IUserOtpCodeRepository userOtpCodeRepository,
            AppSetting appSetting,
            ITeacherRepository teacherRepository,
            ICSORepository cSORepository,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
            _teacherRepository = teacherRepository;
            _cSORepository = cSORepository;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            var user = await _userManager.FindByEmailAsync(request.Email!);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                return methodResult;
            }
            else
            {
                var role = await _roleManager.FindByNameAsync(request?.Role.ToString() ?? string.Empty);
                var newPassword = new PasswordGeneratorHelper(8, 10).Generate();
                IdentityResult result;
                user = new();
                _mapper.Map(request, user);
                user.UserName = request?.Email;
                result = await _userManager.CreateAsync(user, newPassword);
                if (!result.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.UserFailToCreate), nameof(newPassword), newPassword);
                    return methodResult;
                }

                var human = await CreateHuman(request!, user);
                human = _humanRepository.Add(human);
                await _humanRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                if (request!.Role == EnumRoleRegisterWithAdmin.Teacher)
                {
                    var roleLives = human!.Teacher!.RoleLives;
                    if (roleLives != null && roleLives.Count > 0)
                    {
                        await _userManager.AddToRoleAsync(user, EnumRole.TeacherLive.ToString());
                    }
                }
                await _userManager.AddToRoleAsync(user, request!.Role.ToString());

                #region Send Code OTP

                var userOtpCode = await _userOtpCodeRepository.Queryable
                        .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumStatusUser.New && !x.IsDeleted, cancellationToken);

                var randomSecure = new RandomSecureHelper();
                var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
                var otp = totp.ComputeTotp();
                if (userOtpCode == null)
                {
                    userOtpCode = new UserOtpCode
                    {
                        UserId = user.Id,
                        OTPCode = otp,
                        Status = EnumStatusUser.New,
                        ExpiredTime = DateTime.Now.AddDays(_appSetting!.Otp!.StepDayWithAdmin)
                    };
                    _userOtpCodeRepository.Add(userOtpCode);
                    await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                var content = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.ConfirmOtpUrl!, otp) + "  " + "Mã OTP là : " + otp;
                var subject = StringValues.SendOtpSubject + user.FullName;
                var sendResult = new MethodResult<bool>();
                if (request != null && request.Email != null)
                {
                    sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Content = content, Subject = subject }, cancellationToken).ConfigureAwait(false);
                }

                if (!sendResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                    return methodResult;
                }

                #endregion Send Code OTP
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }

        private async Task<Human> CreateHuman(CreateUserCommand request, User user)
        {
            var human = _mapper.Map<Human>(request);
            human.UserId = user.Id;
            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                var stt = await _teacherRepository.Queryable.CountAsync();
                human.Teacher = new Teacher
                {
                    HumanId = human.Id,
                    CourseLevels = request.CourseLevels,
                    RoleLives = request.RoleLives,
                    CourseTypes = request.CourseTypes
                };
                human.Teacher.TeacherBankAccounts?.Add(new TeacherBankAccount
                {
                    BankAccountName = request.BankAccountName,
                    BankAccountNumber = request.BankAccountNumber,
                    BankName = request.BankName,
                    Status = EnumStatusBank.Approve
                });
                human.Code = $"TC_{stt:0000}";
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                var stt = await _cSORepository.Queryable.CountAsync();
                human.CSO = new CSO
                {
                    HumanId = human.Id,
                    RoleLives = request.RoleLives,
                    CourseLevels = request.CourseLevels,
                    CourseTypes = request.CourseTypes,
                    SubscriptionClasses = request.SubscriptionClasses
                };
                human.Code = $"CSO_{stt:0000}";
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.Moderator)
            {
                human.Code = "Moderator";
            }
            return human;
        }
    }
}
