using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.StudentCmd;
using MassTransit;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class UpdateStudentsDailyStreakConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateStudentsDailyStreakConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            await _mediator.Send(new UpdateStudentsDailyStreakCommand()).ConfigureAwait(false);
        }
    }
}
