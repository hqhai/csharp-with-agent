using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Ordering.Application.Commands.OrderCmds;
using MediatR;

namespace Fsel.Ordering.Application.Queues.Consumers
{
    public class NoticePaymentConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public NoticePaymentConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new NoticePaymentOrderCommand()
            ).ConfigureAwait(false);
        }
    }
}
