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

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<QuestBoardStudentQueueModel>> context)
        {
            if (context != null)
            {
                await _mediator.Send(new QuestBoardFinishCommand
                {
                    ObjectId = context.Message.Data.ObjectId,
                    QuestBoardCategory = context.Message.Data.QuestBoardCategory,
                    QuestBoardType = context.Message.Data.QuestBoardType,
                    StudentId = context.Message.Data.StudentId,
                }).ConfigureAwait(false);
            }
        }
    }
}
