using Fsel.Core.Base.BaseModels;
using Fsel.Training.Application.Commands.ClassLiveCmd;
using MassTransit;
using MediatR;

namespace Fsel.Training.Application.Queues.Consumers
{
    public class UpdateClassLiveAssignmentConsumer : Core.Base.Interfaces.IBaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateClassLiveAssignmentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            await _mediator.Send(new UpdateClassLiveAssignmentCommand()).ConfigureAwait(false);
        }
    }
}
