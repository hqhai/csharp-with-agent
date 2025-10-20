// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserSettingQuery
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUserSettingsByEmailsQuery : IRequest<MethodResult<IList<UserSettingEmailModel>>>
    {
        public IList<string>? Emails { get; set; }
    }

    public class GetUserSettingsByEmailsQueryHandler : IRequestHandler<GetUserSettingsByEmailsQuery, MethodResult<IList<UserSettingEmailModel>>>
    {
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly ISystemService _systemService;
        private readonly UserManager<User> _userManager;

        public GetUserSettingsByEmailsQueryHandler(IUserSettingRepository userSettingRepository,
                                                   ISystemService systemService,
                                                   UserManager<User> userManager)
        {
            _userSettingRepository = userSettingRepository;
            _systemService = systemService;
            _userManager = userManager;
        }

        public async Task<MethodResult<IList<UserSettingEmailModel>>> Handle(GetUserSettingsByEmailsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UserSettingEmailModel>> methodResult = new MethodResult<IList<UserSettingEmailModel>>();

            if (request.Emails == null || request.Emails.Count == 0)
            {
                methodResult.Result = new List<UserSettingEmailModel>();
                return methodResult;
            }

            request.Emails = request.Emails.Select(x => x.Trim().ToLower(CultureInfo.CurrentCulture)).ToList() ?? new List<string>();

            var users = _userManager.Users.WhereBulkContains(request.Emails, x => x.Email);
            var userSettings = _userSettingRepository.Queryable.Include(x => x.UserSenderSettings);

            var query = from a in users
                        join b in userSettings on a.Id equals b.UserId
                        select new UserSettingEmailModel
                        {
                            Email = a.Email,
                            UserSenderSettings = b.UserSenderSettings.Select(item => new UserSenderSettingModel
                            {
                                SenderConfigId = item.SenderConfigId,
                                IsActive = item.IsActive
                            }).ToList()
                        };

            var userSenderSettings = await query.AsNoTracking().ToListAsync(cancellationToken);

            if (userSenderSettings != null && userSenderSettings.Any())
            {
                await UserSenderSettingHandler(userSenderSettings);
            }

            methodResult.Result = userSenderSettings;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UserSenderSettingHandler(IList<UserSettingEmailModel> userSettings)
        {
            var senderConfigQuery = await _systemService.GetSenderConfigs();
            if (!senderConfigQuery.IsSuccessStatusCode)
            {
                return;
            }

            var senderConfigs = senderConfigQuery.Content?.Result?.ToList();

            foreach (var userSetting in userSettings)
            {
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
}
