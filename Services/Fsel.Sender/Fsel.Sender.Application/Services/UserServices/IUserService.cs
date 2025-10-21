// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.UserServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Application.Services.UserServices.Models;
    using Fsel.Sender.Application.Services.UserServices.QueryModels;
    using Refit;

    public interface IUserService
    {
        [Post("/v1/user-setting/user-setting-emails")]
        Task<IApiResponse<MethodResult<IList<UserSettingEmailModel>>>> GetUserSettingsByEmailsQuery(GetUserSettingsByEmailsModel query);
    }
}
