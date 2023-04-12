// Copyright (c) Atlantic. All rights reserved.

using System.Text;
using System.Transactions;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly AppSetting _appSetting;

        public SignUpCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IMediator mediator,
            IUserOtpCodeRepository userOtpCodeRepository,
            AppSetting appSetting)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<UserModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();
            User? user = null;
            if (!string.IsNullOrEmpty(request.Email))
            {
                user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null && user.EmailConfirmed)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                    return methodResult;
                }
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            var role = await _roleManager.FindByNameAsync(request?.Role.ToString() ?? string.Empty);
                            if (role == null)
                            {
                                role = new Role
                                {
                                    Name = request?.Role.ToString(),
                                    NormalizedName = request?.Role.ToString(),
                                };
                                await _roleManager.CreateAsync(role);
                            }

                            IdentityResult result;
                            if (user != null)
                            {
                                var hashPassword = _userManager.PasswordHasher.HashPassword(user, request?.Password ?? string.Empty);
                                user.PasswordHash = hashPassword;
                                GetUser(user, request ?? new SignUpCommand());
                                result = await _userManager.UpdateAsync(user);
                            }
                            else
                            {
                                user = new();
                                GetUser(user, request ?? new SignUpCommand());
                                result = await _userManager.CreateAsync(user, request?.Password ?? string.Empty);
                            }

                            if (!result.Succeeded)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.UserFailToCreate), nameof(request.Password), request?.Password);
                                return methodResult;
                            }
                            await _userManager.AddToRoleAsync(user, request?.Role.ToString() ?? string.Empty);
                            //await _userManager..SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                            #region Send Code OTP

                            var userOtpCode = await _userOtpCodeRepository.Queryable
                                    .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumStatusUser.New && !x.IsDeleted, cancellationToken);

                            RandomSecureHelper randomSecure = new RandomSecureHelper();
                            var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
                            var otp = totp.ComputeTotp();
                            if (userOtpCode == null)
                            {
                                userOtpCode = new UserOtpCode
                                {
                                    UserId = user.Id,
                                    OTPCode = otp,
                                    Status = EnumStatusUser.New,
                                    ExpiredTime = DateTime.Now.AddSeconds(_appSetting!.Otp!.StepTime)
                                };
                                _userOtpCodeRepository.Add(userOtpCode);
                                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                userOtpCode.OTPCode = otp;
                                userOtpCode.ExpiredTime = DateTime.Now.AddSeconds(_appSetting!.Otp!.StepTime);
                                _userOtpCodeRepository.Update(userOtpCode);
                                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }
                            var sendResult = new MethodResult<bool>();
                            if (request != null && request.Email != null)
                            {
                                sendResult = await _mediator.Send(new SendOTPCommand { Email = user.Email, FullName = user.FullName, Otp = otp }, cancellationToken).ConfigureAwait(false);
                            }

                            if (!sendResult.IsOK)
                            {
                                scope.Dispose();
                                methodResult.StatusCode = sendResult?.StatusCode ?? default;
                                methodResult.AddResultFromErrorList(sendResult?.ErrorMessages);
                                return methodResult;
                            }
                            scope.Complete();

                            #endregion Send Code OTP
                        }
                        catch
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddError(nameof(EnumAuthErrorCode.SendAuthErorr));
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
                    methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }

        private static void GetUser(User user, SignUpCommandModel request)
        {
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.UserName = request.Email;
            user.PhoneNumber = request.PhoneNumber;
        }
    }
}
