// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    public class WeeklyNoticePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public WeeklyNoticePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(WeeklyNoticeQueueModel request, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.WeeklyNotice, request, cancellationToken);
        }
    }
}
