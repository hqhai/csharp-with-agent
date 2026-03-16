// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class AITranslationResultConsumer : BaseConsumer<AITranslationResultModel>
    {
        private readonly IHubContext<TranslationHub> _translationHubContext;

        private const string HubMethodName = RealtimeSettings.TranslationHub.Methods.TranslationResultHub;

        public AITranslationResultConsumer(IHubContext<TranslationHub> translationHubContext,
                                           AuthContext authContext,
                                           IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _translationHubContext = translationHubContext;
        }

        public override async Task ConsumeQueue(AITranslationResultModel? message)
        {
            if (message == null)
            {
                return;
            }

            await SendResultToClientAsync(message);
        }

        #region Private Methods

        private async Task SendResultToClientAsync(AITranslationResultModel message)
        {
            await _translationHubContext.Clients.Group(message.ClassForumDetailResultId.ToString()).SendAsync(HubMethodName, message);
        }

        #endregion Private Methods
    }
}
