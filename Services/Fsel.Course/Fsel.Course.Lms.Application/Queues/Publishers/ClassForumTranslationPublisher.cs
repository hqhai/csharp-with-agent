// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Models;

    public class ClassForumTranslationPublisher
    {
        private readonly IQueueProvider _queueProvider;

        private const string QueueName = QueueSettings.LmsQueue.NameQueue.ClassForumTranslationRequest;

        public ClassForumTranslationPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ClassForumTranslationQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueName, request, cancellationToken);
        }
    }
}
