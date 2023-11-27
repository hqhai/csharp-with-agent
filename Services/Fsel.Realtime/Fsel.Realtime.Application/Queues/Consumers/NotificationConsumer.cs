using Fsel.Core.Extensions;
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
            if (context != null && context!.Message!.UserIds != null)
            {
                var userIds = context.Message.UserIds;
                await _notificationHubContext.GetGroups(userIds.Select(x => x.ToString()).ToList()).SendAsync(RealtimeSettings.NotificationHub.Methods.NotificationMessage, context.Message);
            }
        }
    }
}
