// Copyright (c) Atlantic. All rights reserved.

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Fsel.Common.ActionResults;
using Fsel.Master.Identity.Domain.Enums.ErrorCodes;
using Fsel.Master.Identity.Domain.IRepositories;
using Fsel.Master.Identity.Domain.Models.CommandModels;
using Fsel.Master.Identity.Domain.Models.EntityModels;
using Fsel.Master.Identity.Infrastructure.ValueSettings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Fsel.Master.Identity.Application.Commands.AuthCmd
{
    public class RefreshTokenCommand : RefreshTokenCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, MethodResult<TokenModel>>
    {
        private readonly IMasterUserTokenRepository _userTokenRepository;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;

        public RefreshTokenCommandHandler(
            IMasterUserTokenRepository userTokenRepository,
            AppSetting appSetting,
            IMediator mediator)
        {
            _userTokenRepository = userTokenRepository;
            _mediator = mediator;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<TokenModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();
            ArgumentNullException.ThrowIfNull(request);

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.ASCII.GetBytes(_appSetting.Jwt?.SecretKey ?? string.Empty);
            var tokenValidateParam = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = _appSetting?.Jwt?.Audience,
                ValidIssuer = _appSetting?.Jwt?.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                ClockSkew = TimeSpan.Zero,
            };
            var tokenValidationResult = await jwtTokenHandler.ValidateTokenAsync(request.AccessToken, tokenValidateParam);

            if (tokenValidationResult.SecurityToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase);
                if (!result)
                {
                    methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumMasterAuthErrorCode.InvalidToken));
                    return methodResult;
                }
            }

            var refreshToken = await _userTokenRepository.GetByRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized,
                    nameof(EnumMasterAuthErrorCode.InvalidToken), nameof(request.RefreshToken), request.RefreshToken);
                return methodResult;
            }
            else if (refreshToken.RefreshTokenExpiryTime == null || refreshToken.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized,
                    nameof(EnumMasterAuthErrorCode.RefreshTokenExpired), nameof(request.RefreshToken), request.RefreshToken);
                return methodResult;
            }

            await _userTokenRepository.RemoveAsync(refreshToken);

            methodResult = await _mediator.Send(new GenerateTokenCommand { Id = refreshToken.UserId }, cancellationToken).ConfigureAwait(false);
            return methodResult;
        }
    }
}
