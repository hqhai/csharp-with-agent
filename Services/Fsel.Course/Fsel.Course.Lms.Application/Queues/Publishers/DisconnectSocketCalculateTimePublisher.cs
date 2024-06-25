// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class DisconnectSocketCalculateTimePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public DisconnectSocketCalculateTimePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SetTimeModuleModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.DisconnectSocketCalculateTime, request, cancellationToken);
        }
    }
}
