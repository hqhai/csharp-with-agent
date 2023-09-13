using Fsel.Ordering.Application.Commands.OrderCmds;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;

namespace Fsel.Ordering.Application.Queues.Consumers
{
    public class CreateOrderConsumer : IConsumer<CreateOrderQueueModel>
    {
        private readonly IMediator _mediator;

        public CreateOrderConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<CreateOrderQueueModel> context)
        {
            var message = context?.Message;
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new CreateOrderCommand
            {
                PaymentMethod = message.PaymentMethod,
                FullName = message.FullName,
                Address = message.Address,
                Country = message.Country,
                CourseId = message.CourseId,
                CourseLevel = message.CourseLevel,
                CodeCourse = message.CodeCourse
            }).ConfigureAwait(false);
        }
    }
}
