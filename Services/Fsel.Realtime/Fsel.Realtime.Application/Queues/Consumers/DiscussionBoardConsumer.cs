using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class DiscussionBoardConsumer : IConsumer<DiscussionBoardQueueModel>
    {
        private readonly IHubContext<DiscussionBoardHub> _discussionBoardHubContext;

        public DiscussionBoardConsumer(IHubContext<DiscussionBoardHub> discussionBoardHubContext)
        {
            _discussionBoardHubContext = discussionBoardHubContext;
        }

        public async Task Consume(ConsumeContext<DiscussionBoardQueueModel> context)
        {
            if (context != null)
            {
                await _discussionBoardHubContext.Clients.All.SendAsync(RealtimeSettings.DiscussionBoardHub.Methods.CommentLikeMessage, context.Message);
            }
        }
    }
}
