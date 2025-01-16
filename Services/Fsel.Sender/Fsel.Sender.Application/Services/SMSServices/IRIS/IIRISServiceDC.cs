// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.IRIS
{
    using System.Threading.Tasks;
    using Fsel.Sender.Application.Services.SMSServices.IRIS.Models;
    using Refit;

    public interface IIRISServiceDC
    {
        [Post("/oauth2/token")]
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        Task<IApiResponse<IRISSMSTokenResponseModel>> GetToken([Body(BodySerializationMethod.UrlEncoded)] IRISSMSTokenRequestModel model, [Header("Authorization")] string token);

        [Post("/api/sms")]
        Task<IApiResponse<IRISSendSMSResponseModels>> SendSMSs([Body] IRISSendSMSRequestModels model, [Header("Authorization")] string token);
    }
}
