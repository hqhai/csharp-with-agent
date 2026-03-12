// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserSetttingCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserSettings;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateSenderSettingCommand : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
        public EnumSenderTemplate Template { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateSenderSettingCommandHandler : IRequestHandler<UpdateSenderSettingCommand, MethodResult<bool>>
    {
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly ISystemService _systemService;
        private readonly AppSetting _appSetting;
        private readonly UserManager<User> _userManager;

        public UpdateSenderSettingCommandHandler(IUserSettingRepository userSettingRepository,
                                                 ISystemService systemService,
                                                 AppSetting appSetting,
                                                 UserManager<User> userManager)
        {
            _userSettingRepository = userSettingRepository;
            _systemService = systemService;
            _appSetting = appSetting;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(UpdateSenderSettingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);
            if (user == null)
            {
                return methodResult;
            }

            var model = new UpdateSenderSettingCommandModel
            {
                UserId = user.Id,
                Template = request.Template
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
    }
}
