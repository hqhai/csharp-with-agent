// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SubmitMockTestCriteriaPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SubmitMockTestCriteriaPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SubmitMockTestResponseModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.MockTestWriting, request, cancellationToken);
        }
    }
}
