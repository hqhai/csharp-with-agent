// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserSettingQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUserSettingQuery : IRequest<MethodResult<UserSettingModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserSettingQueryHandler : IRequestHandler<GetUserSettingQuery, MethodResult<UserSettingModel>>
    {
        private readonly IMapper _mapper;
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;

        public GetUserSettingQueryHandler(IMapper mapper,
                                          IUserSettingRepository userSettingRepository,
                                          AuthContext authContext,
                                          ISystemService systemService)
        {
            _mapper = mapper;
            _userSettingRepository = userSettingRepository;
            _authContext = authContext;
            _systemService = systemService;
        }

        public async Task<MethodResult<UserSettingModel>> Handle(GetUserSettingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserSettingModel> methodResult = new MethodResult<UserSettingModel>();

            var userModel = await _userSettingRepository.Queryable
                                                        .Include(x => x.UserSenderSettings)
                                                        .Where(x => x.UserId == _authContext.CurrentUserId)
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync(cancellationToken);

            var userSetting = _mapper.Map<UserSettingModel>(userModel);
            if (userSetting != null)
            {
                await SaveUserSenderSetting(userSetting);
            }

            methodResult.Result = userSetting;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SaveUserSenderSetting(UserSettingModel userSetting)
        {
            var senderConfigQuery = await _systemService.GetSenderConfigs();
            if (!senderConfigQuery.IsSuccessStatusCode)
            {
                return;
            }

            var senderConfigs = senderConfigQuery.Content?.Result?.ToList();

            senderConfigs?.ForEach(item =>
            {
                var userSenderSetting = userSetting.UserSenderSettings?.FirstOrDefault(x => x.SenderConfigId == item.Id);
                if (userSenderSetting != null)
                {
                    userSenderSetting.IsEdit = item.IsEdit;
                    userSenderSetting.Type = item.Type;
                    userSenderSetting.TemplateEmails = item.TemplateEmails;
                }
                else
                {
                    userSetting.UserSenderSettings?.Add(new UserSenderSettingModel
                    {
                        SenderConfigId = item.Id,
                        IsActive = true,
                        IsEdit = item.IsEdit,
                        Type = item.Type,
                        TemplateEmails = item.TemplateEmails
                    });
                }
            });
        }
    }
}
