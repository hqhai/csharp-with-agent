using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class LeaderBoardConsumer : BaseConsumer<LeaderBoardQueueModel>
    {
        private readonly IHubContext<LeaderBoardHub> _leaderBoardHubContext;
        private readonly IQueueProvider _queueProvider;

        public LeaderBoardConsumer(IHubContext<LeaderBoardHub> leaderBoardHubContext, IQueueProvider queueProvider, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _leaderBoardHubContext = leaderBoardHubContext;
            _queueProvider = queueProvider;
        }

        public override async Task ConsumeQueue(LeaderBoardQueueModel? message)
        {
            if (message != null)
            {
                var courseLevel = message.CourseLevel.ToString();
                await _leaderBoardHubContext.GetGroup(courseLevel!).SendAsync(RealtimeSettings.LeaderBoardHub.Methods.LeaderBoardMessage, message);

                try
                {
                    _queueProvider.Publish(RealtimeSettings.LeaderBoardHub.Methods.LeaderBoardMessage, courseLevel, message);
                }
                catch { }
            }
        }
    }
}
