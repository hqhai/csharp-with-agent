using Fsel.Notification.Application.Commands;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class DiscussionBoardCommentConsumer : IConsumer<DiscussionBoardQueueModel>
    {
        private readonly IMediator _mediator;

        public DiscussionBoardCommentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<DiscussionBoardQueueModel> context)
        {
            var dataReceipt = context?.Message;

            if (dataReceipt != null)
            {
                await _mediator.Send(new CreateNotificationCommand()).ConfigureAwait(false);
            }
        }
    }
}
