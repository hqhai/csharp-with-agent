// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserSetttingCmd
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserSettings;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;

    public class UpdateSenderSettingCommand : IRequest<MethodResult<bool>>
    {
        public string? Token { get; set; }

        public bool IsActive { get; set; }
    }

    public class UpdateSenderSettingCommandHandler : IRequestHandler<UpdateSenderSettingCommand, MethodResult<bool>>
    {
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly ISystemService _systemService;
        private readonly AppSetting _appSetting;
        private const string ExpiredError = "Token has expired!";
        private const string FailedError = "Token validation failed";
        private const string UnexpectedError = "An unexpected error occurred";

        public UpdateSenderSettingCommandHandler(IUserSettingRepository userSettingRepository,
                                                 ISystemService systemService,
                                                 AppSetting appSetting)
        {
            _userSettingRepository = userSettingRepository;
            _systemService = systemService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(UpdateSenderSettingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Token);
            var methodResult = new MethodResult<bool>();

            var validate = ValidateToken(request.Token);
            if (!validate.IsOK)
            {
                methodResult.AddErrorBadRequest(validate.ErrorMessages);
                return methodResult;
            }

            var claimsPrincipal = validate.Result;

            string? userId = claimsPrincipal?.FindFirst("UserId")?.Value;
            string? template = claimsPrincipal?.FindFirst("Template")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(template))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request));
                return methodResult;
            }

            var model = new UpdateSenderSettingCommandModel
            {
                UserId = Guid.Parse(userId),
                Template = (EnumSenderTemplate)Enum.Parse(typeof(EnumSenderTemplate), template)
            };

            await _userSettingRepository.ExecuteTransactionAsync(async () =>
            {
                var userSetting = await _userSettingRepository.Queryable
                                                              .Include(x => x.UserSenderSettings).Where(x => x.UserId == model.UserId)
                                                              .FirstOrDefaultAsync(cancellationToken);

                if (userSetting != null)
                {
                    await SaveUserSenderSetting(model, userSetting, request.IsActive);
                    userSetting = _userSettingRepository.Update(userSetting);
                }
                else
                {
                    var newUserSetting = new UserSetting(true);
                    await SaveUserSenderSetting(model, newUserSetting, request.IsActive);
                    userSetting = _userSettingRepository.Add(newUserSetting);
                }

                if (!userSetting.IsValid())
                {
                    methodResult.AddErrorBadRequest(userSetting.ErrorMessages);
                    return methodResult;
                }

                userSetting.UserId = model.UserId;
                await _userSettingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private async Task SaveUserSenderSetting(UpdateSenderSettingCommandModel request, UserSetting userSetting, bool isActive)
        {
            var senderConfigQuery = await _systemService.GetSenderConfigs();
            if (!senderConfigQuery.IsSuccessStatusCode)
            {
                return;
            }

            var senderConfigs = senderConfigQuery.Content?.Result?.ToList();
            //var senderConfig = senderConfigs?.FirstOrDefault(x => x.TemplateEmails != null && x.TemplateEmails.Contains(request.Template));
            var senderConfig = senderConfigs?.FirstOrDefault(x => x.IsEdit);
            if (senderConfig != null && senderConfig.IsEdit)
            {
                var userSenderSetting = userSetting.UserSenderSettings.FirstOrDefault(x => x.SenderConfigId == senderConfig.Id);
                if (userSenderSetting != null)
                {
                    userSenderSetting.IsActive = isActive;
                }
                else
                {
                    userSetting.UserSenderSettings.Add(new UserSenderSetting
                    {
                        SenderConfigId = senderConfig.Id,
                        IsActive = isActive
                    });
                }
            }
        }

        private MethodResult<ClaimsPrincipal> ValidateToken(string token)
        {
            MethodResult<ClaimsPrincipal> methodResult = new MethodResult<ClaimsPrincipal>();
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // rule xác thực
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSetting.SenderJwt?.SecretKey ?? string.Empty)),
                    ValidateIssuer = true,
                    ValidIssuer = _appSetting.SenderJwt?.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _appSetting.SenderJwt?.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // xác thực token và trả về ClaimsPrincipal
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

                methodResult.Result = principal;
                return methodResult;
            }
            catch (SecurityTokenExpiredException)
            {
                methodResult.AddErrorBadRequest(ExpiredError);
                return methodResult;
            }
            catch (SecurityTokenValidationException ex)
            {
                methodResult.AddErrorBadRequest($"{FailedError}: {ex.Message}");
                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest($"{UnexpectedError}: {ex.Message}");
                return methodResult;
            }
        }
    }
}
