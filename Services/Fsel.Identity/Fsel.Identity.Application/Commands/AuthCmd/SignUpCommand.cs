// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using System.Transactions;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
using Fsel.Identity.Application.Commands.UserReferrals;
using Fsel.Identity.Application.Queues.Publishers;
using Fsel.Identity.Application.Services.TrainingService;
using Fsel.Identity.Domain.Entities;
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
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ITrainingService _trainingService;
        private readonly IHumanRepository _humanRepository;

        public SignUpCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IMediator mediator,
            AppSetting appSetting,
            IPlatformRepository platformRepository,
            QuestBoardPublisher questBoardPublisher,
            ITrainingService trainingService,
            IHumanRepository humanRepository
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mediator = mediator;
            _appSetting = appSetting;
            _platformRepository = platformRepository;
            _questBoardPublisher = questBoardPublisher;
            _trainingService = trainingService;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
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
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken);
                if (user != null && user.EmailConfirmed)
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
                                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
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
                                await _userManager.AddToRoleAsync(user, request.Role.ToString() ?? string.Empty);
                            }

                            #region Send Code OTP

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
