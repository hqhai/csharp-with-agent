// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class SendSemanticDictionaryConsumer : BaseConsumer<SemanticDictionaryQueueModel>
    {
        private readonly IHubContext<SemanticDictionaryHub> _hubContext;

        public SendSemanticDictionaryConsumer(
            AuthContext authContext,
            IHttpContextAccessor httpContextAccessor,
            IHubContext<SemanticDictionaryHub> hubContext)
            : base(authContext, httpContextAccessor)
        {
            _hubContext = hubContext;
        }

        public override async Task ConsumeQueue(SemanticDictionaryQueueModel? message)
        {
            if (message == null || string.IsNullOrEmpty(message.UserId))
            {
                return;
            }

            // Send result to specific user group
            await _hubContext.GetGroup(message.UserId).SendAsync(
                RealtimeSettings.SemanticDictionaryHub.Methods.SearchSemanticDictionaryHub,
                message.Result);
        }
    }
}
