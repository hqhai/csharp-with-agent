// Copyright (c) Atlantic. All rights reserved.
using System.Globalization;
using System.Text;
using System.Transactions;
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
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace Fsel.Identity.Application.Commands.UserCmd
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

        public CreateUserCommandHandler(UserManager<User> userManager,
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

        public async Task<MethodResult<UserModel>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
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
                            var newPassword = new PasswordGeneratorHelper(8, 10).Generate();
                            IdentityResult result;
                            if (user != null)
                            {
                                var hashPassword = _userManager.PasswordHasher.HashPassword(user, newPassword);
                                user.PasswordHash = hashPassword;
                                GetUser(user, request ?? new CreateUserCommand());
                                result = await _userManager.UpdateAsync(user);
                            }
                            else
                            {
                                user = new();
                                GetUser(user, request ?? new CreateUserCommand());
                                result = await _userManager.CreateAsync(user, newPassword);
                            }

                            if (!result.Succeeded)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.UserFailToCreate), nameof(newPassword), newPassword);
                                return methodResult;
                            }
                            await _userManager.AddToRoleAsync(user, request?.Role.ToString() ?? string.Empty);

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
                                    ExpiredTime = DateTime.Now.AddHours(_appSetting!.Otp!.StepTimeWithAdmin)
                                };
                                _userOtpCodeRepository.Add(userOtpCode);
                                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                userOtpCode.OTPCode = otp;
                                userOtpCode.ExpiredTime = DateTime.Now.AddHours(_appSetting!.Otp!.StepTimeWithAdmin);
                                _userOtpCodeRepository.Update(userOtpCode);
                                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }
                            var content = string.Format(CultureInfo.InvariantCulture, StringValuesContent.Url, otp);
                            var subject = StringValues.SendOtpSubject + user.FullName;

                            var sendResult = new MethodResult<bool>();
                            if (request != null && request.Email != null)
                            {
                                sendResult = await _mediator.Send(new SendOTPCommand { Email = user.Email, Content = content, Subject = subject }, cancellationToken).ConfigureAwait(false);
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
                            methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.SendAuthErorr));
                            scope.Dispose();
                        }
                    }
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }

        private static void GetUser(User user, CreateUserCommand request)
        {
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.UserName = request.Email;
            user.PhoneNumber = request.PhoneNumber;
        }
    }
}
