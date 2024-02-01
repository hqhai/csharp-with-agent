using Fsel.Core.Base.Interfaces;
using Fsel.Core.Extensions;
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
        private readonly IQueueProvider _queueProvider;

        public LeaderBoardConsumer(IHubContext<LeaderBoardHub> leaderBoardHubContext, IQueueProvider queueProvider)
        {
            _leaderBoardHubContext = leaderBoardHubContext;
            _queueProvider = queueProvider;
        }

        public async Task Consume(ConsumeContext<LeaderBoardQueueModel> context)
        {
            if (context != null)
            {
                var courseLevel = context.Message.CourseLevel.ToString();
                await _leaderBoardHubContext.GetGroup(courseLevel!).SendAsync(RealtimeSettings.LeaderBoardHub.Methods.LeaderBoardMessage, context.Message);

                try
                {
                    _queueProvider.Publish(RealtimeSettings.LeaderBoardHub.Methods.LeaderBoardMessage, courseLevel, context.Message);
                }
                catch { }
            }
        }
    }
}
