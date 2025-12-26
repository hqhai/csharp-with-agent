// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers.Test
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs.Test;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class TestAISpeakingConsumer : BaseConsumer<SubmitTestAiSpeakingResponseModel>
    {
        private readonly IHubContext<TestSpeakingHub> _aISpeakingHub;

        public TestAISpeakingConsumer(IHubContext<TestSpeakingHub> aISpeakingHub, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _aISpeakingHub = aISpeakingHub;
        }

        public override async Task ConsumeQueue(SubmitTestAiSpeakingResponseModel? message)
        {
            if (message != null)
            {
                var testResultId = message.TestResultId.ToString();
                await _aISpeakingHub.GetGroup(testResultId!).SendAsync(RealtimeSettings.TestSpeakingAIFeedBackHub.Methods.TestSpeakingAIFeedBack, message);
            }
        }
    }
}
