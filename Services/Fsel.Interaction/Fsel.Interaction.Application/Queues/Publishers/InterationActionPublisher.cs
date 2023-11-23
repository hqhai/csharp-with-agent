using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Fsel.Interaction.Application.Queues.Publishers
{
    public class InterationActionPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public InterationActionPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(InterationActionQueueModel model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.InteractionQueue.NameQueue.InteractionAction, new InterationActionQueueModel
            {
                ObjectId = model.ObjectId,
                Type = model.Type,
                UserId = model.UserId,
                Content = model.Content,
                SenderId = model.SenderId,
                ParamsLink = model.ParamsLink,
                ParamsMessage = model.ParamsMessage,
                InterationType = model.InterationType
            }, cancellationToken);
        }
    }
}
