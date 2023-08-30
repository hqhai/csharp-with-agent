using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.QuestBoardStudentCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class QuestBoardFinishOneFinalTestConsumer : IConsumer<QuestBoardStudentQueueModel>
    {
        private readonly IMediator _mediator;

        public QuestBoardFinishOneFinalTestConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<QuestBoardStudentQueueModel> context)
        {
            if (context != null)
            {
                await _mediator.Send(new QuestBoardFinishOneFinalTestCommand
                {
                    ObjectId = context.Message.ObjectId,
                    QuestBoardCategory = context.Message.QuestBoardCategory,
                    QuestBoardType = context.Message.QuestBoardType,
                    StudentId = context.Message.StudentId,
                }).ConfigureAwait(false);
            }
        }
    }
}
