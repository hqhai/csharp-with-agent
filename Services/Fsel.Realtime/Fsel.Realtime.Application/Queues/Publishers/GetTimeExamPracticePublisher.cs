// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class GetTimeExamPracticePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public GetTimeExamPracticePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SetTimeExamPracticeModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.GetTimeExamPractice, request, cancellationToken);
        }
    }
}
