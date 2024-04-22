using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.DailyStreakCmd;
using MassTransit;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class SyncStudentShieldForDailyStreakEveryDayConsumer : Core.Base.Interfaces.IBaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public SyncStudentShieldForDailyStreakEveryDayConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            await _mediator.Send(new SyncStudentShieldEveryDayCommand()).ConfigureAwait(false);
        }
    }
}
