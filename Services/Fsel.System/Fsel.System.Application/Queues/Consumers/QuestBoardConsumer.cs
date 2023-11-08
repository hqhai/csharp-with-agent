using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.QuestBoardStudentCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class QuestBoardConsumer : IConsumer<QuestBoardQueueModel>
    {
        private readonly IMediator _mediator;

        public QuestBoardConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<QuestBoardQueueModel> context)
        {
            if (context != null)
            {
                await _mediator.Send(new QuestBoardStudentCommand
                {
                    AchievedPoints = context.Message.AchievedPoint,
                    StudentId = context.Message.StudentId,
                    Categories = context.Message.Categories,
                    ObjectId = context.Message.ObjectId,
                    CourseId = context.Message.CourseId,
                }).ConfigureAwait(false);
            }
        }
    }
}
