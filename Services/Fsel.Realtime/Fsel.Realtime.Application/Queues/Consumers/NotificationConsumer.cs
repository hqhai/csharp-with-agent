using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class NotificationConsumer : IConsumer<NotificationQueueModel>
    {
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public NotificationConsumer(IHubContext<NotificationHub> notificationHubContext)
        {
            _notificationHubContext = notificationHubContext;
        }

        public async Task Consume(ConsumeContext<NotificationQueueModel> context)
        {
            if (context != null)
            {
                var userId = context.Message.UserId.ToString();
                if (userId != null && context!.Message!.UserIds!.Count == 0)
                {
                    await _notificationHubContext.Clients.Group(userId!).SendAsync(RealtimeSettings.NotificationHub.Methods.NotificationMessage, context.Message);
                }
                else if (context!.Message!.UserIds!.Count > 0)
                {
                    var userIds = context.Message.UserIds;
                    await _notificationHubContext.Clients.Groups(userIds).SendAsync(RealtimeSettings.NotificationHub.Methods.NotificationMessage, context.Message);
                }
            }
        }
    }
}
