using Fsel.Core.Base.BaseModels;
using Fsel.Core.Base.Interfaces;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class DiscussionBoardConsumer : IBaseConsumer<DiscussionBoardQueueModel>
    {
        private readonly IHubContext<DiscussionBoardHub> _discussionBoardHubContext;
        private readonly IQueueProvider _queueProvider;

        public DiscussionBoardConsumer(IHubContext<DiscussionBoardHub> discussionBoardHubContext, IQueueProvider queueProvider)
        {
            _discussionBoardHubContext = discussionBoardHubContext;
            _queueProvider = queueProvider;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<DiscussionBoardQueueModel>> context)
        {
            if (context != null)
            {
                await _discussionBoardHubContext.Clients.All.SendAsync(RealtimeSettings.DiscussionBoardHub.Methods.CommentLikeMessage, context.Message);

                try
                {
                    _queueProvider.Publish(RealtimeSettings.DiscussionBoardHub.Methods.CommentLikeMessage, RealtimeSettings.DiscussionBoardHub.Methods.CommentLikeMessage, context.Message);
                }
                catch { }
            }
        }
    }
}
