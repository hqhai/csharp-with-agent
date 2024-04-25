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

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<QuestBoardQueueModel>> context)
        {
            if (context != null)
            {
                await _mediator.Send(new QuestBoardStudentCommand
                {
                    AchievedPoints = context.Message.Data.AchievedPoint,
                    StudentId = context.Message.Data.StudentId,
                    Categories = context.Message.Data.Categories,
                    ObjectId = context.Message.Data.ObjectId,
                    CourseId = context.Message.Data.CourseId,
                }).ConfigureAwait(false);
            }
        }
    }
}
