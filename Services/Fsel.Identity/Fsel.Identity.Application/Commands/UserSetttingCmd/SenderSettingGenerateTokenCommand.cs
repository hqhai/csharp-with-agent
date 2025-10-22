// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserSetttingCmd
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Models.CommandModels.UserSettings;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.IdentityModel.Tokens;

    public class SenderSettingGenerateTokenCommand : UpdateSenderSettingCommandModel, IRequest<MethodResult<string>>
    {
    }

    public class SenderSettingGenerateTokenCommandHandler : IRequestHandler<SenderSettingGenerateTokenCommand, MethodResult<string>>
    {
        private readonly AppSetting _appSetting;

        public SenderSettingGenerateTokenCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public Task<MethodResult<string>> Handle(SenderSettingGenerateTokenCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();

            var claims = new List<Claim>
            {
                 new Claim("UserId", request.UserId.ToString()),
                 new Claim("Template", request.Template.ToString())
            };

            var secretKeyBytes = Encoding.ASCII.GetBytes(_appSetting.SenderJwt?.SecretKey ?? string.Empty);
            var signin = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _appSetting.SenderJwt?.Issuer ?? string.Empty,
                _appSetting.SenderJwt?.Audience ?? string.Empty,
                claims,
                expires: DateTime.UtcNow.AddMinutes(_appSetting.SenderJwt?.Expiration ?? default),
                signingCredentials: signin
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            methodResult.Result = accessToken;
            return Task.FromResult(methodResult);
        }
    }
}
