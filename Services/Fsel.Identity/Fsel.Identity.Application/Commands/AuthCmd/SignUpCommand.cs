// Copyright (c) Atlantic. All rights reserved.

using System.Transactions;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

        public SignUpCommandHandler(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IMediator mediator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MethodResult<UserModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();
            User? user = null;
            if (request != null && request.Email != null)
            {
                user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null && user.EmailConfirmed)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.AU08V), nameof(request.Email), request.Email);
                    return methodResult;
                }
            }
            else if (request != null && request.PhoneNumber != null)
            {
                user = await _userManager.Users.FirstOrDefaultAsync(e => e.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken);
                if (user != null && user.EmailConfirmed)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.AU09V), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }

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
                        methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.AU10ER), nameof(request.Password), request?.Password);
                        return methodResult;
                    }
                    await _userManager.AddToRoleAsync(user, request?.Role.ToString() ?? string.Empty);

                    #region Send Code OTP

                    var sendResult = new MethodResult<bool>();
                    if (request != null && request.Email != null)
                    {
                        sendResult = await _mediator.Send(new SendOTPCommand { Email = user.Email }, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
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
                    methodResult.AddError(nameof(EnumAuthErrorCode.AU11ER));
                    scope.Dispose();
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
            user.TwoFactorEnabled = true;
        }
    }
}
