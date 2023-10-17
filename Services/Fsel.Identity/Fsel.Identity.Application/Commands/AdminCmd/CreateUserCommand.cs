// Copyright (c) Atlantic. All rights reserved.
using System.Globalization;
using System.Text;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Application.Services.OrderService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Users;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.SenderTemplates;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using EnumAuthUserErrorCode = Fsel.Identity.Domain.Enums.ErrorCodes.EnumAuthUserErrorCode;

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
        private readonly IOrderService _orderService;
        private readonly IMediator _mediator;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICSORepository _cSORepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly IHumanRepository _humanRepository;

        public CreateUserCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IOrderService orderService,
            IMediator mediator,
            IUserOtpCodeRepository userOtpCodeRepository,
            AppSetting appSetting,
            ITeacherRepository teacherRepository,
            ICSORepository cSORepository,
            IPlatformRepository platformRepository,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _orderService = orderService;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
            _teacherRepository = teacherRepository;
            _cSORepository = cSORepository;
            _platformRepository = platformRepository;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            #region validate

            if (!string.IsNullOrEmpty(request.Email) && !request.Email.IsValidEmail())
            {
                methodResult.AddError(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddError(nameof(EnumAuthUserErrorCode.PhoneNumberIsNotValid), nameof(request.PhoneNumber));
                return methodResult;
            }
            if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                if (request.PackageIds == null || request.PackageIds.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.PackageIds));
                    return methodResult;
                }

                var packageResults = await _orderService.GetPackages();
                var packages = packageResults?.Content?.Result;
                if (packages != null)
                {
                    var isCheck = request.PackageIds.All(x => packages.Select(y => y.Id).Contains(x));
                    if (!isCheck)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PackageIdsEnteredIsIncorrect));
                        return methodResult;
                    }
                }
            }
            if (request.CourseLevels == null || request.CourseLevels.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseLevels));
                return methodResult;
            }

            if ((request.CourseTypes == null || request.CourseTypes.Count == 0) && (request.LiveCourseTypes == null || request.LiveCourseTypes.Count == 0))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseTypes), nameof(request.LiveCourseTypes));
                return methodResult;
            }

            #endregion validate

            var user = await _userManager.FindByEmailAsync(request.Email!);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                return methodResult;
            }
            else
            {
                var role = await _roleManager.FindByNameAsync(request.Role.ToString() ?? string.Empty);
                var newPassword = new PasswordGeneratorHelper(8, 10).Generate();
                Microsoft.AspNetCore.Identity.IdentityResult result;
                user = new();
                _mapper.Map(request, user);
                user.UserName = request.Email;

                #region Add Platform to User

                var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
                if (platform != null)
                {
                    user.UserPlatforms.Add(new UserPlatform
                    {
                        PlatformId = platform.Id
                    });
                }

                #endregion Add Platform to User

                result = await _userManager.CreateAsync(user, newPassword);
                if (!result.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), nameof(newPassword), newPassword);
                    return methodResult;
                }
                var human = await CreateHuman(request, user);
                human = _humanRepository.Add(human);
                await _humanRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await _userManager.AddToRoleAsync(user, request.Role.ToString());

                #region Send Code OTP

                var userOtpCode = await _userOtpCodeRepository.Queryable
                        .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted, cancellationToken);

                var randomSecure = new RandomSecureHelper();
                var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
                var otp = totp.ComputeTotp();
                if (userOtpCode == null)
                {
                    userOtpCode = new UserOtpCode
                    {
                        UserId = user.Id,
                        OTPCode = otp,
                        Status = EnumOtpCodeStatus.New,
                        ExpiredTime = DateTime.UtcNow.AddDays(_appSetting!.Otp!.StepDayWithAdmin)
                    };
                    _userOtpCodeRepository.Add(userOtpCode);
                    await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                var param = new SendOtpTemplateModel
                {
                    OtpCode = otp,
                    AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.ConfirmOtpUrl!, otp),
                    OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidDay, _appSetting!.Otp!.StepDayWithAdmin)
                };
                var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
                var sendResult = new MethodResult<bool>();
                if (!string.IsNullOrEmpty(request.Email))
                {
                    sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
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
                    LiveCourseTypes = request.LiveCourseTypes,
                    CourseLevels = request.CourseLevels,
                    CourseTypes = request.CourseTypes
                };
                human.Teacher.TeacherBankAccounts?.Add(new TeacherBankAccount
                {
                    BankAccountName = request.BankAccountName,
                    BankAccountNumber = request.BankAccountNumber,
                    BankName = request.BankName,
                    Status = EnumBankStatus.Approve
                });
                human.Code = $"TC_{stt:0000}";
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                var stt = await _cSORepository.Queryable.CountAsync();
                human.CSO = new CSO
                {
                    HumanId = human.Id,
                    CourseTypes = request.CourseTypes,
                    CourseLevels = request.CourseLevels,
                    PackageIds = request.PackageIds,
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
