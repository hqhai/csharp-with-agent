// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class CreateClassForumResultPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CreateClassForumResultPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ClassForumResult? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.LmsCourseQueue.NameQueue.CreateClassForumResult, new CreateClassForumResultQueueModel
            {
                ObjectId = request.Id,
                Role = EnumRole.CSO
            }, cancellationToken);
        }
    }
}
