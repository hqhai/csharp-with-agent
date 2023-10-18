// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using System.Text;
using System.Transactions;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Commands.StudentCmd;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.SenderTemplates;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OtpNet;

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
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly AppSetting _appSetting;

        public SignUpCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IMediator mediator,
            IUserOtpCodeRepository userOtpCodeRepository,
            AppSetting appSetting,
            IPlatformRepository platformRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
            _platformRepository = platformRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();
            User? user = null;
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken);
                if (user != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }
            if (!string.IsNullOrEmpty(request.Email))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken);
                if (user != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }

                user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null && user.EmailConfirmed)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                    return methodResult;
                }
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            ArgumentNullException.ThrowIfNull(request);
                            var role = await _roleManager.FindByNameAsync(request.Role.ToString() ?? string.Empty);
                            if (role == null)
                            {
                                role = new Role
                                {
                                    Name = request.Role.ToString(),
                                    NormalizedName = request.Role.ToString(),
                                };
                                await _roleManager.CreateAsync(role);
                            }

                            Microsoft.AspNetCore.Identity.IdentityResult result;
                            if (user != null)
                            {
                                var hashPassword = _userManager.PasswordHasher.HashPassword(user, request.Password ?? string.Empty);
                                user.PasswordHash = hashPassword;
                                _mapper.Map(request, user);
                                user.UserName = request.Email;
                                if (!user.IsValid())
                                {
                                    methodResult.AddErrorBadRequest(user.ErrorMessages);
                                    return methodResult;
                                }
                                result = await _userManager.UpdateAsync(user);
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

                                result = await _userManager.CreateAsync(user, request.Password ?? string.Empty);
                                if (!result.Succeeded)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), nameof(request.Password), request.Password);
                                    return methodResult;
                                }
                                await _userManager.AddToRoleAsync(user, request.Role.ToString() ?? string.Empty);

                                if (!string.IsNullOrEmpty(request.ReferralCode))
                                {
                                    var updateReferralCodeResult = await _mediator.Send(new UpdateReferralCodeStudentCommand { ReferralCode = request.ReferralCode, UserId = user.Id }, cancellationToken).ConfigureAwait(false);
                                    if (!updateReferralCodeResult.IsOK)
                                    {
                                        methodResult.AddError(updateReferralCodeResult.ErrorMessages);
                                        return methodResult;
                                    }
                                }
                            }

                            #region Send Code OTP

                            var userOtpCode = await _userOtpCodeRepository.Queryable
                                    .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted, cancellationToken);

                            RandomSecureHelper randomSecure = new RandomSecureHelper();
                            var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
                            var otp = totp.ComputeTotp();
                            if (userOtpCode == null)
                            {
                                userOtpCode = new UserOtpCode
                                {
                                    UserId = user.Id,
                                    OTPCode = otp,
                                    Status = EnumOtpCodeStatus.New,
                                    ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime)
                                };
                                _userOtpCodeRepository.Add(userOtpCode);
                                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                userOtpCode.OTPCode = otp;
                                userOtpCode.ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime);
                                _userOtpCodeRepository.Update(userOtpCode);
                                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }

                            var param = new SendOtpTemplateModel
                            {
                                OtpCode = userOtpCode.OTPCode,
                                OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidMinute, _appSetting!.Otp!.StepTime)
                            };
                            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
                            var sendResult = new MethodResult<bool>();

                            ArgumentNullException.ThrowIfNull(request);
                            if (!string.IsNullOrEmpty(request.Email))
                            {
                                sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtp }, cancellationToken).ConfigureAwait(false);
                            }
                            else if (!string.IsNullOrEmpty(request.PhoneNumber))
                            {
                                sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject }, cancellationToken).ConfigureAwait(false);
                            }

                            if (!sendResult.IsOK)
                            {
                                scope.Dispose();
                                methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                                return methodResult;
                            }
                            scope.Complete();

                            #endregion Send Code OTP
                        }
                        catch
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.SendAuthErorr));
                            scope.Dispose();
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(e => e.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken);
                if (user != null && user.EmailConfirmed)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
