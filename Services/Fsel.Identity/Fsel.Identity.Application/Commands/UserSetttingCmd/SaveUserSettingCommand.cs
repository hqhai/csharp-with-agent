// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserSetttingCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserSettings;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveUserSettingCommand : SaveUserSettingCommandModel, IRequest<MethodResult<UserSettingModel>>
    {
    }

    public class SaveUserSettingCommandHandler : IRequestHandler<SaveUserSettingCommand, MethodResult<UserSettingModel>>
    {
        private readonly IMapper _mapper;
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly AuthContext _authContext;

        public SaveUserSettingCommandHandler(IMapper mapper, IUserSettingRepository userSettingRepository, AuthContext authContext)
        {
            _mapper = mapper;
            _userSettingRepository = userSettingRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<UserSettingModel>> Handle(SaveUserSettingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserSettingModel>();

            await _userSettingRepository.ExecuteTransactionAsync(async () =>
            {
                var userSetting = await _userSettingRepository.Queryable.Include(x => x.UserSenderSettings).Where(x => x.UserId == _authContext.CurrentUserId).FirstOrDefaultAsync(cancellationToken);

                if (userSetting != null)
                {
                    _mapper.Map(request, userSetting);
                    SaveUserSenderSetting(request.UserSenderSettings, userSetting);

                    userSetting = _userSettingRepository.Update(userSetting);
                }
                else
                {
                    userSetting = _mapper.Map<UserSetting>(request);
                    SaveUserSenderSetting(request.UserSenderSettings, userSetting);

                    userSetting = _userSettingRepository.Add(userSetting);
                }
                if (!userSetting.IsValid())
                {
                    methodResult.AddErrorBadRequest(userSetting.ErrorMessages);
                    return methodResult;
                }
                userSetting.UserId = _authContext.CurrentUserId;
                await _userSettingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UserSettingModel>(userSetting);
                return methodResult;
            });

            return methodResult;
        }

        private static void SaveUserSenderSetting(IList<SaveUserSenderSetting>? userSenderSettings, UserSetting userSetting)
        {
            userSenderSettings.ForEach(item =>
            {
                var userSenderSetting = userSetting.UserSenderSettings.FirstOrDefault(x => x.SenderConfigId == item.SenderConfigId);
                if (userSenderSetting != null)
                {
                    userSenderSetting.IsActive = item.IsActive;
                }
                else
                {
                    userSetting.UserSenderSettings.Add(new UserSenderSetting
                    {
                        SenderConfigId = item.SenderConfigId,
                        IsActive = item.IsActive
                    });
                }
            });
        }
    }
}
