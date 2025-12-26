// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers.Test
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs.Test;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class TestAIFeedBackConsumer : BaseConsumer<SubmitTestResponseModel>
    {
        private readonly IHubContext<TestWritingHub> _hubContext;

        public TestAIFeedBackConsumer(IHubContext<TestWritingHub> hubContext,
            AuthContext authContext,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _hubContext = hubContext;
        }

        public override async Task ConsumeQueue(SubmitTestResponseModel? message)
        {
            if (message != null)
            {
                var testResultId = message.TestResultId.ToString();
                await _hubContext.GetGroup(testResultId!).SendAsync(RealtimeSettings.TestWritingAIFeedBackHub.Methods.TestWritingAIFeedBackHub, message);
            }
        }
    }
}
