// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Auths;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SignUpSMSCommand : SignUpSMSCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class SignUpSMSCommandHandler : IRequestHandler<SignUpSMSCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPlatformRepository _platformRepository;
        private readonly UserDbContext _userDbContext;
        private readonly ILogger<SignUpSMSCommandHandler> _logger;

        public SignUpSMSCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IMediator mediator,
            IPlatformRepository platformRepository,
            ILogger<SignUpSMSCommandHandler> logger,
            UserDbContext userDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mediator = mediator;
            _platformRepository = platformRepository;
            _logger = logger;
            _userDbContext = userDbContext;
        }

        public async Task<MethodResult<UserModel>> Handle(SignUpSMSCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            User? user = null;

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
                    methodResult.AddError(StatusCodes.Status400BadRequest, nameof(EnumAuthUserErrorCode.AccountHasBeenCutOff), new Error(nameof(request.Email), request.Email));
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
                    methodResult.AddError(StatusCodes.Status400BadRequest, nameof(EnumAuthUserErrorCode.AccountHasBeenCutOff), new Error(nameof(request.Email), request.Email));
                    return methodResult;
                }
                if (user != null && (user.EmailConfirmed || user.Human != null))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                    return methodResult;
                }

                {
                    try
                    {
                        ArgumentNullException.ThrowIfNull(request);
                        var role = await _roleManager.FindByNameAsync(EnumRole.Student.ToString());
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

                            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
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
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SignUpSMSCommand encouters error: {message}", ex.Message);
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), ex.Message);
                        return methodResult;
                    }

                    #region Send Code OTP

                    try
                    {
                        var userOtpCodeResult = await _mediator.Send(new SaveUserOtpCodeSMSCommand { Id = user.Id }, cancellationToken);
                        if (!userOtpCodeResult.IsOK)
                        {
                            methodResult.AddErrorBadRequest(userOtpCodeResult.ErrorMessages);
                            return methodResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.SendAuthErorr), ex.Message);
                        return methodResult;
                    }

                    #endregion Send Code OTP
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
