// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Models.CommandModels.QuestionExplanationErrors;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class ErrorExplainPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ErrorExplainPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CreateQuestionExplanationErrorCommandModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.ErrorExplainGgSheet, request, cancellationToken);
        }
    }
}
