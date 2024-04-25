using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Base.Managers;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class NotificationConsumer : BaseConsumer<NotificationQueueModel>
    {
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IQueueProvider _queueProvider;

        public NotificationConsumer(IHubContext<NotificationHub> notificationHubContext, IQueueProvider queueProvider, AuthContext authContext) : base(authContext)
        {
            _notificationHubContext = notificationHubContext;
            _queueProvider = queueProvider;
        }

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<NotificationQueueModel>> context)
        {
            if (context != null && context!.Message!.Data.UserIds != null)
            {
                var userIds = context.Message.Data.UserIds;
                await _notificationHubContext.GetGroups(userIds.Select(x => x.ToString()).ToList()).SendAsync(RealtimeSettings.NotificationHub.Methods.NotificationMessage, context.Message.Data);
            }
        }
    }
}
