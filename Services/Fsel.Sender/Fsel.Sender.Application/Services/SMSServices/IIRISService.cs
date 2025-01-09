// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices
{
    using System.Threading.Tasks;
    using Fsel.Sender.Application.Services.SMSServices.Models;
    using Refit;

    public interface IIRISService
    {
        [Post("/oauth2/token")]
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        Task<IApiResponse<SMSTokenModel>> GetToken([Body(BodySerializationMethod.UrlEncoded)] SMSTokenRequestModel model, [Header("Authorization")] string token);

        [Post("/api/sms")]
        Task<IApiResponse<SendSMSResponseModels>> SendSMSs([Body] SendSMSRequestModels model, [Header("Authorization")] string token);
    }
}
