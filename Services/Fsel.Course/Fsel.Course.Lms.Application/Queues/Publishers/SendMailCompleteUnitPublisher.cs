// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using Fsel.Shared.Constants;

    public class SendMailCompleteUnitPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SendMailCompleteUnitPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SendMailCompleteUnitCommandModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.SendMailCompleteUnit, request, cancellationToken);
        }
    }
}
