using Fsel.Core.Base;
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

        public MockTestAIFeedBackConsumer(IHubContext<MockTestWritingHub> classForumAIFeedBackHubContext, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _classForumFeedBackHubContext = classForumAIFeedBackHubContext;
        }

        public override async Task ConsumeQueue(SubmitMockTestResponseModel? message)
        {
            if (message != null)
            {
                var mockTestResultId = message.MockTestResultId.ToString();
                await _classForumFeedBackHubContext.GetGroup(mockTestResultId!).SendAsync(RealtimeSettings.MockTestWritingAIFeedBackHub.Methods.MockTestWritingAIFeedBack, message);
            }
        }
    }
}
