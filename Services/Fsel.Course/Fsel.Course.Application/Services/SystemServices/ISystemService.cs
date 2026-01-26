// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.SystemServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Services.SystemServices.CommandModels;
    using Fsel.Course.Application.Services.SystemServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ISystemService
    {
        [Post("/v1/chat-bot-config/unit-chatbot-configs")]
        Task<IApiResponse<MethodResult<IList<UnitChatbotStatusModel>>>> GetUnitChatbotsStatus([Body] GetUnitChatbotConfigsQueryModel query);

        [Post("/v1/chat-bot-config")]
        Task<IApiResponse<MethodResult<ChatbotConfigModel>>> SaveChatBotConfigAsync([Body] SaveChatbotConfigCommandModel cmd);

        [Delete("/v1/chat-bot-config/{unitId}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteChatbotConfigAsync([FromRoute] Guid unitId);
    }
}
