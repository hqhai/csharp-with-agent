using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class LeaderBoardConsumer : IConsumer<LeaderBoardQueueModel>
    {
        private readonly IHubContext<LeaderBoardHub> _leaderBoardHubContext;

        public LeaderBoardConsumer(IHubContext<LeaderBoardHub> leaderBoardHubContext)
        {
            _leaderBoardHubContext = leaderBoardHubContext;
        }

        public async Task Consume(ConsumeContext<LeaderBoardQueueModel> context)
        {
            if (context != null)
            {
                var courseLevel = context.Message.CourseLevel.ToString();
                await _leaderBoardHubContext.Clients.Group(courseLevel!).SendAsync(RealtimeSettings.NotificationHub.Methods.NotificationMessage, context.Message);
            }
        }
    }
}
