// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Models;
    using Microsoft.AspNetCore.SignalR;

    public class QueueSocketTestConsumer : BaseConsumer<QueueTestModel>
    {
        private readonly IHubContext<TestHub> _chatBotHubContext;

        public QueueSocketTestConsumer(IHubContext<TestHub> chatBotHubContext, AuthContext authContext) : base(authContext)
        {
            _chatBotHubContext = chatBotHubContext;
        }

        public override async Task ConsumeQueue(QueueTestModel? message)
        {
            if (message != null)
            {
                var chatBotId = "984A2357-3EC7-4F4E-88C6-99370CDEB027";
                await _chatBotHubContext.GetGroup(chatBotId!).SendAsync("Test", message);
            }
        }
    }
}
