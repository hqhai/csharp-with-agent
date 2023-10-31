// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class NotificationMessagePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public NotificationMessagePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(NotificationQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.SendNotification, new NotificationQueueModel
            {
                ObjectId = request.ObjectId,
                Message = request.Message,
                UserId = request.UserId,
                Link = request.Link,
                Type = request.Type,
                Content = request.Content,
                ParamsMessage = request.ParamsMessage,
                ParamsLink = request.ParamsLink,
                Roles = request.Roles,
                SenderId = request.SenderId,
                PlatformCode = request.PlatformCode,
            }, cancellationToken);
        }
    }
}
