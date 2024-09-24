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

        public DiscussionBoardCommentConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(DiscussionBoardQueueModel? message)
        {
            var dataReceipt = message;

            if (dataReceipt != null)
            {
                await _mediator.Send(new CreateNotificationCommand()).ConfigureAwait(false);
            }
        }
    }
}
