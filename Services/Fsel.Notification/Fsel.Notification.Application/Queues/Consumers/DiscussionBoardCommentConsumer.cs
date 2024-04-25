using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Notification.Application.Commands;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class DiscussionBoardCommentConsumer : BaseConsumer<DiscussionBoardQueueModel>
    {
        private readonly IMediator _mediator;

        public DiscussionBoardCommentConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<DiscussionBoardQueueModel>> context)
        {
            var dataReceipt = context?.Message?.Data;

            if (dataReceipt != null)
            {
                await _mediator.Send(new CreateNotificationCommand()).ConfigureAwait(false);
            }
        }
    }
}
