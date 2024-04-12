using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class MockTestAIFeedBackConsumer : IConsumer<SubmitMockTestResponseModel>
    {
        private readonly IHubContext<MockTestWritingHub> _classForumFeedBackHubContext;

        public MockTestAIFeedBackConsumer(IHubContext<MockTestWritingHub> classForumAIFeedBackHubContext)
        {
            _classForumFeedBackHubContext = classForumAIFeedBackHubContext;
        }

        public async Task Consume(ConsumeContext<SubmitMockTestResponseModel> context)
        {
            if (context != null)
            {
                var mockTestResultId = context.Message.MockTestResultId.ToString();
                await _classForumFeedBackHubContext.GetGroup(mockTestResultId!).SendAsync(RealtimeSettings.MockTestWritingAIFeedBackHub.Methods.MockTestWritingAIFeedBack, context.Message);
            }
        }
    }
}
