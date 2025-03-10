// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Shared.Constants;

    public class SavePlacementTestAnswersPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SavePlacementTestAnswersPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CreatePlacementTestAnswerBySectionGroupCommandModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.SavePlacementTestAnswers, request, cancellationToken);
        }
    }
}
