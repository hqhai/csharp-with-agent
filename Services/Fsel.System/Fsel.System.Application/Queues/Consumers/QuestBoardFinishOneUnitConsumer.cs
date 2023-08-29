using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.QuestBoardStudentCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class QuestBoardFinishOneUnitConsumer : IConsumer<QuestBoardStudentQueueModel>
    {
        private readonly IMediator _mediator;

        public QuestBoardFinishOneUnitConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<QuestBoardStudentQueueModel> context)
        {
            if (context != null)
            {
                await _mediator.Send(new QuestBoardFinishOneFinalTestCommand()).ConfigureAwait(false);
            }
        }
    }
}
