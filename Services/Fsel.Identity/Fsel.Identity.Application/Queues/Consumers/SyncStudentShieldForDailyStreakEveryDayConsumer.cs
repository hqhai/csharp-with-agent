using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.DailyStreakCmd;
using MassTransit;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class SyncStudentShieldForDailyStreakEveryDayConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public SyncStudentShieldForDailyStreakEveryDayConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            await _mediator.Send(new UpdateStudentsDailyStreakCommand()).ConfigureAwait(false);
        }
    }
}
