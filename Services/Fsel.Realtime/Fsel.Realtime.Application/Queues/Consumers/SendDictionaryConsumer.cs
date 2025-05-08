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

    public class SendDictionaryConsumer : BaseConsumer<DictionaryQueueModel>
    {
        private readonly IHubContext<DictionaryHub> _dictionaryHub;
        private readonly AuthContext _authContext;

        public SendDictionaryConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IHubContext<DictionaryHub> dictionaryHub) : base(authContext, httpContextAccessor)
        {
            _dictionaryHub = dictionaryHub;
            _authContext = authContext;
        }

        public override async Task ConsumeQueue(DictionaryQueueModel? message)
        {
            if (message != null)
            {
                await _dictionaryHub.GetGroup(_authContext.CurrentUserId.ToString()).SendAsync(RealtimeSettings.SendDictionaryHub.Methods.GetDictionary, message);
            }
        }
    }
}
