// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models;
    using Fsel.Shared.Models.ShareModels;

    public class QueueSocketTestPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public QueueSocketTestPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(QueueTestModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.QueueSocketTest, request, cancellationToken);
        }
    }
}
