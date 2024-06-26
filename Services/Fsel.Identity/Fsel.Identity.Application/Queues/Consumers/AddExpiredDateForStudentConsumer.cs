// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class AddExpiredDateForStudentConsumer : IConsumer<AddExpiredDateForStudentQueueModel>
    {
        private readonly IMediator _mediator;

        public AddExpiredDateForStudentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<AddExpiredDateForStudentQueueModel> context)
        {
            var message = context?.Message;
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new AddExpiredDateForStudentCommand
            {
                StudentId = message.StudentId,
                Day = message.Day,
                Month = message.Month,
            }).ConfigureAwait(false);
        }
    }
}
