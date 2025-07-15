using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class MockTestAISpeakingConsumer : BaseConsumer<SubmitAiSpeakingResponseModel>
    {
        private readonly IHubContext<MockTestSpeakingHub> _aISpeakingHub;

        public MockTestAISpeakingConsumer(IHubContext<MockTestSpeakingHub> aISpeakingHub, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _aISpeakingHub = aISpeakingHub;
        }

        public override async Task ConsumeQueue(SubmitAiSpeakingResponseModel? message)
        {
            if (message != null)
            {
                var mockTestResultId = message.MockTestResultId.ToString();
                await _aISpeakingHub.GetGroup(mockTestResultId!).SendAsync(RealtimeSettings.MockTestSpeakingAIFeedBackHub.Methods.MockTestSpeakingAIFeedBack, message);
            }
        }
    }
}
