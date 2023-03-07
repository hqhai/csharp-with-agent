using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.User.Common.ConfigSettings;
using Fsel.User.Common.Models.Commands;
using Fsel.User.Common.Models.Entities;
using Fsel.User.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Fsel.User.Application.Commands.AuthCmd
{
    public class RefreshTokenCommand : RefreshTokenCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<Account> _userManager;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;

        public RefreshTokenCommandHandler(UserManager<Account> userManager,
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
                if (!result)//false
                {
                    methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                    methodResult.AddErrorMessage("Invalid token");
                    return methodResult;
                }
            }

            //check 3: Check accessToken expire?
            long.TryParse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value, out long utcExpireDate);

            var expireDate = utcExpireDate.UnixTimeStampToDateTime();
            if (expireDate > DateTime.UtcNow)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.AddErrorMessage("Access token has not yet expired");
                return methodResult;
            }

            //check 4: Check refreshtoken exist in DB
            var user = _userManager.Users.FirstOrDefault(x => x.RefreshToken == request.RefreshToken);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage("Refresh token does not exist");
                return methodResult;
            }
            else if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime.Value <= DateTime.Now)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                methodResult.AddErrorMessage("Refresh token has expired");
                return methodResult;
            }

            methodResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }).ConfigureAwait(false);
            return methodResult;
        }
    }
}