// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CreateClassForumResultPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CreateClassForumResultPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(NotificationQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.LmsCourseQueue.NameQueue.SendNotification, new NotificationQueueModel
            {
                ObjectId = request.ObjectId,
                Message = request.Message,
                Link = request.Link,
                Type = request.Type,
                Content = request.Content,
                ParamsMessage = request.ParamsMessage,
                Roles = request.Roles
            }, cancellationToken);
        }
    }
}
