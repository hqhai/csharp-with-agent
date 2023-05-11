// Copyright (c) Atlantic. All rights reserved.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Services.InteractionService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class GenerateTokenCommand : IRequest<MethodResult<TokenModel>>
    {
        public string? Id { get; set; }
    }

    public class GenerateTokenCommandHandler : IRequestHandler<GenerateTokenCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IInteractionService _interactionService;
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IHumanRepository _humanRepository;
        private readonly AppSetting _appSetting;

        public GenerateTokenCommandHandler(UserManager<User> userManager,
            IInteractionService interactionService,
            IUserTokenRepository userTokenRepository,
            IHumanRepository humanRepository,
            AppSetting appSetting)
        {
            _userManager = userManager;
            _interactionService = interactionService;
            _userTokenRepository = userTokenRepository;
            _humanRepository = humanRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<TokenModel>> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();
            var user = await _userManager.FindByIdAsync(request.Id ?? string.Empty);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var jti = Guid.NewGuid().ToString();
            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FullName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, _appSetting.Jwt?.Subject ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var secretKeyBytes = Encoding.ASCII.GetBytes(_appSetting.Jwt?.SecretKey ?? string.Empty);
            var signin = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _appSetting.Jwt?.Issuer ?? string.Empty,
                _appSetting.Jwt?.Audience ?? string.Empty,
                authClaims,
                expires: DateTime.Now.AddMinutes(_appSetting.Jwt?.TokenValidityInMinutes ?? default),
                signingCredentials: signin
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = TokenHelper.GenerateRefreshToken();

            await _userTokenRepository.AddAsync(new UserToken
            {
                Name = jti,
                Value = accessToken,
                RefreshToken = refreshToken,
                LoginProvider = JwtBearerDefaults.AuthenticationScheme,
                UserId = user.Id ?? string.Empty,
                RefreshTokenExpiryTime = DateTime.Now.AddDays(_appSetting.Jwt?.RefreshTokenValidityInDays ?? default)
            });

            Guid? classId = userRoles.Contains(EnumRole.Student.ToString()) ? await GetClassId(user.Id) : null;

            var isSurvey = await _interactionService.IsSurveyCompleted(Guid.Parse(request.Id ?? string.Empty));
            if (!isSurvey.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.SurveyCalledError));
                return methodResult;
            }

            var tokenLogin = new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = token.ValidTo.ConvertTimeFromUtc(TimeZoneInfo.Local),
                FullName = user.FullName,
                ClassId = classId,
                Roles = userRoles.ToList(),
                IsSurvey = userRoles.FirstOrDefault() != EnumRole.Student.ToString() || isSurvey!.Content!.Result
            };

            methodResult.Result = tokenLogin;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Guid?> GetClassId(string? userId)
        {
            var human = await _humanRepository.Queryable.Include(x => x.Student).FirstOrDefaultAsync(x => x.UserId == userId);
            return human?.Student?.ClassId;
        }
    }
}
