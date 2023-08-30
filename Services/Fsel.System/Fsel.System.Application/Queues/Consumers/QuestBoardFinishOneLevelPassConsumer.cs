using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.QuestBoardStudentCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class QuestBoardFinishOneLevelPassConsumer : IConsumer<QuestBoardStudentQueueModel>
    {
        private readonly IMediator _mediator;

        public QuestBoardFinishOneLevelPassConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<QuestBoardStudentQueueModel> context)
        {
            if (context != null)
            {
                await _mediator.Send(new QuestBoardFinishOneLevelPassCommand
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
