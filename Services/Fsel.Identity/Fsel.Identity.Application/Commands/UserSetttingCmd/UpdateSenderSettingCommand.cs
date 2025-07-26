// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserSetttingCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
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

        public UpdateSenderSettingCommandHandler(IUserSettingRepository userSettingRepository,
                                                 ISystemService systemService)
        {
            _userSettingRepository = userSettingRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(UpdateSenderSettingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            await _userSettingRepository.ExecuteTransactionAsync(async () =>
            {
                var userSetting = await _userSettingRepository.Queryable
                                                              .Include(x => x.UserSenderSettings).Where(x => x.UserId == request.UserId)
                                                              .FirstOrDefaultAsync(cancellationToken);

                if (userSetting != null)
                {
                    await SaveUserSenderSetting(request, userSetting);
                    userSetting = _userSettingRepository.Update(userSetting);
                }
                else
                {
                    var newUserSetting = new UserSetting(true);
                    await SaveUserSenderSetting(request, newUserSetting);
                    userSetting = _userSettingRepository.Add(newUserSetting);
                }

                if (!userSetting.IsValid())
                {
                    methodResult.AddErrorBadRequest(userSetting.ErrorMessages);
                    return methodResult;
                }

                userSetting.UserId = request.UserId;
                await _userSettingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private async Task SaveUserSenderSetting(UpdateSenderSettingCommand request, UserSetting userSetting)
        {
            var senderConfigQuery = await _systemService.GetSenderConfigs();
            if (!senderConfigQuery.IsSuccessStatusCode)
            {
                return;
            }

            var senderConfigs = senderConfigQuery.Content?.Result?.ToList();
            var senderConfig = senderConfigs?.FirstOrDefault(x => x.TemplateEmails != null && x.TemplateEmails.Contains(request.Template));

            if (senderConfig != null && senderConfig.IsEdit)
            {
                var userSenderSetting = userSetting.UserSenderSettings.FirstOrDefault(x => x.SenderConfigId == senderConfig.Id);
                if (userSenderSetting != null)
                {
                    userSenderSetting.IsActive = request.IsActive;
                }
                else
                {
                    userSetting.UserSenderSettings.Add(new UserSenderSetting
                    {
                        SenderConfigId = senderConfig.Id,
                        IsActive = request.IsActive
                    });
                }
            }
        }
    }
}
