using Fsel.Core.Base.BaseModels;
using Fsel.Training.Application.Commands.ClassLiveCmd;
using MassTransit;
using MassTransit.Mediator;

namespace Fsel.Training.Application.Queues.Consumers
{
    public class UpdateClassLiveAssignmentConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateClassLiveAssignmentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            await _mediator.Send(new UpdateClassLiveAssignmentCommand()).ConfigureAwait(false);
        }
    }
}
