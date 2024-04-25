using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class MockTestAIFeedBackConsumer : BaseConsumer<SubmitMockTestResponseModel>
    {
        private readonly IHubContext<MockTestWritingHub> _classForumFeedBackHubContext;

        public MockTestAIFeedBackConsumer(IHubContext<MockTestWritingHub> classForumAIFeedBackHubContext, AuthContext authContext) : base(authContext)
        {
            _classForumFeedBackHubContext = classForumAIFeedBackHubContext;
        }

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<SubmitMockTestResponseModel>> context)
        {
            if (context != null)
            {
                var mockTestResultId = context.Message.Data?.MockTestResultId.ToString();
                await _classForumFeedBackHubContext.GetGroup(mockTestResultId!).SendAsync(RealtimeSettings.MockTestWritingAIFeedBackHub.Methods.MockTestWritingAIFeedBack, context.Message);
            }
        }
    }
}
