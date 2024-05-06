using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.QuestBoardStudentCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class QuestBoardConsumer : BaseConsumer<QuestBoardQueueModel>
    {
        private readonly IMediator _mediator;

        public QuestBoardConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(QuestBoardQueueModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new QuestBoardStudentCommand
                {
                    AchievedPoints = message.AchievedPoint,
                    StudentId = message.StudentId,
                    Categories = message.Categories,
                    ObjectId = message.ObjectId,
                    CourseId = message.CourseId,
                }).ConfigureAwait(false);
            }
        }
    }
}
