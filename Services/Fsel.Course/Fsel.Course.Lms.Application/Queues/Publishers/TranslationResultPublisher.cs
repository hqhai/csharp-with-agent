// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class TranslationResultPublisher
    {
        private readonly IQueueProvider _queueProvider;

        private const string QueueName = QueueSettings.RealtimeQueue.NameQueue.AITranslationResponse;

        public TranslationResultPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(AITranslationResultModel? result, CancellationToken cancellationToken)
        {
            if (IsNullResult(result))
            {
                return;
            }

            await _queueProvider.Publish(QueueName, result, cancellationToken);
        }

        #region Private Methods

        private bool IsNullResult(AITranslationResultModel? result)
        {
            return result == null;
        }

        #endregion
    }
}
