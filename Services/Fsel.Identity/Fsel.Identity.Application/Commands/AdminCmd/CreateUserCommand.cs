// Copyright (c) Atlantic. All rights reserved.
using System.Globalization;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Commands.AuthCmd;
using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
using Fsel.Identity.Application.Services.OrderService;
using Fsel.Identity.Domain.Entities;
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
        private readonly AppSetting _appSetting;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICSORepository _cSORepository;
        private readonly IPlatformRepository _platformRepository;

        public CreateUserCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IOrderService orderService,
            IMediator mediator,
            AppSetting appSetting,
            ITeacherRepository teacherRepository,
            ICSORepository cSORepository,
            IPlatformRepository platformRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _orderService = orderService;
            _mediator = mediator;
            _appSetting = appSetting;
            _teacherRepository = teacherRepository;
            _cSORepository = cSORepository;
            _platformRepository = platformRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            #region validate

            if (string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Email), request.Email);
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
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                return methodResult;
            }
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            #endregion validate

            var role = await _roleManager.FindByNameAsync(request.Role.ToString() ?? string.Empty);
            var newPassword = new PasswordGeneratorHelper(8, 10).Generate();
            Microsoft.AspNetCore.Identity.IdentityResult result;
            user = new();
            _mapper.Map(request, user);
            user.UserName = request.Email;
            user = await GetUserByRoleAsync(request, user);

            #region Add Platform to User

            switch (request.Role)
            {
                case EnumRoleRegisterWithAdmin.CSO:
                case EnumRoleRegisterWithAdmin.Teacher:
                case EnumRoleRegisterWithAdmin.Moderator:
                    await AddUserToPlatForm(user, EnumPlatformCode.LMSAdmin, cancellationToken);
                    break;

                default:
                    await AddUserToPlatForm(user, EnumPlatformCode.LMS, cancellationToken);
                    break;
            }

            #endregion Add Platform to User

            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }
            result = await _userManager.CreateAsync(user, newPassword);
            if (!result.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), nameof(newPassword), newPassword);
                return methodResult;
            }

            await _userManager.AddToRoleAsync(user, request.Role.ToString());

            #region Send Code OTP

            var userOtpCode = await _mediator.Send(new SaveUserOtpCodeCommand { Id = user.Id }, cancellationToken);
            var param = new SendOtpTemplateModel
            {
                OtpCode = userOtpCode.Result,
                AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.ConfirmOtpUrl!, userOtpCode.Result, user.Id),
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

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }

        private async Task<User> GetUserByRoleAsync(CreateUserCommand request, User user)
        {
            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                var stt = await _teacherRepository.Queryable.CountAsync();
                user.Teacher = new Teacher
                {
                    UserId = user.Id,
                    LiveCourseTypes = request.LiveCourseTypes,
                    CourseLevels = request.CourseLevels,
                    CourseTypes = request.CourseTypes
                };
                user.Teacher.TeacherBankAccounts?.Add(new TeacherBankAccount
                {
                    BankAccountName = request.BankAccountName,
                    BankAccountNumber = request.BankAccountNumber,
                    BankName = request.BankName,
                    Status = EnumBankStatus.Approve
                });
                user.Code = $"TC_{stt:0000}";
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                var stt = await _cSORepository.Queryable.CountAsync();
                user.CSO = new CSO
                {
                    UserId = user.Id,
                    CourseTypes = request.CourseTypes,
                    CourseLevels = request.CourseLevels,
                    PackageIds = request.PackageIds,
                };
                user.Code = $"CSO_{stt:0000}";
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.Moderator)
            {
                user.Code = "Moderator";
            }
            return user;
        }

        private async Task AddUserToPlatForm(User user, EnumPlatformCode platformCode, CancellationToken cancellationToken)
        {
            var platform = await _platformRepository.GetPlatformAsync(platformCode, cancellationToken);
            if (platform != null)
            {
                user.UserPlatforms.Add(new UserPlatform
                {
                    PlatformId = platform.Id
                });
            }
        }
    }
}
