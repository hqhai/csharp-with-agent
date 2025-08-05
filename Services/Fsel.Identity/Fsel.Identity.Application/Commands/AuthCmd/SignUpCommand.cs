// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Managers;
using Fsel.Core.Entities;
using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
using Fsel.Identity.Application.Commands.UserReferrals;
using Fsel.Identity.Application.Queries.UserReferrals;
using Fsel.Identity.Application.Services.TrainingService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Enums.ErrorCodes;
using Fsel.Shared.Models.SenderTemplates;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class SignUpCommand : SignUpCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPlatformRepository _platformRepository;
        private readonly AppSetting _appSetting;
        private readonly UserDbContext _userDbContext;
        private readonly ILogger<SignUpCommandHandler> _logger;

        public SignUpCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IMediator mediator,
            AppSetting appSetting,
            IPlatformRepository platformRepository,
            ILogger<SignUpCommandHandler> logger,
            UserDbContext userDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mediator = mediator;
            _appSetting = appSetting;
            _platformRepository = platformRepository;
            _logger = logger;
            _userDbContext = userDbContext;
        }

        public async Task<MethodResult<UserModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            User? user = null;

            if (!string.IsNullOrEmpty(request.ReferralCode))
            {
                var checkReferralCodeResult = await _mediator.Send(new CheckReferralCodeQuery() { ReferralCode = request.ReferralCode }, cancellationToken);
                if (!checkReferralCodeResult.IsOK || checkReferralCodeResult.Result == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserReferralErrorCode.FriendCodeDoesNotExist));
                    return methodResult;
                }
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                if (!request.PhoneNumber.IsValidPhoneNumber())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PhoneNumberIsNotValid), nameof(request.PhoneNumber));
                    return methodResult;
                }
                user = await _userManager.Users.Include(x => x.Human).FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber.Trim(), cancellationToken: cancellationToken);

                if (user != null && user.Status.HasValue && user.Status == EnumUserStatus.Disable)
                {
                    methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.AccountHasBeenCutOff), new Error(nameof(request.Email), request.Email));
                    return methodResult;
                }
                if (user != null && (user.EmailConfirmed || user.Human != null))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }
            if (!string.IsNullOrEmpty(request.Email))
            {
                if (!request.Email.IsValidEmail())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                    return methodResult;
                }
                user = await _userManager.Users.Include(x => x.Human).FirstOrDefaultAsync(x => x.Email == request.Email.Trim(), cancellationToken: cancellationToken);

                if (user != null && user.Status.HasValue && user.Status == EnumUserStatus.Disable)
                {
                    methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.AccountHasBeenCutOff), new Error(nameof(request.Email), request.Email));
                    return methodResult;
                }
                if (user != null && (user.EmailConfirmed || user.Human != null))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                    return methodResult;
                }

                //using (var scope = new TransactionScope(TransactionScopeOption.Required,
                //    new TransactionOptions
                //    {
                //        IsolationLevel = IsolationLevel.ReadCommitted
                //    },
                //    TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        ArgumentNullException.ThrowIfNull(request);
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

                        var passwordValidator = new Microsoft.AspNetCore.Identity.PasswordValidator<User>();
                        Microsoft.AspNetCore.Identity.IdentityResult result;
                        if (user != null)
                        {
                            _mapper.Map(request, user);

                            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, request.Password);
                            if (!validPassword.Succeeded)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
                                return methodResult;
                            }

                            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password ?? string.Empty);
                            user.UserName = request.Email;
                            if (!user.IsValid())
                            {
                                methodResult.AddErrorBadRequest(user.ErrorMessages);
                                return methodResult;
                            }
                            result = await _userManager.UpdateAsync(user);
                            if (!result.Succeeded)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), nameof(result.Errors));
                                return methodResult;
                            }
                        }
                        else
                        {
                            user = new();
                            _mapper.Map(request, user);
                            user.UserName = request.Email;
                            if (!user.IsValid())
                            {
                                methodResult.AddErrorBadRequest(user.ErrorMessages);
                                return methodResult;
                            }

                            #region Add Platform to User

                            request.PlatformCode ??= EnumPlatformCode.LMS;
                            var platform = await _platformRepository.GetPlatformAsync(request.PlatformCode.Value, cancellationToken);
                            if (platform != null)
                            {
                                user.UserPlatforms.Add(new UserPlatform
                                {
                                    PlatformId = platform.Id
                                });
                            }

                            #endregion Add Platform to User

                            #region add user setting

                            user.UserSettings = new List<UserSetting>()
                                    {
                                        new UserSetting(true)
                                    };

                            #endregion add user setting

                            //#region add user role

                            //user.UserRoles = new List<UserRoleEntity>()
                            //    {
                            //        new UserRoleEntity
                            //        {
                            //            RoleId = role.Id
                            //        }
                            //    };

                            //#endregion add user role

                            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, request.Password);
                            if (!validPassword.Succeeded)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
                                return methodResult;
                            }

                            result = await _userManager.CreateAsync(user, request.Password ?? string.Empty);
                            if (!result.Succeeded)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                                return methodResult;
                            }

                            await _userDbContext.UserRoles.AddAsync(new UserRole
                            {
                                UserId = user.Id,
                                RoleId = role.Id
                            }, cancellationToken);

                            //await _userManager.AddToRoleAsync(user, request.Role.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SignUpCommand encouters error: {message}", ex.Message);
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), ex.Message);
                        return methodResult;
                    }

                    #region Send Code OTP

                    try
                    {
                        var userOtpCode = await _mediator.Send(new SaveUserOtpCodeCommand { Id = user.Id }, cancellationToken);
                        var param = new SendOtpTemplateModel
                        {
                            OtpCode = userOtpCode.Result,
                            OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidMinute, _appSetting!.Otp!.StepTime)
                        };
                        var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
                        var sendResult = new MethodResult<bool>();

                        ArgumentNullException.ThrowIfNull(request);
                        if (!string.IsNullOrEmpty(request.Email))
                        {
                            sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, IsCCEmailDefault = true, Template = EnumSenderTemplate.SendOtp }, cancellationToken).ConfigureAwait(false);
                        }
                        else if (!string.IsNullOrEmpty(request.PhoneNumber))
                        {
                            sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject }, cancellationToken).ConfigureAwait(false);
                        }

                        if (!sendResult.IsOK)
                        {
                            //scope.Dispose();
                            methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                            return methodResult;
                        }
                        //scope.Complete();

                        #endregion Send Code OTP
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SignUpCommand SendEmail encouters error: {message}", ex.Message);
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.SendAuthErorr), ex.Message);
                        return methodResult;
                        //scope.Dispose();
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.ReferralCode))
            {
                var updateReferralCodeResult = await _mediator.Send(new CreateUserReferralCommand { ReferralCode = request.ReferralCode, ReceiverId = user.Id, UserReferralType = EnumUserReferralType.Link }, cancellationToken).ConfigureAwait(false);
                if (!updateReferralCodeResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(updateReferralCodeResult.ErrorMessages);
                    return methodResult;
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
