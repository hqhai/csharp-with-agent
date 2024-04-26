using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.QuestBoardStudentCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class QuestBoardFinishConsumer : BaseConsumer<QuestBoardStudentQueueModel>
    {
        private readonly IMediator _mediator;

        public QuestBoardFinishConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(QuestBoardStudentQueueModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new QuestBoardFinishCommand
                {
                    ObjectId = message.ObjectId,
                    QuestBoardCategory = message.QuestBoardCategory,
                    QuestBoardType = message.QuestBoardType,
                    StudentId = message.StudentId,
                }).ConfigureAwait(false);
            }
        }
    }
}
