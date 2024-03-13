using Fsel.Ordering.Application.Commands.OrderCmds;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;

namespace Fsel.Ordering.Application.Queues.Consumers
{
    public class NoticePaymentConsumer : IConsumer<NoticePaymentModel>
    {
        private readonly IMediator _mediator;

        public NoticePaymentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<NoticePaymentModel> context)
        {
            var message = context?.Message;
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new NoticePaymentOrderCommand()
            ).ConfigureAwait(false);
        }
    }
}
