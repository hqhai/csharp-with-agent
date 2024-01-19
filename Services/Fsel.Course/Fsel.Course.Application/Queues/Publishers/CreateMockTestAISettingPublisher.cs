// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Shared.Constants;

    public class CreateMockTestAISettingPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CreateMockTestAISettingPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(MockTestAiSettingModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.LcmsQueue.NameQueue.CreateAiGradeSetting, request, cancellationToken);
        }
    }
}
