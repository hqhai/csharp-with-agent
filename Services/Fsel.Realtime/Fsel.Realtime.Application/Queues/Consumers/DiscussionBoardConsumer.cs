using Fsel.Core.Base;
using Fsel.Core.Base.Interfaces;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class DiscussionBoardConsumer : BaseConsumer<DiscussionBoardQueueModel>
    {
        private readonly IHubContext<DiscussionBoardHub> _discussionBoardHubContext;
        private readonly IQueueProvider _queueProvider;

        public DiscussionBoardConsumer(IHubContext<DiscussionBoardHub> discussionBoardHubContext, IQueueProvider queueProvider, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _discussionBoardHubContext = discussionBoardHubContext;
            _queueProvider = queueProvider;
        }

        public override async Task ConsumeQueue(DiscussionBoardQueueModel? message)
        {
            if (message != null)
            {
                await _discussionBoardHubContext.Clients.All.SendAsync(RealtimeSettings.DiscussionBoardHub.Methods.CommentLikeMessage, message);
            }
        }
    }
}
