using Fsel.Core.Base;
using Fsel.Ordering.Application.Commands.OrderCmds;
using Fsel.Shared.Models.ShareModels;
using MediatR;

namespace Fsel.Ordering.Application.Queues.Consumers
{
    public class CreateOrderConsumer : BaseConsumer<CreateOrderQueueModel>
    {
        private readonly IMediator _mediator;

        public CreateOrderConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(CreateOrderQueueModel? message)
        {
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
                CodeCourse = message.CodeCourse,
                UserId = message.UserId,
            }).ConfigureAwait(false);
        }
    }
}
