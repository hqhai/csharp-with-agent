// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class UpdateTeacherGradingInClassForumAndMockTestPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public UpdateTeacherGradingInClassForumAndMockTestPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }
        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.UpdateOcCheckInClassForumResult, new BaseQueueModel { QueueId = Guid.NewGuid().ToString() }, cancellationToken);
        }
    }
}
