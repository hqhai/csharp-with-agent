// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class DeleteClassForumByFlagPublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly IFlagRepository _flagRepository;

        public DeleteClassForumByFlagPublisher(IQueueProvider queueProvider, IFlagRepository flagRepository)
        {
            _queueProvider = queueProvider;
            _flagRepository = flagRepository;
        }

        public async Task Publish(IList<Flag> models, CancellationToken cancellationToken)
        {
            if (models == null || !models.Any())
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.FlagClassForumResult, new FlagQueueModel
            {
                ObjectIds = models.Where(x => x.ObjectId.HasValue).Select(x => x.ObjectId!.Value).ToList(),
                Type = models.FirstOrDefault()!.Type,
                Status = models.FirstOrDefault()!.Status,
            }, cancellationToken);
        }
    }
}
