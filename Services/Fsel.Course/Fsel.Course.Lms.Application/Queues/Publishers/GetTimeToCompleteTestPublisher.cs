// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class GetTimeToCompleteTestPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public GetTimeToCompleteTestPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SetTimeToCompleteTestModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.CompleteTestWhenTimeOut, request, cancellationToken);
        }
    }
}
