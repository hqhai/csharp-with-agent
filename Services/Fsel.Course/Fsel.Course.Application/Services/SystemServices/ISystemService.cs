// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.SystemServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Services.SystemServices.Models;
    using Refit;

    public interface ISystemService
    {
        [Post("/v1/chat-bot-config/unit-chatbot-configs")]
        Task<IApiResponse<MethodResult<IList<UnitChatbotStatusModel>>>> GetUnitChatbotsStatus([Body] GetUnitChatbotConfigsQueryModel query);
    }
}
