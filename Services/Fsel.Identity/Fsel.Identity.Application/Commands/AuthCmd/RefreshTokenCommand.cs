// Copyright (c) Atlantic. All rights reserved.

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class RefreshTokenCommand : RefreshTokenCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;

        public RefreshTokenCommandHandler(UserManager<User> userManager,
            AppSetting appSetting,
            IMediator mediator)
        {
            _userManager = userManager;
            _mediator = mediator;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<TokenModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.ASCII.GetBytes(_appSetting.Jwt?.SecretKey ?? string.Empty);
            var tokenValidateParam = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false
            };
            var tokenInVerification = jwtTokenHandler.ValidateToken(request.AccessToken, tokenValidateParam, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                if (!result)
                {
                    methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.AU06ER));
                    return methodResult;
                }
            }

            long.TryParse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value, out long utcExpireDate);

            var expireDate = utcExpireDate.ConvertUnixTimeStampToDateTime();
            if (expireDate > DateTime.UtcNow)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.AU07ER));
                return methodResult;
            }

            var user = _userManager.Users.FirstOrDefault(x => x.RefreshToken == request.RefreshToken);
            if (user == null)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.AU06ER),
                    nameof(request.RefreshToken), request.RefreshToken);
                return methodResult;
            }
            else if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime.Value <= DateTime.Now)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthErrorCode.AU09ER));
            }

            methodResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);
            return methodResult;
        }
    }
}
