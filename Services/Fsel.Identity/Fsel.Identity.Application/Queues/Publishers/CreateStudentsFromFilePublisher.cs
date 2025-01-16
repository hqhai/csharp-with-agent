// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CreateStudentsFromFilePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CreateStudentsFromFilePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CreateStudentsToEventFromByteModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.CreateStudentsFromFile, request, cancellationToken);
        }
    }
}
