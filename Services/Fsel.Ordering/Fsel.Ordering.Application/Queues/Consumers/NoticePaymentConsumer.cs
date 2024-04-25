using Fsel.Core.Base.BaseModels;
using Fsel.Ordering.Application.Commands.OrderCmds;
using MassTransit;
using MediatR;

namespace Fsel.Ordering.Application.Queues.Consumers
{
    public class NoticePaymentConsumer : Core.Base.Interfaces.IBaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public NoticePaymentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
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
