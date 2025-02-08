// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.GAPIT
{
    using Fsel.Sender.Application.Services.SMSServices.GAPIT.Models;
    using Refit;

    public interface IGAPITService
    {
        [Post("/V1/SendMtbulk")]
        Task<IApiResponse<GAPITSendSMSResponseModels>> SendSMSs([Body] GAPITSendSMSRequestModels model, [Header("Authorization")] string token);
    }
}
