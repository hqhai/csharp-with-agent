// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class AddExpiredDateForStudentPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public AddExpiredDateForStudentPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(AddExpiredDateForStudentQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.AddExpiredDateForStudent, request, cancellationToken);
        }
    }
}
