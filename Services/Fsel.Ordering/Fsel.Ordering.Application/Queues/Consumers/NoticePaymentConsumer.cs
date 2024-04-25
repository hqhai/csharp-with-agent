using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Ordering.Application.Commands.OrderCmds;
using MassTransit;
using MediatR;

namespace Fsel.Ordering.Application.Queues.Consumers
{
    public class NoticePaymentConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public NoticePaymentConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            var message = context?.Message?.Data;
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new NoticePaymentOrderCommand()
            ).ConfigureAwait(false);
        }
    }
}
