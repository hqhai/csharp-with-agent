// Copyright (c) Atlantic. All rights reserved.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Master.Identity.Domain.Entities;
using Fsel.Master.Identity.Domain.IRepositories;
using Fsel.Master.Identity.Domain.Models.EntityModels;
using Fsel.Master.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Fsel.Master.Identity.Application.Commands.AuthCmd
{
    public class GenerateTokenCommand : IRequest<MethodResult<TokenModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GenerateTokenCommandHandler : IRequestHandler<GenerateTokenCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<MasterUser> _userManager;
        private readonly IMasterUserTokenRepository _userTokenRepository;
        private readonly IUserEventRepository _userEventRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSetting _appSetting;

        public GenerateTokenCommandHandler(
            UserManager<MasterUser> userManager,
            IMasterUserTokenRepository userTokenRepository,
            IUserEventRepository userEventRepository,
            AppSetting appSetting,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _userTokenRepository = userTokenRepository;
            _userEventRepository = userEventRepository;
            _appSetting = appSetting;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MethodResult<TokenModel>> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var jti = Guid.NewGuid().ToString();
            var authClaims = new List<Claim>
            {
                new Claim(JwtClaimNames.UserName, user.UserName ?? string.Empty),
                new Claim(JwtClaimNames.FullName, user.FullName ?? string.Empty),
                new Claim(JwtClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtClaimNames.UserId, user.Id.ToString()),
                new Claim(JwtClaimNames.Sub, _appSetting.Jwt?.Subject ?? string.Empty),
                new Claim(JwtClaimNames.Jti, jti),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(JwtClaimNames.Role, userRole));
            }

            var userEvent = await _userEventRepository.GetLatestByUserIdAsync(user.Id);
            if (userEvent != null)
            {
                authClaims.Add(new Claim(JwtClaimConstant.EventCode, userEvent.EventCode ?? string.Empty));
                if (userEvent.SchoolIds != null)
                {
                    foreach (var schoolId in userEvent.SchoolIds)
                    {
                        authClaims.Add(new Claim(JwtClaimConstant.SchoolIds, schoolId.ToString()));
                    }
                }
            }

            var secretKeyBytes = Encoding.ASCII.GetBytes(_appSetting.Jwt?.SecretKey ?? string.Empty);
            var signin = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _appSetting.Jwt?.Issuer ?? string.Empty,
                _appSetting.Jwt?.Audience ?? string.Empty,
                authClaims,
                expires: DateTime.UtcNow.AddMinutes(_appSetting.Jwt?.TokenValidityInMinutes ?? default),
                signingCredentials: signin
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = TokenHelper.GenerateRefreshToken();
            var forwarded = _httpContextAccessor.HttpContext?.Request?.Headers["X-Forwarded-For"];

            await _userTokenRepository.AddAsync(new MasterUserToken
            {
                Name = jti,
                Value = accessToken,
                RefreshToken = refreshToken,
                LoginProvider = JwtBearerDefaults.AuthenticationScheme,
                UserId = user.Id,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_appSetting.Jwt?.RefreshTokenValidityInDays ?? default),
                IpAddress = forwarded?.ToString()
            });

            var tokenLogin = new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = token.ValidTo.ConvertTimeFromUtc(TimeZoneInfo.Local),
                FullName = user.FullName,
                Roles = userRoles.ToList(),
                Code = user.Code,
            };

            methodResult.Result = tokenLogin;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
