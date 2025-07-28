// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class SpeechToTextConsumer : BaseConsumer<SpeechToTextConsumerModel>
    {
        private readonly IHubContext<TranscriptHub> _transcriptHub;

        public SpeechToTextConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IHubContext<TranscriptHub> transcriptHub) : base(authContext, httpContextAccessor)
        {
            _transcriptHub = transcriptHub;
        }

        public async override Task ConsumeQueue(SpeechToTextConsumerModel? message)
        {
            if (message != null)
            {
                await _transcriptHub.GetGroup(message.UserId.ToString()).SendAsync(RealtimeSettings.TranscriptHub.Methods.Transcript, message.TranscriptFile);
            }
        }
    }
}
